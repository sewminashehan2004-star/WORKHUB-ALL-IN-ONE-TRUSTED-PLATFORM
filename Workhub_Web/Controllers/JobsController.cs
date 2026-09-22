using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Workhub_Web.Models;

namespace Workhub_Web.Controllers
{
    [Route("Jobs")]
    public class JobsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public JobsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(
            int? careerCategoryId,
            int? careerRoleId,
            string? search)
        {
            var model = new JobsPageViewModel
            {
                SelectedCareerCategoryId = careerCategoryId,
                SelectedCareerRoleId = careerRoleId,
                Search = search
            };

            try
            {
                model.Categories = await LoadCategoriesAsync();

                if (careerCategoryId.HasValue &&
                    !model.Categories.Any(x => x.CareerCategoryId == careerCategoryId.Value))
                {
                    careerCategoryId = null;
                    careerRoleId = null;
                    model.SelectedCareerCategoryId = null;
                    model.SelectedCareerRoleId = null;
                }

                if (careerCategoryId.HasValue)
                {
                    model.Roles = await LoadRolesAsync(careerCategoryId.Value);

                    if (careerRoleId.HasValue &&
                        !model.Roles.Any(x => x.CareerRoleId == careerRoleId.Value))
                    {
                        careerRoleId = null;
                        model.SelectedCareerRoleId = null;
                    }
                }

                model.Jobs = await LoadJobsAsync(careerCategoryId, careerRoleId, search);
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage = "Unable to connect to the WorkHub API.";
            }
            catch
            {
                model.ErrorMessage = "Unable to load available jobs.";
            }

            return View(model);
        }

        [HttpGet("Roles")]
        public async Task<IActionResult> Roles(int categoryId)
        {
            if (categoryId <= 0)
            {
                return Json(new { success = true, roles = Array.Empty<object>() });
            }

            try
            {
                var roles = await LoadRolesAsync(categoryId);

                return Json(new
                {
                    success = true,
                    roles = roles.Select(x => new
                    {
                        careerRoleId = x.CareerRoleId,
                        roleName = x.RoleName
                    })
                });
            }
            catch
            {
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new { success = false, message = "Unable to load career roles." });
            }
        }

        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var model = await LoadJobDetailsAsync(id);
                if (model == null)
                {
                    return NotFound();
                }

                model.SuccessMessage = TempData["JobApplySuccess"]?.ToString();
                model.ErrorMessage = TempData["JobApplyError"]?.ToString();

                return View(model);
            }
            catch (HttpRequestException)
            {
                return View(new JobDetailsViewModel
                {
                    JobId = id,
                    ErrorMessage = "Unable to connect to the WorkHub API."
                });
            }
        }

        [HttpGet("Apply/{id:int}")]
        public async Task<IActionResult> Apply(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Account", new
                {
                    returnUrl = Url.Action(nameof(Apply), "Jobs", new { id })
                });
            }

            if (!string.Equals(role, "RegisteredUser", StringComparison.OrdinalIgnoreCase))
            {
                TempData["JobApplyError"] = "Only registered users can apply for jobs.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                var model = await BuildApplicationModelAsync(id, token);
                if (model == null)
                {
                    return NotFound();
                }

                model.ErrorMessage = TempData["JobApplyError"]?.ToString();
                return View(model);
            }
            catch (HttpRequestException)
            {
                return View(new JobDetailsViewModel
                {
                    JobId = id,
                    ErrorMessage = "Unable to connect to the WorkHub API."
                });
            }
        }

        [HttpPost("Apply/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int id, JobDetailsViewModel model)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login", "Account", new
                {
                    returnUrl = Url.Action(nameof(Apply), "Jobs", new { id })
                });
            }

            if (!string.Equals(role, "RegisteredUser", StringComparison.OrdinalIgnoreCase))
            {
                TempData["JobApplyError"] = "Only registered users can apply for jobs.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                var freshModel = await BuildApplicationModelAsync(id, token);
                if (freshModel == null)
                {
                    return NotFound();
                }

                CopyApplicationInput(model, freshModel);
                ValidateScreeningAnswers(freshModel);

                if (!freshModel.HasJobSeekerProfile)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Create or activate your Job Seeker profile before applying for a job.");
                }

                if (!freshModel.HasPrimaryCv)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Upload a CV and set a primary CV before submitting this application.");
                }

                if (!ModelState.IsValid)
                {
                    return View(freshModel);
                }

                var client = CreateAuthenticatedClient(token);

                var profileUpdateResult = await UpdateApplicantProfileAsync(client, freshModel);
                if (!profileUpdateResult.Success)
                {
                    ModelState.AddModelError(string.Empty, profileUpdateResult.Message);
                    return View(freshModel);
                }

                var jobSeekerUpdateResult = await UpdateJobSeekerLocationAsync(client, freshModel.ApplicantLocation);
                if (!jobSeekerUpdateResult.Success)
                {
                    ModelState.AddModelError(string.Empty, jobSeekerUpdateResult.Message);
                    return View(freshModel);
                }

                var answers = freshModel.ScreeningAnswers
                    .Where(x => x.QuestionId > 0)
                    .Select(x => new
                    {
                        questionId = x.QuestionId,
                        answer = string.IsNullOrWhiteSpace(x.Answer)
                            ? null
                            : x.Answer.Trim()
                    })
                    .ToList();

                var applyResponse = await client.PostAsJsonAsync(
                    $"api/JobApplications/job/{id}/apply",
                    new
                    {
                        cvDocumentId = freshModel.PrimaryCvDocumentId,
                        coverLetter = string.IsNullOrWhiteSpace(freshModel.CoverLetter)
                            ? null
                            : freshModel.CoverLetter.Trim(),
                        screeningAnswers = answers
                    });

                if (!applyResponse.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, await GetApiMessageAsync(applyResponse));
                    return View(freshModel);
                }

                HttpContext.Session.SetString(
                    "FullName",
                    $"{freshModel.ApplicantFirstName.Trim()} {freshModel.ApplicantLastName.Trim()}".Trim());

                TempData["JobApplySuccess"] =
                    "Application submitted successfully. Your profile, primary CV and screening answers are now available to the employer.";

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage = "Unable to connect to the WorkHub API.";

                try
                {
                    var fallback = await BuildApplicationModelAsync(id, token);
                    if (fallback != null)
                    {
                        CopyApplicationInput(model, fallback);
                        fallback.ErrorMessage = model.ErrorMessage;
                        return View(fallback);
                    }
                }
                catch
                {
                    // Return the posted model if the API is unavailable during rehydration.
                }

                return View(model);
            }
        }

        private async Task<JobDetailsViewModel?> BuildApplicationModelAsync(int jobId, string token)
        {
            var model = await LoadJobDetailsAsync(jobId);
            if (model == null)
            {
                return null;
            }

            var client = CreateAuthenticatedClient(token);

            await LoadApplicantProfileAsync(client, model);
            await LoadJobSeekerContextAsync(client, model);
            await LoadScreeningQuestionsAsync(client, model);

            return model;
        }

        private async Task LoadApplicantProfileAsync(HttpClient client, JobDetailsViewModel model)
        {
            var response = await client.GetAsync("api/UserProfile/me");

            if (response.IsSuccessStatusCode)
            {
                var profile = await response.Content.ReadFromJsonAsync<ApplicantProfileResponse>();
                if (profile != null)
                {
                    model.ApplicantFirstName = profile.FirstName ?? string.Empty;
                    model.ApplicantLastName = profile.LastName ?? string.Empty;
                    model.ApplicantEmail = profile.Email ?? string.Empty;
                    model.ApplicantPhone = profile.Phone ?? string.Empty;
                    model.ApplicantLocation = profile.Location;
                    model.ApplicantNationalIdNumber = profile.NationalIdNumber;
                    return;
                }
            }

            var fullName = HttpContext.Session.GetString("FullName") ?? string.Empty;
            var nameParts = fullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

            model.ApplicantFirstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
            model.ApplicantLastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;
            model.ApplicantEmail = HttpContext.Session.GetString("Email") ?? string.Empty;
        }

        private async Task LoadJobSeekerContextAsync(HttpClient client, JobDetailsViewModel model)
        {
            var profileResponse = await client.GetAsync("api/JobSeeker/profile");

            if (profileResponse.IsSuccessStatusCode)
            {
                var profile = await profileResponse.Content.ReadFromJsonAsync<ApplicantJobSeekerProfileResponse>();
                model.HasJobSeekerProfile = profile?.IsActive ?? true;

                if (string.IsNullOrWhiteSpace(model.ApplicantLocation) &&
                    !string.IsNullOrWhiteSpace(profile?.CurrentLocation))
                {
                    model.ApplicantLocation = profile.CurrentLocation;
                }
            }
            else
            {
                model.HasJobSeekerProfile = false;
            }

            var cvsResponse = await client.GetAsync("api/jobseeker/cv");
            if (!cvsResponse.IsSuccessStatusCode)
            {
                model.PrimaryCvDocumentId = null;
                model.PrimaryCvFileName = null;
                return;
            }

            var cvs = await cvsResponse.Content.ReadFromJsonAsync<List<ApplicantCvResponse>>() ?? new();
            var primaryCv = cvs
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.UploadedAt)
                .FirstOrDefault();

            model.PrimaryCvDocumentId = primaryCv?.CVDocumentId;
            model.PrimaryCvFileName = primaryCv?.FileName;
        }

        private async Task LoadScreeningQuestionsAsync(HttpClient client, JobDetailsViewModel model)
        {
            var response = await client.GetAsync($"api/JobScreening/job/{model.JobId}");

            if (!response.IsSuccessStatusCode)
            {
                model.ScreeningQuestions = new();
                model.ScreeningAnswers = new();
                return;
            }

            model.ScreeningQuestions =
                await response.Content.ReadFromJsonAsync<List<JobScreeningQuestionViewModel>>()
                ?? new();

            model.ScreeningAnswers = model.ScreeningQuestions
                .Select(question => new JobScreeningAnswerInputViewModel
                {
                    QuestionId = question.JobScreeningQuestionId
                })
                .ToList();
        }

        private static void CopyApplicationInput(
            JobDetailsViewModel source,
            JobDetailsViewModel target)
        {
            target.ApplicantFirstName = source.ApplicantFirstName;
            target.ApplicantLastName = source.ApplicantLastName;
            target.ApplicantEmail = source.ApplicantEmail;
            target.ApplicantPhone = source.ApplicantPhone;
            target.ApplicantLocation = source.ApplicantLocation;
            target.ApplicantNationalIdNumber = source.ApplicantNationalIdNumber;
            target.CoverLetter = source.CoverLetter;

            var postedAnswers = source.ScreeningAnswers
                ?.Where(x => x.QuestionId > 0)
                .GroupBy(x => x.QuestionId)
                .ToDictionary(x => x.Key, x => x.Last().Answer)
                ?? new Dictionary<int, string?>();

            target.ScreeningAnswers = target.ScreeningQuestions
                .Select(question => new JobScreeningAnswerInputViewModel
                {
                    QuestionId = question.JobScreeningQuestionId,
                    Answer = postedAnswers.TryGetValue(question.JobScreeningQuestionId, out var answer)
                        ? answer
                        : null
                })
                .ToList();
        }

        private void ValidateScreeningAnswers(JobDetailsViewModel model)
        {
            var answerMap = model.ScreeningAnswers
                .Where(x => x.QuestionId > 0)
                .GroupBy(x => x.QuestionId)
                .ToDictionary(x => x.Key, x => x.Last().Answer?.Trim());

            foreach (var question in model.ScreeningQuestions)
            {
                answerMap.TryGetValue(question.JobScreeningQuestionId, out var answer);

                if (question.IsRequired && string.IsNullOrWhiteSpace(answer))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Please answer the required screening question: {question.QuestionText}");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(answer))
                {
                    continue;
                }

                if (string.Equals(question.QuestionType, "YesNo", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(answer, "Yes", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(answer, "No", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Please select Yes or No for: {question.QuestionText}");
                }

                if (string.Equals(question.QuestionType, "MultipleChoice", StringComparison.OrdinalIgnoreCase) &&
                    question.Options.Count > 0 &&
                    !question.Options.Any(option => string.Equals(option, answer, StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Please select a valid option for: {question.QuestionText}");
                }
            }
        }

        private async Task<(bool Success, string Message)> UpdateApplicantProfileAsync(
            HttpClient client,
            JobDetailsViewModel model)
        {
            var currentResponse = await client.GetAsync("api/UserProfile/me");
            if (!currentResponse.IsSuccessStatusCode)
            {
                return (false, "Unable to load your WorkHub profile before submitting the application.");
            }

            var current = await currentResponse.Content.ReadFromJsonAsync<ApplicantProfileResponse>();
            if (current == null)
            {
                return (false, "Unable to read your WorkHub profile.");
            }

            // Email belongs to the account and is intentionally not editable from this form.
            model.ApplicantEmail = current.Email ?? model.ApplicantEmail;

            var updateResponse = await client.PutAsJsonAsync(
                "api/UserProfile/me",
                new
                {
                    firstName = model.ApplicantFirstName.Trim(),
                    lastName = model.ApplicantLastName.Trim(),
                    about = current.About,
                    dateOfBirth = current.DateOfBirth,
                    nationalIdNumber = Clean(model.ApplicantNationalIdNumber),
                    country = current.Country,
                    location = Clean(model.ApplicantLocation),
                    phone = Clean(model.ApplicantPhone),
                    website = current.Website,
                    linkedInUrl = current.LinkedInUrl,
                    facebookUrl = current.FacebookUrl,
                    gitHubUrl = current.GitHubUrl,
                    instagramUrl = current.InstagramUrl
                });

            if (!updateResponse.IsSuccessStatusCode)
            {
                return (false, await GetApiMessageAsync(updateResponse));
            }

            return (true, string.Empty);
        }

        private async Task<(bool Success, string Message)> UpdateJobSeekerLocationAsync(
            HttpClient client,
            string? location)
        {
            var response = await client.GetAsync("api/JobSeeker/profile");
            if (!response.IsSuccessStatusCode)
            {
                return (false, "Your Job Seeker profile could not be loaded.");
            }

            var profile = await response.Content.ReadFromJsonAsync<ApplicantJobSeekerProfileResponse>();
            if (profile == null)
            {
                return (false, "Your Job Seeker profile could not be read.");
            }

            var updateResponse = await client.PutAsJsonAsync(
                "api/JobSeeker/profile",
                new
                {
                    professionalSummary = profile.ProfessionalSummary,
                    preferredJobCategory = profile.PreferredJobCategory,
                    expectedSalary = profile.ExpectedSalary,
                    currentLocation = Clean(location)
                });

            if (!updateResponse.IsSuccessStatusCode)
            {
                return (false, await GetApiMessageAsync(updateResponse));
            }

            return (true, string.Empty);
        }

        private async Task<JobDetailsViewModel?> LoadJobDetailsAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("WorkHubApi");
            var response = await client.GetAsync($"api/Jobs/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<JobDetailsViewModel>();
        }

        private HttpClient CreateAuthenticatedClient(string token)
        {
            var client = _httpClientFactory.CreateClient("WorkHubApi");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private static string? Clean(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static async Task<string> GetApiMessageAsync(HttpResponseMessage response)
        {
            var text = await response.Content.ReadAsStringAsync();

            try
            {
                using var document = JsonDocument.Parse(text);
                if (document.RootElement.TryGetProperty("message", out var message))
                {
                    return message.GetString() ?? "Request failed.";
                }
            }
            catch (JsonException)
            {
                // A non-JSON API response is handled by the friendly fallback below.
            }

            return "Request failed.";
        }

        private async Task<List<JobsCareerCategoryViewModel>> LoadCategoriesAsync()
        {
            var client = _httpClientFactory.CreateClient("WorkHubApi");
            var response = await client.GetAsync("api/CareerCatalog/categories");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<JobsCareerCategoryViewModel>>()
                   ?? new List<JobsCareerCategoryViewModel>();
        }

        private async Task<List<JobsCareerRoleViewModel>> LoadRolesAsync(int categoryId)
        {
            var client = _httpClientFactory.CreateClient("WorkHubApi");
            var response = await client.GetAsync($"api/CareerCatalog/categories/{categoryId}/roles");

            if (!response.IsSuccessStatusCode)
            {
                return new List<JobsCareerRoleViewModel>();
            }

            var result = await response.Content.ReadFromJsonAsync<JobsCareerRolesApiResponse>();
            return result?.Roles ?? new List<JobsCareerRoleViewModel>();
        }

        private async Task<List<JobsListItemViewModel>> LoadJobsAsync(
            int? careerCategoryId,
            int? careerRoleId,
            string? search)
        {
            var client = _httpClientFactory.CreateClient("WorkHubApi");
            var queryParts = new List<string>();

            if (careerCategoryId.HasValue)
            {
                queryParts.Add("careerCategoryId=" + careerCategoryId.Value);
            }

            if (careerRoleId.HasValue)
            {
                queryParts.Add("careerRoleId=" + careerRoleId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                queryParts.Add("search=" + Uri.EscapeDataString(search.Trim()));
            }

            var endpoint = "api/Jobs";
            if (queryParts.Count > 0)
            {
                endpoint += "?" + string.Join("&", queryParts);
            }

            var response = await client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<JobsListItemViewModel>>()
                   ?? new List<JobsListItemViewModel>();
        }

        private sealed class ApplicantProfileResponse
        {
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public string? Email { get; set; }
            public string? About { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public string? NationalIdNumber { get; set; }
            public string? Country { get; set; }
            public string? Location { get; set; }
            public string? Phone { get; set; }
            public string? Website { get; set; }
            public string? LinkedInUrl { get; set; }
            public string? FacebookUrl { get; set; }
            public string? GitHubUrl { get; set; }
            public string? InstagramUrl { get; set; }
        }

        private sealed class ApplicantJobSeekerProfileResponse
        {
            public string? ProfessionalSummary { get; set; }
            public string? PreferredJobCategory { get; set; }
            public decimal? ExpectedSalary { get; set; }
            public string? CurrentLocation { get; set; }
            public bool IsActive { get; set; }
        }

        private sealed class ApplicantCvResponse
        {
            public int CVDocumentId { get; set; }
            public string FileName { get; set; } = string.Empty;
            public bool IsPrimary { get; set; }
            public DateTime UploadedAt { get; set; }
        }
    }
}
