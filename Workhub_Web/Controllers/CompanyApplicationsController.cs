using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Workhub_Web.Models;

namespace Workhub_Web.Controllers
{
    [Route("Employer/Applications")]
    public class CompanyApplicationsController : Controller
    {
        private readonly IHttpClientFactory
            _httpClientFactory;


        public CompanyApplicationsController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory =
                httpClientFactory;
        }


        // =========================================
        // COMPANY APPLICATIONS
        //
        // GET:
        // /Employer/Applications
        //
        // Optional:
        // /Employer/Applications?jobId=5
        // =========================================

        [HttpGet("")]
        public async Task<IActionResult> Index(
            int? jobId)
        {
            if (!IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "CompanyPortal");
            }


            var model =
                new CompanyApplicationsViewModel
                {
                    SuccessMessage =
                        TempData["CompanyApplicationSuccess"]
                            ?.ToString(),

                    ErrorMessage =
                        TempData["CompanyApplicationError"]
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


                // =====================================
                // LOAD COMPANY JOBS
                // =====================================

                var jobsResponse =
                    await client.GetAsync(
                        "api/Jobs/company/me");


                if (IsUnauthorized(
                        jobsResponse))
                {
                    ClearCompanySession();


                    return RedirectToAction(
                        "Login",
                        "CompanyPortal");
                }


                if (!jobsResponse.IsSuccessStatusCode)
                {
                    model.ErrorMessage =
                        await GetApiMessageAsync(
                            jobsResponse);


                    return View(model);
                }


                model.Jobs =
                    await jobsResponse.Content
                        .ReadFromJsonAsync<
                            List<
                                CompanyApplicationJobViewModel>>()
                    ??
                    new List<
                        CompanyApplicationJobViewModel>();


                // =====================================
                // COMPANY HAS NO JOBS
                // =====================================

                if (!model.Jobs.Any())
                {
                    return View(model);
                }


                // =====================================
                // SELECT JOB
                //
                // If no job was selected,
                // choose the newest job.
                // =====================================

                CompanyApplicationJobViewModel?
                    selectedJob;


                if (jobId.HasValue)
                {
                    selectedJob =
                        model.Jobs
                            .FirstOrDefault(x =>
                                x.JobId ==
                                jobId.Value);


                    if (selectedJob == null)
                    {
                        model.ErrorMessage =
                            "The selected job could not be found.";


                        return View(model);
                    }
                }
                else
                {
                    selectedJob =
                        model.Jobs
                            .OrderByDescending(x =>
                                x.CreatedAt)
                            .First();
                }


                model.SelectedJobId =
                    selectedJob.JobId;


                model.SelectedJobTitle =
                    selectedJob.Title;


                // =====================================
                // LOAD APPLICANTS
                // =====================================

                var applicantsResponse =
                    await client.GetAsync(
                        $"api/JobApplications/company/job/{selectedJob.JobId}");


                if (IsUnauthorized(
                        applicantsResponse))
                {
                    ClearCompanySession();


                    return RedirectToAction(
                        "Login",
                        "CompanyPortal");
                }


                if (!applicantsResponse
                    .IsSuccessStatusCode)
                {
                    model.ErrorMessage =
                        await GetApiMessageAsync(
                            applicantsResponse);


                    return View(model);
                }


                var applicantsResult =
                    await applicantsResponse.Content
                        .ReadFromJsonAsync<
                            CompanyJobApplicantsApiResponse>();


                if (applicantsResult != null)
                {
                    model.SelectedJobId =
                        applicantsResult.Job.JobId;


                    model.SelectedJobTitle =
                        applicantsResult.Job.Title;


                    model.TotalApplicants =
                        applicantsResult.TotalApplicants;


                    model.Applicants =
                        applicantsResult.Applicants
                        ??
                        new List<
                            CompanyApplicantItemViewModel>();
                }


                return View(model);
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage =
                    "Unable to connect to the WorkHub API.";


                return View(model);
            }
        }


        // =========================================
        // APPLICATION DETAILS
        //
        // GET:
        // /Employer/Applications/10
        // =========================================

        [HttpGet("{applicationId:int}")]
        public async Task<IActionResult> Details(
            int applicationId)
        {
            if (!IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "CompanyPortal");
            }


            if (applicationId <= 0)
            {
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
                    await client.GetAsync(
                        $"api/JobApplications/company/application/{applicationId}");


                if (IsUnauthorized(
                        response))
                {
                    ClearCompanySession();


                    return RedirectToAction(
                        "Login",
                        "CompanyPortal");
                }


                if (response.StatusCode ==
                    HttpStatusCode.NotFound)
                {
                    TempData["CompanyApplicationError"] =
                        await GetApiMessageAsync(
                            response);


                    return RedirectToAction(
                        nameof(Index));
                }


                if (!response.IsSuccessStatusCode)
                {
                    TempData["CompanyApplicationError"] =
                        await GetApiMessageAsync(
                            response);


                    return RedirectToAction(
                        nameof(Index));
                }


                var model =
                    await response.Content
                        .ReadFromJsonAsync<
                            CompanyApplicationDetailsViewModel>();


                if (model == null)
                {
                    TempData["CompanyApplicationError"] =
                        "Unable to load application details.";


                    return RedirectToAction(
                        nameof(Index));
                }


                model.SuccessMessage =
                    TempData["CompanyApplicationSuccess"]
                        ?.ToString();


                model.ErrorMessage =
                    TempData["CompanyApplicationError"]
                        ?.ToString();


                var screeningResponse =
                    await client.GetAsync(
                        $"api/JobScreening/company/application/{applicationId}/answers");

                if (screeningResponse.IsSuccessStatusCode)
                {
                    model.ScreeningAnswers =
                        await screeningResponse.Content
                            .ReadFromJsonAsync<List<CompanyScreeningAnswerViewModel>>()
                        ?? new List<CompanyScreeningAnswerViewModel>();
                }


                return View(model);
            }
            catch (HttpRequestException)
            {
                TempData["CompanyApplicationError"] =
                    "Unable to connect to the WorkHub API.";


                return RedirectToAction(
                    nameof(Index));
            }
        }


        // =========================================
        // UPDATE APPLICATION STATUS
        //
        // POST:
        // /Employer/Applications/10/Status
        // =========================================

        [HttpPost("{applicationId:int}/Status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
            int applicationId,
            string status)
        {
            if (!IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "CompanyPortal");
            }


            if (applicationId <= 0)
            {
                TempData["CompanyApplicationError"] =
                    "Invalid application.";


                return RedirectToAction(
                    nameof(Index));
            }


            status =
                status?.Trim()
                ??
                string.Empty;


            // =====================================
            // LOCAL VALIDATION
            // =====================================

            var allowedStatuses =
                new[]
                {
                    "Received",
                    "Under Review",
                    "Shortlisted",
                    "Interview",
                    "Offered",
                    "Hired",
                    "Rejected"
                };


            var normalizedStatus =
                allowedStatuses
                    .FirstOrDefault(x =>
                        x.Equals(
                            status,
                            StringComparison.OrdinalIgnoreCase));


            if (normalizedStatus == null)
            {
                TempData["CompanyApplicationError"] =
                    "Invalid application status.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        applicationId
                    });
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


                var apiRequest =
                    new CompanyApplicationStatusRequest
                    {
                        Status =
                            normalizedStatus
                    };


                using var request =
                    new HttpRequestMessage(
                        HttpMethod.Patch,
                        $"api/JobApplications/company/application/{applicationId}/status");


                request.Content =
                    JsonContent.Create(
                        apiRequest);


                var response =
                    await client.SendAsync(
                        request);


                if (IsUnauthorized(
                        response))
                {
                    ClearCompanySession();


                    return RedirectToAction(
                        "Login",
                        "CompanyPortal");
                }


                if (!response.IsSuccessStatusCode)
                {
                    TempData["CompanyApplicationError"] =
                        await GetApiMessageAsync(
                            response);


                    return RedirectToAction(
                        nameof(Details),
                        new
                        {
                            applicationId
                        });
                }


                TempData["CompanyApplicationSuccess"] =
                    "Application status updated successfully.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        applicationId
                    });
            }
            catch (HttpRequestException)
            {
                TempData["CompanyApplicationError"] =
                    "Unable to connect to the WorkHub API.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        applicationId
                    });
            }
        }


        // =========================================
        // CHECK COMPANY LOGIN
        // =========================================

        private bool IsCompanyLoggedIn()
        {
            var role =
                HttpContext.Session
                    .GetString(
                        "Role");


            var token =
                GetCompanyToken();


            return
                !string.IsNullOrWhiteSpace(
                    token)
                &&
                string.Equals(
                    role,
                    "Company",
                    StringComparison.OrdinalIgnoreCase);
        }


        // =========================================
        // GET COMPANY JWT
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
        // CLEAR COMPANY SESSION
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
        // 401 / 403
        // =========================================

        private static bool IsUnauthorized(
            HttpResponseMessage response)
        {
            return
                response.StatusCode ==
                HttpStatusCode.Unauthorized
                ||
                response.StatusCode ==
                HttpStatusCode.Forbidden;
        }


        // =========================================
        // API ERROR MESSAGE
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
    }
}