using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;
using WorkHub_Admin.Models;

namespace WorkHub_Admin.Controllers
{
    public class AdminAuthController : Controller
    {
        private readonly IHttpClientFactory
            _httpClientFactory;


        public AdminAuthController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory =
                httpClientFactory;
        }


        // =========================================
        // ADMIN LOGIN - GET
        // =========================================

        [HttpGet]
        public IActionResult Login()
        {
            if (IsAdminLoggedIn())
            {
                return RedirectToAction(
                    "Index",
                    "AdminDashboard");
            }


            return View(
                new AdminLoginViewModel());
        }


        // =========================================
        // ADMIN LOGIN - POST
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            AdminLoginViewModel model)
        {
            if (IsAdminLoggedIn())
            {
                return RedirectToAction(
                    "Index",
                    "AdminDashboard");
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
                            AdminLoginApiResponse>();


                if (loginResult == null ||
                    string.IsNullOrWhiteSpace(
                        loginResult.Token))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Unable to complete admin login.");


                    return View(model);
                }


                // =================================
                // ADMIN ROLE CHECK
                // =================================

                if (!string.Equals(
                        loginResult.Role,
                        "Admin",
                        StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Access denied. This account is not an administrator account.");


                    return View(model);
                }


                // =================================
                // CLEAR OLD SESSION
                // =================================

                HttpContext.Session.Clear();


                // =================================
                // SAVE ADMIN SESSION
                // =================================

                HttpContext.Session.SetString(
                    "AdminJwtToken",
                    loginResult.Token);


                HttpContext.Session.SetString(
                    "AdminUserId",
                    loginResult.UserId.ToString());


                HttpContext.Session.SetString(
                    "AdminFullName",
                    loginResult.FullName);


                HttpContext.Session.SetString(
                    "AdminEmail",
                    loginResult.Email);


                HttpContext.Session.SetString(
                    "AdminRole",
                    loginResult.Role);


                return RedirectToAction(
                    "Index",
                    "AdminDashboard");
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
        // ADMIN LOGOUT
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();


            return RedirectToAction(
                "Login");
        }


        // =========================================
        // CHECK ADMIN SESSION
        // =========================================

        private bool IsAdminLoggedIn()
        {
            var token =
                HttpContext.Session.GetString(
                    "AdminJwtToken");


            var role =
                HttpContext.Session.GetString(
                    "AdminRole");


            return
                !string.IsNullOrWhiteSpace(
                    token)
                &&
                string.Equals(
                    role,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase);
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
                "Unable to complete the request.";
        }
    }
}