using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Workhub_Web.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Workhub_Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory
            _httpClientFactory;


        public AccountController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory =
                httpClientFactory;
        }


        // =========================================
        // REGISTER - GET
        // =========================================

        [HttpGet]
        public IActionResult Register()
        {
            if (IsLoggedIn())
            {
                return RedirectToAction(
                    "Profile");
            }

            return View(
                new RegisterViewModel());
        }


        // =========================================
        // REGISTER - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model)
        {
            if (IsLoggedIn())
            {
                return RedirectToAction(
                    "Profile");
            }


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var client =
                _httpClientFactory.CreateClient(
                    "WorkHubApi");


            var apiRequest =
                new
                {
                    fullName =
                        model.FullName.Trim(),

                    email =
                        model.Email
                            .Trim()
                            .ToLowerInvariant(),

                    password =
                        model.Password,

                    confirmPassword =
                        model.ConfirmPassword,

                    phone =
                        (string?)null,

                    location =
                        (string?)null,

                    accountType =
                        "RegisteredUser"
                };


            try
            {
                var response =
                    await client.PostAsJsonAsync(
                        "api/Auth/register",
                        apiRequest);


                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Your WorkHub account was created successfully. Please sign in.";


                    return RedirectToAction(
                        "Login");
                }


                var errorMessage =
                    await GetApiMessageAsync(
                        response);


                ModelState.AddModelError(
                    string.Empty,
                    errorMessage);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to WorkHub API.");
            }


            return View(model);
        }


        // =========================================
        // LOGIN - GET
        // =========================================

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (IsLoggedIn())
            {
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }

                return RedirectToAction("Profile");
            }

            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }


        // =========================================
        // LOGIN - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (IsLoggedIn())
            {
                return RedirectToAction(
                    "Profile");
            }


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            var client =
                _httpClientFactory.CreateClient(
                    "WorkHubApi");


            var apiRequest =
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
                        apiRequest);


                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage =
                        await GetApiMessageAsync(
                            response);


                    ModelState.AddModelError(
                        string.Empty,
                        errorMessage);


                    return View(model);
                }


                var loginResponse =
                    await response.Content
                        .ReadFromJsonAsync<
                            LoginApiResponse>();


                if (
                    loginResponse == null
                    ||
                    string.IsNullOrWhiteSpace(
                        loginResponse.Token)
                )
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Unable to complete login.");


                    return View(model);
                }


                HttpContext.Session.SetString(
                    "JwtToken",
                    loginResponse.Token);


                HttpContext.Session.SetString(
                    "UserId",
                    loginResponse.UserId.ToString());


                HttpContext.Session.SetString(
                    "FullName",
                    loginResponse.FullName);


                HttpContext.Session.SetString(
                    "Email",
                    loginResponse.Email);


                HttpContext.Session.SetString(
                    "Role",
                    loginResponse.Role);


                TempData["SuccessMessage"] =
                    $"Welcome back, {loginResponse.FullName}!";

                if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return LocalRedirect(model.ReturnUrl);
                }

                return RedirectToAction(
                    "Index",
                    "Home");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to WorkHub API.");
            }


            return View(model);
        }


        // =========================================
        // MY PROFILE DASHBOARD
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdValue =
                HttpContext.Session
                    .GetString("UserId");


            var fullName =
                HttpContext.Session
                    .GetString("FullName");


            var email =
                HttpContext.Session
                    .GetString("Email");


            var role =
                HttpContext.Session
                    .GetString("Role");


            var token =
                HttpContext.Session
                    .GetString("JwtToken");


            if (
                string.IsNullOrWhiteSpace(
                    userIdValue)
                ||
                string.IsNullOrWhiteSpace(
                    fullName)
                ||
                string.IsNullOrWhiteSpace(
                    email)
                ||
                string.IsNullOrWhiteSpace(
                    token)
            )
            {
                return RedirectToAction(
                    "Login");
            }


            int.TryParse(
                userIdValue,
                out var userId);


            var model =
                new UserProfileViewModel
                {
                    UserId =
                        userId,

                    FullName =
                        fullName,

                    Email =
                        email,

                    Role =
                        role
                        ??
                        "RegisteredUser"
                };


            // =====================================
            // REGISTERED USER INFORMATION
            // =====================================

            if (
                model.Role.Equals(
                    "RegisteredUser",
                    StringComparison
                        .OrdinalIgnoreCase)
            )
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                // =================================
                // LOAD PROFILE TYPES
                // =================================

                try
                {
                    var response =
                        await client.GetAsync(
                            "api/Profiles/me");


                    if (response.IsSuccessStatusCode)
                    {
                        var profileStatus =
                            await response.Content
                                .ReadFromJsonAsync<
                                    ProfileStatusApiResponse>();


                        if (profileStatus != null)
                        {
                            model.JobSeekerActive =
                                profileStatus.JobSeeker;


                            model.ServiceProviderActive =
                                profileStatus.ServiceProvider;


                            model.MarketplaceSellerActive =
                                profileStatus.MarketplaceSeller;
                        }
                    }
                }
                catch (HttpRequestException)
                {
                    TempData["ProfileError"] =
                        "Unable to load profile status from WorkHub API.";
                }


                // =================================
                // LOAD NOTIFICATIONS
                // =================================

                try
                {
                    var notificationResponse =
                        await client.GetAsync(
                            "api/UserNotifications/me");

                    if (notificationResponse.IsSuccessStatusCode)
                    {
                        var notifications =
                            await notificationResponse.Content
                                .ReadFromJsonAsync<
                                    List<UserNotificationViewModel>>();

                        model.Notifications =
                            notifications
                            ?? new List<UserNotificationViewModel>();
                    }
                    else
                    {
                        model.Notifications =
                            new List<UserNotificationViewModel>();
                    }
                }
                catch (Exception)
                {
                    model.Notifications =
                        new List<UserNotificationViewModel>();
                }
            }


            return View(model);
        }


        // =========================================
        // MARK ONE NOTIFICATION READ
        // =========================================
        // =========================================
        // CHANGE PASSWORD
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(
            string currentPassword,
            string newPassword,
            string confirmPassword)
        {
            var token =
                HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrWhiteSpace(currentPassword) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                TempData["AccountError"] =
                    "Please fill in all password fields.";

                return RedirectToAction(
                    "Profile",
                    new { tab = "account-settings" });
            }

            if (newPassword.Length < 6)
            {
                TempData["AccountError"] =
                    "New password must be at least 6 characters.";

                return RedirectToAction(
                    "Profile",
                    new { tab = "account-settings" });
            }

            if (newPassword != confirmPassword)
            {
                TempData["AccountError"] =
                    "New password and confirmation password do not match.";

                return RedirectToAction(
                    "Profile",
                    new { tab = "account-settings" });
            }

            try
            {
                var client =
                    CreateAuthenticatedClient(token);

                var request =
                    new
                    {
                        currentPassword,
                        newPassword,
                        confirmPassword
                    };

                var response =
                    await client.PutAsJsonAsync(
                        "api/UserAccount/password",
                        request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["AccountSuccess"] =
                        "Password changed successfully.";

                    return RedirectToAction(
                        "Profile",
                        new { tab = "account-settings" });
                }

                TempData["AccountError"] =
                    await GetApiMessageAsync(response);
            }
            catch (HttpRequestException)
            {
                TempData["AccountError"] =
                    "Unable to connect to WorkHub API.";
            }

            return RedirectToAction(
                "Profile",
                new { tab = "account-settings" });
        }

        // =========================================
        // DELETE ACCOUNT
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount(
            string password,
            string confirmation)
        {
            var token =
                HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login");
            }

            // =====================================
            // VALIDATE CONFIRMATION
            // =====================================

            if (!string.Equals(
                    confirmation?.Trim(),
                    "DELETE",
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["AccountError"] =
                    "Please type DELETE to confirm account deletion.";

                return RedirectToAction(
                    "Profile",
                    new { tab = "account-settings" });
            }

            // =====================================
            // VALIDATE PASSWORD
            // =====================================

            if (string.IsNullOrWhiteSpace(password))
            {
                TempData["AccountError"] =
                    "Please enter your current password.";

                return RedirectToAction(
                    "Profile",
                    new { tab = "account-settings" });
            }

            try
            {
                var client =
                    CreateAuthenticatedClient(token);

                // This JSON body matches
                // DeleteAccountRequest in WorkHub_API.
                var request =
                    new
                    {
                        password = password,
                        confirmation = confirmation.Trim()
                    };

                // DELETE request with JSON body.
                using var httpRequest =
                    new HttpRequestMessage(
                        HttpMethod.Delete,
                        "api/UserAccount/account");

                httpRequest.Content =
                    JsonContent.Create(request);

                var response =
                    await client.SendAsync(httpRequest);

                // =================================
                // SUCCESS
                // =================================

                if (response.IsSuccessStatusCode)
                {
                    // Remove all local login/session data.
                    HttpContext.Session.Clear();

                    TempData["SuccessMessage"] =
                        "Your WorkHub account has been deleted successfully.";

                    return RedirectToAction(
                        "Login");
                }

                // =================================
                // API ERROR
                // =================================

                TempData["AccountError"] =
                    await GetApiMessageAsync(response);
            }
            catch (HttpRequestException)
            {
                TempData["AccountError"] =
                    "Unable to connect to WorkHub API.";
            }
            catch (Exception)
            {
                TempData["AccountError"] =
                    "Something went wrong while deleting your account.";
            }

            return RedirectToAction(
                "Profile",
                new { tab = "account-settings" });
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            MarkNotificationRead(
                int id)
        {
            var token =
                HttpContext.Session
                    .GetString("JwtToken");


            if (string.IsNullOrWhiteSpace(
                    token))
            {
                return RedirectToAction(
                    "Login");
            }


            if (id <= 0)
            {
                return RedirectToAction(
                    "Profile");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                await client.PutAsync(
                    $"api/UserNotifications/{id}/read",
                    null);
            }
            catch (HttpRequestException)
            {
                TempData["ProfileError"] =
                    "Unable to update notification.";
            }


            return RedirectToAction(
                "Profile");
        }


        // =========================================
        // MARK ALL NOTIFICATIONS READ
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            MarkAllNotificationsRead()
        {
            var token =
                HttpContext.Session
                    .GetString("JwtToken");


            if (string.IsNullOrWhiteSpace(
                    token))
            {
                return RedirectToAction(
                    "Login");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                await client.PutAsync(
                    "api/UserNotifications/read-all",
                    null);
            }
            catch (HttpRequestException)
            {
                TempData["ProfileError"] =
                    "Unable to update notifications.";
            }


            return RedirectToAction(
                "Profile");
        }


        // =========================================
        // ACTIVATE JOB SEEKER
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            ActivateJobSeeker()
        {
            return await ActivateProfile(
                jobSeeker: true,
                serviceProvider: false,
                marketplaceSeller: false,
                successMessage:
                    "Job Seeker profile activated successfully.");
        }


        // =========================================
        // ACTIVATE SERVICE PROVIDER
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            ActivateServiceProvider()
        {
            return await ActivateProfile(
                jobSeeker: false,
                serviceProvider: true,
                marketplaceSeller: false,
                successMessage:
                    "Service Provider profile activated successfully.");
        }


        // =========================================
        // ACTIVATE MARKETPLACE SELLER
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            ActivateMarketplaceSeller()
        {
            return await ActivateProfile(
                jobSeeker: false,
                serviceProvider: false,
                marketplaceSeller: true,
                successMessage:
                    "Marketplace Seller profile activated successfully.");
        }


        // =========================================
        // PROFILE ACTIVATION HELPER
        // =========================================

        private async Task<IActionResult>
            ActivateProfile(
                bool jobSeeker,
                bool serviceProvider,
                bool marketplaceSeller,
                string successMessage)
        {
            var token =
                HttpContext.Session
                    .GetString("JwtToken");


            var role =
                HttpContext.Session
                    .GetString("Role");


            if (string.IsNullOrWhiteSpace(
                    token))
            {
                return RedirectToAction(
                    "Login");
            }


            if (!string.Equals(
                    role,
                    "RegisteredUser",
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["ProfileError"] =
                    "This profile type is only available to registered users.";


                return RedirectToAction(
                    "Profile");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                var request =
                    new
                    {
                        jobSeeker,
                        serviceProvider,
                        marketplaceSeller
                    };


                var response =
                    await client.PostAsJsonAsync(
                        "api/Profiles/activate",
                        request);


                if (response.IsSuccessStatusCode)
                {
                    TempData["ProfileSuccess"] =
                        successMessage;
                }
                else
                {
                    TempData["ProfileError"] =
                        await GetApiMessageAsync(
                            response);
                }
            }
            catch (HttpRequestException)
            {
                TempData["ProfileError"] =
                    "Unable to connect to WorkHub API.";
            }


            return RedirectToAction(
                "Profile");
        }


        // =========================================
        // LOGOUT
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();


            return RedirectToAction(
                "Index",
                "Home");
        }


        // =========================================
        // AUTHENTICATED HTTP CLIENT
        // =========================================

        private HttpClient
            CreateAuthenticatedClient(
                string token)
        {
            var client =
                _httpClientFactory
                    .CreateClient(
                        "WorkHubApi");


            client.DefaultRequestHeaders
                .Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);


            return client;
        }


        // =========================================
        // LOGIN CHECK
        // =========================================

        private bool IsLoggedIn()
        {
            return
                !string.IsNullOrWhiteSpace(
                    HttpContext.Session
                        .GetString(
                            "JwtToken"));
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
                var json =
                    await response.Content
                        .ReadAsStringAsync();


                using var document =
                    JsonDocument.Parse(
                        json);


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
                        "Request failed.";
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
                        "Request failed.";
                }
            }
            catch
            {
            }


            return
                $"Request failed. Status: {(int)response.StatusCode}";
        }


        // =========================================
        // LOGIN RESPONSE
        // =========================================

        private class LoginApiResponse
        {
            public int UserId { get; set; }


            public string FullName { get; set; }
                = string.Empty;


            public string Email { get; set; }
                = string.Empty;


            public string Role { get; set; }
                = string.Empty;


            public string Token { get; set; }
                = string.Empty;


            public DateTime ExpiresAt { get; set; }
        }


        // =========================================
        // PROFILE STATUS RESPONSE
        // =========================================

        private class ProfileStatusApiResponse
        {
            public bool JobSeeker { get; set; }


            public bool ServiceProvider { get; set; }


            public bool MarketplaceSeller { get; set; }
        }
    }
}
