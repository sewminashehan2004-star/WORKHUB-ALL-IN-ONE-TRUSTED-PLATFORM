using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Workhub_Web.Models;

namespace Workhub_Web.Controllers
{
    [Route("Employer/Jobs")]
    public class CompanyJobsController : Controller
    {
        private readonly IHttpClientFactory
            _httpClientFactory;

        private readonly IWebHostEnvironment
            _environment;


        private const long MaxImageSize =
            5 * 1024 * 1024;


        private static readonly string[]
            AllowedImageExtensions =
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };


        public CompanyJobsController(
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment environment)
        {
            _httpClientFactory =
                httpClientFactory;

            _environment =
                environment;
        }


        // =========================================
        // MY JOBS
        //
        // GET:
        // /Employer/Jobs
        // =========================================

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            if (!IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "CompanyPortal");
            }


            var model =
                new CompanyMyJobsViewModel
                {
                    SuccessMessage =
                        TempData["CompanyJobSuccess"]
                            ?.ToString(),

                    ErrorMessage =
                        TempData["CompanyJobError"]
                            ?.ToString()
                };


            var token =
                GetCompanyToken();


            if (string.IsNullOrWhiteSpace(
                    token))
            {
                ClearCompanySession();


                return RedirectToAction(
                    "Login",
                    "CompanyPortal");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                var response =
                    await client.GetAsync(
                        "api/Jobs/company/me");


                if (
                    response.StatusCode ==
                    HttpStatusCode.Unauthorized
                    ||
                    response.StatusCode ==
                    HttpStatusCode.Forbidden
                )
                {
                    ClearCompanySession();


                    return RedirectToAction(
                        "Login",
                        "CompanyPortal");
                }


                if (!response.IsSuccessStatusCode)
                {
                    model.ErrorMessage =
                        await GetApiMessageAsync(
                            response);


                    return View(model);
                }


                var jobs =
                    await response.Content
                        .ReadFromJsonAsync<
                            List<
                                CompanyMyJobItemViewModel>>();


                model.Jobs =
                    jobs
                    ??
                    new List<
                        CompanyMyJobItemViewModel>();
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage =
                    "Unable to connect to the WorkHub API.";
            }


            return View(model);
        }


        // =========================================
        // CREATE - GET
        //
        // GET:
        // /Employer/Jobs/Create
        // =========================================

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            if (!IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "CompanyPortal");
            }


            var model =
                new CompanyJobFormViewModel
                {
                    SuccessMessage =
                        TempData["CompanyJobSuccess"]
                            ?.ToString(),

                    ErrorMessage =
                        TempData["CompanyJobError"]
                            ?.ToString()
                };


            await LoadCategoriesAsync(
                model);


            return View(model);
        }


        // =========================================
        // CREATE - POST
        // =========================================

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CompanyJobFormViewModel model)
        {
            if (!IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "CompanyPortal");
            }


            // =====================================
            // CAREER CATEGORY VALIDATION
            // =====================================

            if (model.CareerCategoryId <= 0)
            {
                ModelState.AddModelError(
                    nameof(
                        model.CareerCategoryId),
                    "Please select a career category.");
            }


            // =====================================
            // CAREER ROLE VALIDATION
            // =====================================

            if (model.CareerRoleId <= 0)
            {
                ModelState.AddModelError(
                    nameof(
                        model.CareerRoleId),
                    "Please select a career role.");
            }


            // =====================================
            // SALARY VALIDATION
            // =====================================

            if (
                model.SalaryMin.HasValue
                &&
                model.SalaryMax.HasValue
                &&
                model.SalaryMin.Value >
                model.SalaryMax.Value
            )
            {
                ModelState.AddModelError(
                    nameof(
                        model.SalaryMax),
                    "Maximum salary must be greater than minimum salary.");
            }


            // =====================================
            // CLOSING DATE VALIDATION
            // =====================================

            if (
                model.ClosingDate.HasValue
                &&
                model.ClosingDate.Value.Date <=
                DateTime.Today
            )
            {
                ModelState.AddModelError(
                    nameof(
                        model.ClosingDate),
                    "Closing date must be in the future.");
            }


            // =====================================
            // IMAGE VALIDATION
            // =====================================

            if (model.JobImage != null)
            {
                ValidateJobImage(
                    model.JobImage);
            }


            if (!ModelState.IsValid)
            {
                await ReloadFormDataAsync(
                    model);


                return View(model);
            }


            string? imagePath =
                null;


            // =====================================
            // SAVE IMAGE
            // =====================================

            if (model.JobImage != null)
            {
                try
                {
                    imagePath =
                        await SaveJobImageAsync(
                            model.JobImage);
                }
                catch
                {
                    model.ErrorMessage =
                        "Unable to save the job image.";


                    await ReloadFormDataAsync(
                        model);


                    return View(model);
                }
            }


            // =====================================
            // SELECTED SKILLS
            // =====================================

            var selectedSkills =
                model.Skills
                    .Where(x =>
                        x.Selected)
                    .ToList();


            // =====================================
            // API REQUEST
            // =====================================

            var apiRequest =
                new CompanyJobApiRequest
                {
                    Title =
                        model.Title.Trim(),

                    Description =
                        model.Description.Trim(),

                    CareerRoleId =
                        model.CareerRoleId,

                    Category =
                        null,

                    Location =
                        Clean(
                            model.Location),

                    JobType =
                        Clean(
                            model.JobType),

                    ImagePath =
                        imagePath,

                    SalaryMin =
                        model.SalaryMin,

                    SalaryMax =
                        model.SalaryMax,

                    MinimumExperienceYears =
                        model.MinimumExperienceYears,

                    EducationRequirement =
                        Clean(
                            model
                                .EducationRequirement),

                    ClosingDate =
                        model.ClosingDate,

                    Skills =
                        selectedSkills
                            .Select(x =>
                                new CompanyJobSkillApiRequest
                                {
                                    SkillId =
                                        x.SkillId,

                                    Importance =
                                        NormalizeImportance(
                                            x.Importance),

                                    RequiredYearsOfExperience =
                                        x.RequiredYearsOfExperience
                                })
                            .ToList()
                };


            var token =
                GetCompanyToken();


            if (string.IsNullOrWhiteSpace(
                    token))
            {
                DeleteJobImage(
                    imagePath);


                ClearCompanySession();


                return RedirectToAction(
                    "Login",
                    "CompanyPortal");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                var response =
                    await client.PostAsJsonAsync(
                        "api/Jobs",
                        apiRequest);


                if (
                    response.StatusCode ==
                    HttpStatusCode.Unauthorized
                    ||
                    response.StatusCode ==
                    HttpStatusCode.Forbidden
                )
                {
                    DeleteJobImage(
                        imagePath);


                    ClearCompanySession();


                    return RedirectToAction(
                        "Login",
                        "CompanyPortal");
                }


                if (!response.IsSuccessStatusCode)
                {
                    DeleteJobImage(
                        imagePath);


                    model.ErrorMessage =
                        await GetApiMessageAsync(
                            response);


                    await ReloadFormDataAsync(
                        model);


                    return View(model);
                }


                var result =
                    await response.Content
                        .ReadFromJsonAsync<
                            CompanyJobCreateApiResponse>();


                TempData["CompanyJobSuccess"] =
                    result?.Message
                    ??
                    "Job published successfully.";


                // =================================
                // AFTER CREATE -> MY JOBS
                // =================================

                return RedirectToAction(
                    nameof(Index));
            }
            catch (HttpRequestException)
            {
                DeleteJobImage(
                    imagePath);


                model.ErrorMessage =
                    "Unable to connect to the WorkHub API.";


                await ReloadFormDataAsync(
                    model);


                return View(model);
            }
        }


        // =========================================
        // CLOSE JOB
        //
        // POST:
        // /Employer/Jobs/{id}/Close
        // =========================================

        [HttpPost("{id:int}/Close")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(
            int id)
        {
            if (!IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "CompanyPortal");
            }


            if (id <= 0)
            {
                TempData["CompanyJobError"] =
                    "Invalid job.";


                return RedirectToAction(
                    nameof(Index));
            }


            var token =
                GetCompanyToken();


            if (string.IsNullOrWhiteSpace(
                    token))
            {
                ClearCompanySession();


                return RedirectToAction(
                    "Login",
                    "CompanyPortal");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                using var request =
                    new HttpRequestMessage(
                        HttpMethod.Patch,
                        $"api/Jobs/{id}/close");


                var response =
                    await client.SendAsync(
                        request);


                if (
                    response.StatusCode ==
                    HttpStatusCode.Unauthorized
                    ||
                    response.StatusCode ==
                    HttpStatusCode.Forbidden
                )
                {
                    ClearCompanySession();


                    return RedirectToAction(
                        "Login",
                        "CompanyPortal");
                }


                if (!response.IsSuccessStatusCode)
                {
                    TempData["CompanyJobError"] =
                        await GetApiMessageAsync(
                            response);


                    return RedirectToAction(
                        nameof(Index));
                }


                TempData["CompanyJobSuccess"] =
                    "Job closed successfully.";
            }
            catch (HttpRequestException)
            {
                TempData["CompanyJobError"] =
                    "Unable to connect to the WorkHub API.";
            }


            return RedirectToAction(
                nameof(Index));
        }


        // =========================================
        // DELETE JOB
        //
        // POST:
        // /Employer/Jobs/{id}/Delete
        // =========================================

        [HttpPost("{id:int}/Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
        {
            if (!IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "CompanyPortal");
            }


            if (id <= 0)
            {
                TempData["CompanyJobError"] =
                    "Invalid job.";


                return RedirectToAction(
                    nameof(Index));
            }


            var token =
                GetCompanyToken();


            if (string.IsNullOrWhiteSpace(
                    token))
            {
                ClearCompanySession();


                return RedirectToAction(
                    "Login",
                    "CompanyPortal");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                var response =
                    await client.DeleteAsync(
                        $"api/Jobs/{id}");


                if (
                    response.StatusCode ==
                    HttpStatusCode.Unauthorized
                    ||
                    response.StatusCode ==
                    HttpStatusCode.Forbidden
                )
                {
                    ClearCompanySession();


                    return RedirectToAction(
                        "Login",
                        "CompanyPortal");
                }


                if (!response.IsSuccessStatusCode)
                {
                    TempData["CompanyJobError"] =
                        await GetApiMessageAsync(
                            response);


                    return RedirectToAction(
                        nameof(Index));
                }


                TempData["CompanyJobSuccess"] =
                    "Job deleted successfully.";
            }
            catch (HttpRequestException)
            {
                TempData["CompanyJobError"] =
                    "Unable to connect to the WorkHub API.";
            }


            return RedirectToAction(
                nameof(Index));
        }


        // =========================================
        // ROLES BY CATEGORY
        //
        // GET:
        // /Employer/Jobs/Roles?categoryId=1
        // =========================================

        [HttpGet("Roles")]
        public async Task<IActionResult> Roles(
            int categoryId)
        {
            if (!IsCompanyLoggedIn())
            {
                return Unauthorized();
            }


            if (categoryId <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid career category."
                });
            }


            try
            {
                var client =
                    _httpClientFactory
                        .CreateClient(
                            "WorkHubApi");


                var response =
                    await client.GetAsync(
                        $"api/CareerCatalog/categories/{categoryId}/roles");


                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(
                        (int)response.StatusCode,
                        new
                        {
                            message =
                                await GetApiMessageAsync(
                                    response)
                        });
                }


                var result =
                    await response.Content
                        .ReadFromJsonAsync<
                            CareerRolesApiResponse>();


                if (result == null)
                {
                    return Json(
                        Array.Empty<object>());
                }


                var roles =
                    result.Roles
                        .Select(x => new
                        {
                            careerRoleId =
                                x.CareerRoleId,

                            roleName =
                                x.RoleName,

                            description =
                                x.Description,

                            baselineExperienceYears =
                                x.BaselineExperienceYears,

                            baselineEducationRequirement =
                                x.BaselineEducationRequirement
                        })
                        .ToList();


                return Json(
                    roles);
            }
            catch (HttpRequestException)
            {
                return StatusCode(
                    StatusCodes
                        .Status503ServiceUnavailable,
                    new
                    {
                        message =
                            "Unable to connect to the WorkHub API."
                    });
            }
        }


        // =========================================
        // ROLE DETAILS
        //
        // GET:
        // /Employer/Jobs/RoleDetails?roleId=1
        // =========================================

        [HttpGet("RoleDetails")]
        public async Task<IActionResult> RoleDetails(
            int roleId)
        {
            if (!IsCompanyLoggedIn())
            {
                return Unauthorized();
            }


            if (roleId <= 0)
            {
                return BadRequest();
            }


            try
            {
                var client =
                    _httpClientFactory
                        .CreateClient(
                            "WorkHubApi");


                var response =
                    await client.GetAsync(
                        $"api/CareerCatalog/roles/{roleId}");


                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(
                        (int)response.StatusCode);
                }


                var role =
                    await response.Content
                        .ReadFromJsonAsync<
                            CareerRoleDetailsApiResponse>();


                if (role == null)
                {
                    return NotFound();
                }


                return Json(new
                {
                    role.CareerRoleId,

                    role.CareerCategoryId,

                    role.CategoryName,

                    role.RoleName,

                    role.Description,

                    role.BaselineExperienceYears,

                    role.BaselineEducationRequirement,

                    skills =
                        role.Skills.Select(
                            x => new
                            {
                                x.SkillId,

                                x.SkillName,

                                x.Category,

                                importance =
                                    NormalizeImportance(
                                        x.Importance),

                                x.RecommendedYearsOfExperience
                            })
                });
            }
            catch (HttpRequestException)
            {
                return StatusCode(
                    StatusCodes
                        .Status503ServiceUnavailable);
            }
        }


        // =========================================
        // VALIDATE JOB IMAGE
        // =========================================

        private void ValidateJobImage(
            IFormFile image)
        {
            if (image.Length <= 0)
            {
                ModelState.AddModelError(
                    nameof(
                        CompanyJobFormViewModel
                            .JobImage),
                    "Please select a valid image.");


                return;
            }


            if (image.Length >
                MaxImageSize)
            {
                ModelState.AddModelError(
                    nameof(
                        CompanyJobFormViewModel
                            .JobImage),
                    "Job image must be 5 MB or smaller.");
            }


            var extension =
                Path.GetExtension(
                        image.FileName)
                    .ToLowerInvariant();


            if (!AllowedImageExtensions
                .Contains(extension))
            {
                ModelState.AddModelError(
                    nameof(
                        CompanyJobFormViewModel
                            .JobImage),
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");
            }
        }


        // =========================================
        // SAVE JOB IMAGE
        // =========================================

        private async Task<string>
            SaveJobImageAsync(
                IFormFile image)
        {
            var extension =
                Path.GetExtension(
                        image.FileName)
                    .ToLowerInvariant();


            var fileName =
                $"{Guid.NewGuid():N}{extension}";


            var webRoot =
                _environment.WebRootPath;


            if (string.IsNullOrWhiteSpace(
                    webRoot))
            {
                webRoot =
                    Path.Combine(
                        _environment.ContentRootPath,
                        "wwwroot");
            }


            var folder =
                Path.Combine(
                    webRoot,
                    "uploads",
                    "jobs");


            Directory.CreateDirectory(
                folder);


            var physicalPath =
                Path.Combine(
                    folder,
                    fileName);


            await using var stream =
                new FileStream(
                    physicalPath,
                    FileMode.Create);


            await image.CopyToAsync(
                stream);


            return
                $"/uploads/jobs/{fileName}";
        }


        // =========================================
        // DELETE UNUSED IMAGE
        // =========================================

        private void DeleteJobImage(
            string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(
                    imagePath))
            {
                return;
            }


            if (!imagePath.StartsWith(
                    "/uploads/jobs/",
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }


            try
            {
                var webRoot =
                    _environment.WebRootPath;


                if (string.IsNullOrWhiteSpace(
                        webRoot))
                {
                    return;
                }


                var fileName =
                    Path.GetFileName(
                        imagePath);


                var physicalPath =
                    Path.Combine(
                        webRoot,
                        "uploads",
                        "jobs",
                        fileName);


                if (System.IO.File.Exists(
                        physicalPath))
                {
                    System.IO.File.Delete(
                        physicalPath);
                }
            }
            catch
            {
            }
        }


        // =========================================
        // LOAD CAREER CATEGORIES
        // =========================================

        private async Task LoadCategoriesAsync(
            CompanyJobFormViewModel model)
        {
            try
            {
                var client =
                    _httpClientFactory
                        .CreateClient(
                            "WorkHubApi");


                model.Categories =
                    await client.GetFromJsonAsync<
                        List<
                            CompanyCareerCategoryViewModel>>(
                        "api/CareerCatalog/categories")
                    ??
                    new List<
                        CompanyCareerCategoryViewModel>();
            }
            catch
            {
                model.Categories =
                    new List<
                        CompanyCareerCategoryViewModel>();


                model.ErrorMessage ??=
                    "Unable to load career categories.";
            }
        }


        // =========================================
        // RELOAD CREATE FORM DATA
        // =========================================

        private async Task ReloadFormDataAsync(
            CompanyJobFormViewModel model)
        {
            await LoadCategoriesAsync(
                model);


            // =====================================
            // LOAD ROLES
            // =====================================

            if (model.CareerCategoryId > 0)
            {
                try
                {
                    var client =
                        _httpClientFactory
                            .CreateClient(
                                "WorkHubApi");


                    var result =
                        await client
                            .GetFromJsonAsync<
                                CareerRolesApiResponse>(
                            $"api/CareerCatalog/categories/{model.CareerCategoryId}/roles");


                    if (result != null)
                    {
                        model.Roles =
                            result.Roles
                                .Select(x =>
                                    new CompanyCareerRoleViewModel
                                    {
                                        CareerRoleId =
                                            x.CareerRoleId,

                                        CareerCategoryId =
                                            model.CareerCategoryId,

                                        RoleName =
                                            x.RoleName,

                                        Description =
                                            x.Description
                                    })
                                .ToList();
                    }
                }
                catch
                {
                    model.Roles =
                        new List<
                            CompanyCareerRoleViewModel>();
                }
            }


            // =====================================
            // LOAD ROLE SKILLS
            // =====================================

            if (model.CareerRoleId > 0)
            {
                try
                {
                    var client =
                        _httpClientFactory
                            .CreateClient(
                                "WorkHubApi");


                    var role =
                        await client
                            .GetFromJsonAsync<
                                CareerRoleDetailsApiResponse>(
                            $"api/CareerCatalog/roles/{model.CareerRoleId}");


                    if (role != null)
                    {
                        model.AvailableSkills =
                            role.Skills
                                .Select(x =>
                                    new CompanyJobSkillOptionViewModel
                                    {
                                        SkillId =
                                            x.SkillId,

                                        SkillName =
                                            x.SkillName,

                                        Category =
                                            x.Category,

                                        Importance =
                                            NormalizeImportance(
                                                x.Importance),

                                        RecommendedYearsOfExperience =
                                            x.RecommendedYearsOfExperience
                                    })
                                .ToList();
                    }
                }
                catch
                {
                    model.AvailableSkills =
                        new List<
                            CompanyJobSkillOptionViewModel>();
                }
            }
        }


        // =========================================
        // COMPANY LOGIN CHECK
        // =========================================

        private bool IsCompanyLoggedIn()
        {
            var role =
                HttpContext.Session
                    .GetString(
                        "Role");


            return
                !string.IsNullOrWhiteSpace(
                    GetCompanyToken())
                &&
                string.Equals(
                    role,
                    "Company",
                    StringComparison.OrdinalIgnoreCase);
        }


        // =========================================
        // GET TOKEN
        // =========================================

        private string? GetCompanyToken()
        {
            return
                HttpContext.Session
                    .GetString(
                        "JwtToken")
                ??
                HttpContext.Session
                    .GetString(
                        "CompanyJwtToken");
        }


        // =========================================
        // CLEAR SESSION
        // =========================================

        private void ClearCompanySession()
        {
            HttpContext.Session.Clear();
        }


        // =========================================
        // AUTHENTICATED API CLIENT
        // =========================================

        private HttpClient
            CreateAuthenticatedClient(
                string token)
        {
            var client =
                _httpClientFactory
                    .CreateClient(
                        "WorkHubApi");


            client
                .DefaultRequestHeaders
                .Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);


            return client;
        }


        // =========================================
        // NORMALIZE SKILL IMPORTANCE
        // =========================================

        private static string
            NormalizeImportance(
                string? importance)
        {
            return string.Equals(
                    importance,
                    "Preferred",
                    StringComparison.OrdinalIgnoreCase)
                ? "Preferred"
                : "Required";
        }


        // =========================================
        // CLEAN OPTIONAL STRING
        // =========================================

        private static string? Clean(
            string? value)
        {
            return string.IsNullOrWhiteSpace(
                    value)
                ? null
                : value.Trim();
        }


        // =========================================
        // GET API ERROR MESSAGE
        // =========================================

        private static async Task<string>
            GetApiMessageAsync(
                HttpResponseMessage response)
        {
            try
            {
                var content =
                    await response.Content
                        .ReadAsStringAsync();


                if (string.IsNullOrWhiteSpace(
                        content))
                {
                    return
                        "Unable to complete the request.";
                }


                using var document =
                    JsonDocument.Parse(
                        content);


                if (
                    document.RootElement
                        .TryGetProperty(
                            "message",
                            out var message)
                )
                {
                    return
                        message.GetString()
                        ??
                        "Unable to complete the request.";
                }


                if (
                    document.RootElement
                        .TryGetProperty(
                            "title",
                            out var title)
                )
                {
                    return
                        title.GetString()
                        ??
                        "Unable to complete the request.";
                }
            }
            catch
            {
            }


            return
                "Unable to complete the request.";
        }


        // =========================================
        // INTERNAL CAREER API MODELS
        // =========================================

        private class CareerRolesApiResponse
        {
            public List<CareerRoleApiItem>
                Roles
            { get; set; }
                = new();
        }


        private class CareerRoleApiItem
        {
            public int CareerRoleId
            {
                get;
                set;
            }


            public string RoleName
            {
                get;
                set;
            } = string.Empty;


            public string? Description
            {
                get;
                set;
            }


            public decimal?
                BaselineExperienceYears
            {
                get;
                set;
            }


            public string?
                BaselineEducationRequirement
            {
                get;
                set;
            }
        }


        private class CareerRoleDetailsApiResponse
        {
            public int CareerRoleId
            {
                get;
                set;
            }


            public int CareerCategoryId
            {
                get;
                set;
            }


            public string CategoryName
            {
                get;
                set;
            } = string.Empty;


            public string RoleName
            {
                get;
                set;
            } = string.Empty;


            public string? Description
            {
                get;
                set;
            }


            public decimal?
                BaselineExperienceYears
            {
                get;
                set;
            }


            public string?
                BaselineEducationRequirement
            {
                get;
                set;
            }


            public List<
                CareerRoleSkillApiItem>
                Skills
            { get; set; }
                = new();
        }


        private class CareerRoleSkillApiItem
        {
            public int SkillId
            {
                get;
                set;
            }


            public string SkillName
            {
                get;
                set;
            } = string.Empty;


            public string? Category
            {
                get;
                set;
            }


            public string Importance
            {
                get;
                set;
            } = "Required";


            public decimal?
                RecommendedYearsOfExperience
            {
                get;
                set;
            }
        }
    }
}