using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Workhub_Web.Models;

namespace Workhub_Web.Controllers
{
    [Route("Employer")]
    public class CompanyPortalController : Controller
    {
        private readonly IHttpClientFactory
            _httpClientFactory;


        private const long MaxProfileImageSize =
            5 * 1024 * 1024;


        private static readonly string[]
            AllowedImageExtensions =
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };


        private static readonly string[]
            AllowedImageContentTypes =
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };


        public CompanyPortalController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory =
                httpClientFactory;
        }


        // =========================================
        // COMPANY PORTAL LANDING
        //
        // GET:
        // /Employer
        // =========================================

        [HttpGet("")]
        public IActionResult Index()
        {
            if (IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Index",
                    "CompanyHome");
            }


            return View();
        }


        // =========================================
        // REGISTER - GET
        // =========================================

        [HttpGet("Register")]
        public IActionResult Register()
        {
            if (IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Index",
                    "CompanyHome");
            }


            return View(
                new CompanyRegisterViewModel());
        }


        // =========================================
        // REGISTER - POST
        // =========================================

        [HttpPost("Register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            CompanyRegisterViewModel model)
        {
            if (IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Index",
                    "CompanyHome");
            }


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var client =
                _httpClientFactory.CreateClient(
                    "WorkHubApi");


            var request =
                new
                {
                    companyName =
                        model.CompanyName.Trim(),

                    fullName =
                        model.FullName.Trim(),

                    email =
                        model.Email
                            .Trim()
                            .ToLowerInvariant(),

                    phone =
                        Clean(model.Phone),

                    location =
                        model.Location.Trim(),

                    password =
                        model.Password,

                    confirmPassword =
                        model.ConfirmPassword
                };


            try
            {
                var response =
                    await client.PostAsJsonAsync(
                        "api/Auth/company-register",
                        request);


                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        await GetApiMessageAsync(
                            response));


                    return View(model);
                }


                TempData["SubmittedCompanyName"] =
                    model.CompanyName.Trim();


                TempData["SubmittedCompanyEmail"] =
                    model.Email
                        .Trim()
                        .ToLowerInvariant();


                return RedirectToAction(
                    "RegistrationSubmitted");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the WorkHub API.");
            }


            return View(model);
        }


        // =========================================
        // REGISTRATION SUBMITTED
        // =========================================

        [HttpGet("RegistrationSubmitted")]
        public IActionResult RegistrationSubmitted()
        {
            if (IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Index",
                    "CompanyHome");
            }


            ViewBag.CompanyName =
                TempData["SubmittedCompanyName"]
                    ?.ToString();


            ViewBag.CompanyEmail =
                TempData["SubmittedCompanyEmail"]
                    ?.ToString();


            return View();
        }


        // =========================================
        // LOGIN - GET
        // =========================================

        [HttpGet("Login")]
        public IActionResult Login()
        {
            if (IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Index",
                    "CompanyHome");
            }


            return View(
                new CompanyLoginViewModel());
        }


        // =========================================
        // LOGIN - POST
        // =========================================

        [HttpPost("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            CompanyLoginViewModel model)
        {
            if (IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Index",
                    "CompanyHome");
            }


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var client =
                _httpClientFactory.CreateClient(
                    "WorkHubApi");


            var request =
                new
                {
                    email =
                        model.Email
                            .Trim()
                            .ToLowerInvariant(),

                    password =
                        model.Password
                };


            try
            {
                var response =
                    await client.PostAsJsonAsync(
                        "api/Auth/login",
                        request);


                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        await GetApiMessageAsync(
                            response));


                    return View(model);
                }


                var loginResult =
                    await response.Content
                        .ReadFromJsonAsync<
                            CompanyLoginApiResponse>();


                if (loginResult == null ||
                    string.IsNullOrWhiteSpace(
                        loginResult.Token))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Unable to complete company login.");


                    return View(model);
                }


                if (!string.Equals(
                        loginResult.Role,
                        "Company",
                        StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "This account is not a Company / Organization account.");


                    return View(model);
                }


                HttpContext.Session.Clear();


                HttpContext.Session.SetString(
                    "JwtToken",
                    loginResult.Token);


                HttpContext.Session.SetString(
                    "UserId",
                    loginResult.UserId.ToString());


                HttpContext.Session.SetString(
                    "FullName",
                    loginResult.FullName);


                HttpContext.Session.SetString(
                    "Email",
                    loginResult.Email);


                HttpContext.Session.SetString(
                    "Role",
                    loginResult.Role);


                return RedirectToAction(
                    "Index",
                    "CompanyHome");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the WorkHub API.");
            }


            return View(model);
        }


        // =========================================
        // COMPANY DASHBOARD
        //
        // GET:
        // /Employer/Dashboard
        //
        // Loads:
        // - Company Profile
        // - Active Job Count
        // - Total Job Applications
        // - Profile Picture
        // =========================================

        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Login");
            }


            var token =
                GetCompanyToken();


            if (string.IsNullOrWhiteSpace(
                    token))
            {
                ClearCompanySession();


                return RedirectToAction(
                    "Login");
            }


            var model =
                new CompanyDashboardViewModel
                {
                    FullName =
                        HttpContext.Session
                            .GetString("FullName")
                        ?? string.Empty,

                    Email =
                        HttpContext.Session
                            .GetString("Email")
                        ?? string.Empty,

                    SuccessMessage =
                        TempData["CompanySuccess"]
                            ?.ToString(),

                    ErrorMessage =
                        TempData["CompanyError"]
                            ?.ToString()
                };


            // =====================================
            // DEFAULT DASHBOARD COUNTS
            // =====================================

            ViewBag.ActiveJobPosts =
                0;


            ViewBag.JobApplications =
                0;


            // =====================================
            // PROFILE IMAGE URL
            // =====================================

            ViewBag.CompanyProfileImageUrl =
                Url.Action(
                    nameof(ProfileImage),
                    "CompanyPortal",
                    new
                    {
                        v =
                            DateTime.UtcNow.Ticks
                    });


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                // =================================
                // 1. LOAD COMPANY PROFILE
                // =================================

                var profileResponse =
                    await client.GetAsync(
                        "api/Company/profile");


                if (IsUnauthorized(
                        profileResponse))
                {
                    ClearCompanySession();


                    return RedirectToAction(
                        "Login");
                }


                if (profileResponse.StatusCode ==
                    HttpStatusCode.NotFound)
                {
                    model.CompanyProfileExists =
                        false;


                    return View(model);
                }


                if (!profileResponse
                    .IsSuccessStatusCode)
                {
                    model.ErrorMessage =
                        await GetApiMessageAsync(
                            profileResponse);


                    return View(model);
                }


                var company =
                    await profileResponse.Content
                        .ReadFromJsonAsync<
                            CompanyProfileFormViewModel>();


                if (company != null)
                {
                    model.CompanyProfileExists =
                        true;


                    model.Company =
                        company;
                }


                // =================================
                // 2. LOAD COMPANY JOBS
                //
                // API:
                // GET api/Jobs/company/me
                // =================================

                var jobsResponse =
                    await client.GetAsync(
                        "api/Jobs/company/me");


                if (IsUnauthorized(
                        jobsResponse))
                {
                    ClearCompanySession();


                    return RedirectToAction(
                        "Login");
                }


                if (jobsResponse
                    .IsSuccessStatusCode)
                {
                    var jobs =
                        await jobsResponse.Content
                            .ReadFromJsonAsync<
                                List<
                                    CompanyMyJobItemViewModel>>()
                        ??
                        new List<
                            CompanyMyJobItemViewModel>();


                    // =============================
                    // ACTIVE JOB POSTS COUNT
                    // =============================

                    var activeJobPosts =
                        jobs.Count(x =>
                            string.Equals(
                                x.Status,
                                "Active",
                                StringComparison
                                    .OrdinalIgnoreCase));


                    ViewBag.ActiveJobPosts =
                        activeJobPosts;


                    // =============================
                    // TOTAL APPLICATION COUNT
                    // =============================

                    var totalApplications =
                        0;


                    foreach (var job
                        in jobs)
                    {
                        try
                        {
                            var applicationsResponse =
                                await client.GetAsync(
                                    $"api/JobApplications/company/job/{job.JobId}");


                            if (IsUnauthorized(
                                    applicationsResponse))
                            {
                                ClearCompanySession();


                                return RedirectToAction(
                                    "Login");
                            }


                            if (!applicationsResponse
                                .IsSuccessStatusCode)
                            {
                                continue;
                            }


                            var applicationResult =
                                await applicationsResponse
                                    .Content
                                    .ReadFromJsonAsync<
                                        CompanyJobApplicantsApiResponse>();


                            if (applicationResult != null)
                            {
                                totalApplications +=
                                    applicationResult
                                        .TotalApplicants;
                            }
                        }
                        catch (HttpRequestException)
                        {
                            // One application's API call
                            // should not break the dashboard.
                        }
                    }


                    ViewBag.JobApplications =
                        totalApplications;
                }
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage =
                    "Unable to connect to the WorkHub API.";
            }


            return View(model);
        }


        // =========================================
        // SAVE COMPANY PROFILE
        // =========================================

        [HttpPost("SaveProfile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProfile(
            CompanyProfileFormViewModel model)
        {
            if (!IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Login");
            }


            if (string.IsNullOrWhiteSpace(
                    model.CompanyName))
            {
                TempData["CompanyError"] =
                    "Company / Organization name is required.";


                return RedirectToAction(
                    "Dashboard");
            }


            var token =
                GetCompanyToken();


            if (string.IsNullOrWhiteSpace(
                    token))
            {
                ClearCompanySession();


                return RedirectToAction(
                    "Login");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                var existingResponse =
                    await client.GetAsync(
                        "api/Company/profile");


                if (IsUnauthorized(
                        existingResponse))
                {
                    ClearCompanySession();


                    return RedirectToAction(
                        "Login");
                }


                var request =
                    new
                    {
                        companyName =
                            model.CompanyName.Trim(),

                        industry =
                            Clean(model.Industry),

                        website =
                            Clean(model.Website),

                        description =
                            Clean(model.Description),

                        address =
                            Clean(model.Address)
                    };


                HttpResponseMessage saveResponse;


                if (existingResponse
                    .IsSuccessStatusCode)
                {
                    saveResponse =
                        await client.PutAsJsonAsync(
                            "api/Company/profile",
                            request);
                }
                else if (
                    existingResponse.StatusCode ==
                    HttpStatusCode.NotFound)
                {
                    saveResponse =
                        await client.PostAsJsonAsync(
                            "api/Company/profile",
                            request);
                }
                else
                {
                    TempData["CompanyError"] =
                        await GetApiMessageAsync(
                            existingResponse);


                    return RedirectToAction(
                        "Dashboard");
                }


                if (IsUnauthorized(
                        saveResponse))
                {
                    ClearCompanySession();


                    return RedirectToAction(
                        "Login");
                }


                if (!saveResponse.IsSuccessStatusCode)
                {
                    TempData["CompanyError"] =
                        await GetApiMessageAsync(
                            saveResponse);
                }
                else
                {
                    TempData["CompanySuccess"] =
                        "Company profile updated successfully.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["CompanyError"] =
                    "Unable to connect to the WorkHub API.";
            }


            return RedirectToAction(
                "Dashboard");
        }


        // =========================================
        // COMPANY PROFILE IMAGE
        //
        // GET:
        // /Employer/ProfileImage
        // =========================================

        [HttpGet("ProfileImage")]
        public async Task<IActionResult> ProfileImage()
        {
            if (!IsCompanyLoggedIn())
            {
                return NotFound();
            }


            var token =
                GetCompanyToken();


            if (string.IsNullOrWhiteSpace(
                    token))
            {
                return NotFound();
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                var response =
                    await client.GetAsync(
                        "api/Company/profile-image");


                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }


                var bytes =
                    await response.Content
                        .ReadAsByteArrayAsync();


                if (bytes.Length == 0)
                {
                    return NotFound();
                }


                var contentType =
                    response.Content.Headers
                        .ContentType?
                        .MediaType
                    ??
                    "image/jpeg";


                DisableImageCaching();


                return File(
                    bytes,
                    contentType);
            }
            catch
            {
                return NotFound();
            }
        }


        // =========================================
        // UPLOAD PROFILE IMAGE
        //
        // POST:
        // /Employer/UploadProfileImage
        // =========================================

        [HttpPost("UploadProfileImage")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            UploadProfileImage(
                IFormFile profileImage)
        {
            if (!IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Login");
            }


            if (profileImage == null ||
                profileImage.Length <= 0)
            {
                TempData["CompanyError"] =
                    "Please select a profile picture.";


                return RedirectToAction(
                    "Dashboard");
            }


            if (profileImage.Length >
                MaxProfileImageSize)
            {
                TempData["CompanyError"] =
                    "Profile picture must be 5 MB or smaller.";


                return RedirectToAction(
                    "Dashboard");
            }


            var extension =
                Path.GetExtension(
                        profileImage.FileName)
                    .ToLowerInvariant();


            if (!AllowedImageExtensions
                .Contains(
                    extension))
            {
                TempData["CompanyError"] =
                    "Only JPG, JPEG, PNG and WEBP images are allowed.";


                return RedirectToAction(
                    "Dashboard");
            }


            if (
                string.IsNullOrWhiteSpace(
                    profileImage.ContentType)
                ||
                !AllowedImageContentTypes
                    .Contains(
                        profileImage.ContentType,
                        StringComparer
                            .OrdinalIgnoreCase)
            )
            {
                TempData["CompanyError"] =
                    "Invalid profile image type.";


                return RedirectToAction(
                    "Dashboard");
            }


            var token =
                GetCompanyToken();


            if (string.IsNullOrWhiteSpace(
                    token))
            {
                ClearCompanySession();


                return RedirectToAction(
                    "Login");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                using var content =
                    new MultipartFormDataContent();


                await using var stream =
                    profileImage.OpenReadStream();


                using var fileContent =
                    new StreamContent(
                        stream);


                fileContent.Headers.ContentType =
                    new MediaTypeHeaderValue(
                        profileImage.ContentType);


                content.Add(
                    fileContent,
                    "file",
                    profileImage.FileName);


                var response =
                    await client.PostAsync(
                        "api/Company/profile-image",
                        content);


                if (IsUnauthorized(
                        response))
                {
                    ClearCompanySession();


                    return RedirectToAction(
                        "Login");
                }


                if (!response.IsSuccessStatusCode)
                {
                    TempData["CompanyError"] =
                        await GetApiMessageAsync(
                            response);
                }
                else
                {
                    TempData["CompanySuccess"] =
                        "Profile picture updated successfully.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["CompanyError"] =
                    "Unable to upload the profile picture.";
            }


            return RedirectToAction(
                "Dashboard");
        }


        // =========================================
        // REMOVE PROFILE IMAGE
        // =========================================

        [HttpPost("RemoveProfileImage")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            RemoveProfileImage()
        {
            if (!IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Login");
            }


            var token =
                GetCompanyToken();


            if (string.IsNullOrWhiteSpace(
                    token))
            {
                ClearCompanySession();


                return RedirectToAction(
                    "Login");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                var response =
                    await client.DeleteAsync(
                        "api/Company/profile-image");


                if (IsUnauthorized(
                        response))
                {
                    ClearCompanySession();


                    return RedirectToAction(
                        "Login");
                }


                if (!response.IsSuccessStatusCode)
                {
                    TempData["CompanyError"] =
                        await GetApiMessageAsync(
                            response);
                }
                else
                {
                    TempData["CompanySuccess"] =
                        "Profile picture removed successfully.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["CompanyError"] =
                    "Unable to remove the profile picture.";
            }


            return RedirectToAction(
                "Dashboard");
        }


        // =========================================
        // UPDATE ACCOUNT MANAGER NAME
        // =========================================

        [HttpPost("UpdateName")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateName(
            string fullName)
        {
            if (!IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Login");
            }


            fullName =
                fullName?.Trim()
                ??
                string.Empty;


            if (string.IsNullOrWhiteSpace(
                    fullName))
            {
                TempData["CompanyError"] =
                    "Account manager name is required.";


                return RedirectToAction(
                    "Dashboard");
            }


            if (fullName.Length < 2)
            {
                TempData["CompanyError"] =
                    "Please enter a valid name.";


                return RedirectToAction(
                    "Dashboard");
            }


            if (fullName.Length > 150)
            {
                TempData["CompanyError"] =
                    "Account manager name cannot exceed 150 characters.";


                return RedirectToAction(
                    "Dashboard");
            }


            var token =
                GetCompanyToken();


            if (string.IsNullOrWhiteSpace(
                    token))
            {
                ClearCompanySession();


                return RedirectToAction(
                    "Login");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                var request =
                    new
                    {
                        fullName
                    };


                var response =
                    await client.PutAsJsonAsync(
                        "api/Company/account/name",
                        request);


                if (IsUnauthorized(
                        response))
                {
                    ClearCompanySession();


                    return RedirectToAction(
                        "Login");
                }


                if (!response.IsSuccessStatusCode)
                {
                    TempData["CompanyError"] =
                        await GetApiMessageAsync(
                            response);


                    return RedirectToAction(
                        "Dashboard");
                }


                HttpContext.Session.SetString(
                    "FullName",
                    fullName);


                TempData["CompanySuccess"] =
                    "Account manager name updated successfully.";
            }
            catch (HttpRequestException)
            {
                TempData["CompanyError"] =
                    "Unable to update the account manager name.";
            }


            return RedirectToAction(
                "Dashboard");
        }


        // =========================================
        // CHANGE PASSWORD
        // =========================================

        [HttpPost("ChangePassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            ChangePassword(
                string currentPassword,
                string newPassword,
                string confirmPassword)
        {
            if (!IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Login");
            }


            currentPassword ??=
                string.Empty;


            newPassword ??=
                string.Empty;


            confirmPassword ??=
                string.Empty;


            if (
                string.IsNullOrWhiteSpace(
                    currentPassword)
                ||
                string.IsNullOrWhiteSpace(
                    newPassword)
                ||
                string.IsNullOrWhiteSpace(
                    confirmPassword)
            )
            {
                TempData["CompanyError"] =
                    "Please complete all password fields.";


                return RedirectToAction(
                    "Dashboard");
            }


            if (newPassword.Length < 6)
            {
                TempData["CompanyError"] =
                    "New password must contain at least 6 characters.";


                return RedirectToAction(
                    "Dashboard");
            }


            if (!string.Equals(
                    newPassword,
                    confirmPassword,
                    StringComparison.Ordinal))
            {
                TempData["CompanyError"] =
                    "New password and confirmation password do not match.";


                return RedirectToAction(
                    "Dashboard");
            }


            if (string.Equals(
                    currentPassword,
                    newPassword,
                    StringComparison.Ordinal))
            {
                TempData["CompanyError"] =
                    "New password must be different from your current password.";


                return RedirectToAction(
                    "Dashboard");
            }


            var token =
                GetCompanyToken();


            if (string.IsNullOrWhiteSpace(
                    token))
            {
                ClearCompanySession();


                return RedirectToAction(
                    "Login");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                var request =
                    new
                    {
                        currentPassword,
                        newPassword,
                        confirmPassword
                    };


                var response =
                    await client.PutAsJsonAsync(
                        "api/Company/account/password",
                        request);


                if (IsUnauthorized(
                        response))
                {
                    ClearCompanySession();


                    return RedirectToAction(
                        "Login");
                }


                if (!response.IsSuccessStatusCode)
                {
                    TempData["CompanyError"] =
                        await GetApiMessageAsync(
                            response);


                    return RedirectToAction(
                        "Dashboard");
                }


                TempData["CompanySuccess"] =
                    "Password changed successfully.";
            }
            catch (HttpRequestException)
            {
                TempData["CompanyError"] =
                    "Unable to change the password.";
            }


            return RedirectToAction(
                "Dashboard");
        }


        // =========================================
        // DELETE COMPANY ACCOUNT
        // =========================================

        [HttpPost("DeleteAccount")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            DeleteAccount(
                string password,
                string confirmation)
        {
            if (!IsCompanyLoggedIn())
            {
                return RedirectToAction(
                    "Login");
            }


            password ??=
                string.Empty;


            confirmation =
                confirmation?.Trim()
                ??
                string.Empty;


            if (string.IsNullOrWhiteSpace(
                    password))
            {
                TempData["CompanyError"] =
                    "Enter your password before deleting the account.";


                return RedirectToAction(
                    "Dashboard");
            }


            if (!string.Equals(
                    confirmation,
                    "DELETE",
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["CompanyError"] =
                    "Type DELETE to confirm account deletion.";


                return RedirectToAction(
                    "Dashboard");
            }


            var token =
                GetCompanyToken();


            if (string.IsNullOrWhiteSpace(
                    token))
            {
                ClearCompanySession();


                return RedirectToAction(
                    "Login");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                var deleteRequest =
                    new
                    {
                        password,
                        confirmation =
                            "DELETE"
                    };


                using var request =
                    new HttpRequestMessage(
                        HttpMethod.Delete,
                        "api/Company/account");


                request.Content =
                    JsonContent.Create(
                        deleteRequest);


                var response =
                    await client.SendAsync(
                        request);


                if (IsUnauthorized(
                        response))
                {
                    ClearCompanySession();


                    return RedirectToAction(
                        "Login");
                }


                if (!response.IsSuccessStatusCode)
                {
                    TempData["CompanyError"] =
                        await GetApiMessageAsync(
                            response);


                    return RedirectToAction(
                        "Dashboard");
                }


                ClearCompanySession();


                TempData["CompanyAccountDeleted"] =
                    "Your company account has been deleted.";


                return RedirectToAction(
                    "Index");
            }
            catch (HttpRequestException)
            {
                TempData["CompanyError"] =
                    "Unable to delete the company account.";


                return RedirectToAction(
                    "Dashboard");
            }
        }


        // =========================================
        // LOGOUT
        // =========================================

        [HttpPost("Logout")]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            ClearCompanySession();


            return RedirectToAction(
                "Index");
        }


        // =========================================
        // GET COMPANY TOKEN
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
        // CHECK COMPANY LOGIN
        // =========================================

        private bool IsCompanyLoggedIn()
        {
            var token =
                GetCompanyToken();


            var role =
                HttpContext.Session
                    .GetString(
                        "Role");


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
        // CLEAR COMPANY SESSION
        // =========================================

        private void ClearCompanySession()
        {
            HttpContext.Session.Clear();
        }


        // =========================================
        // CHECK API AUTH RESPONSE
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
        // AUTHENTICATED API CLIENT
        // =========================================

        private HttpClient
            CreateAuthenticatedClient(
                string token)
        {
            var client =
                _httpClientFactory.CreateClient(
                    "WorkHubApi");


            client.DefaultRequestHeaders
                .Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);


            return client;
        }


        // =========================================
        // DISABLE PROFILE IMAGE CACHE
        // =========================================

        private void DisableImageCaching()
        {
            Response.Headers.CacheControl =
                "no-store, no-cache, must-revalidate, max-age=0";


            Response.Headers.Pragma =
                "no-cache";


            Response.Headers.Expires =
                "0";
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
        // API MESSAGE
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


                if (document.RootElement
                    .TryGetProperty(
                        "message",
                        out var message))
                {
                    return
                        message.GetString()
                        ??
                        "Unable to complete the request.";
                }


                if (document.RootElement
                    .TryGetProperty(
                        "title",
                        out var title))
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
                $"Unable to complete the request. Status: {(int)response.StatusCode}.";
        }
    }
}