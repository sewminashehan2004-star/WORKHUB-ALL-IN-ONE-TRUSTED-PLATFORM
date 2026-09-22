using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WorkHub_Admin.Models;

namespace WorkHub_Admin.Controllers
{
    public class AdminCompanyRequestsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminCompanyRequestsController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        // =========================================
        // DETAILS
        //
        // GET:
        // /AdminCompanyRequests/Details/5
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "AdminAuth");
            }

            var token =
                HttpContext.Session.GetString(
                    "AdminJwtToken")!;

            try
            {
                var client =
                    CreateAuthenticatedClient(token);

                var response =
                    await client.GetAsync(
                        $"api/Admin/CompanyApprovals/{id}");

                if (response.StatusCode ==
                    HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();

                    return RedirectToAction(
                        "Login",
                        "AdminAuth");
                }

                if (response.StatusCode ==
                    HttpStatusCode.Forbidden)
                {
                    HttpContext.Session.Clear();

                    return RedirectToAction(
                        "Login",
                        "AdminAuth");
                }

                if (response.StatusCode ==
                    HttpStatusCode.NotFound)
                {
                    TempData["AdminError"] =
                        "Company registration request not found.";

                    return RedirectToAction(
                        "Index",
                        "AdminDashboard");
                }

                if (!response.IsSuccessStatusCode)
                {
                    TempData["AdminError"] =
                        await GetApiMessageAsync(response);

                    return RedirectToAction(
                        "Index",
                        "AdminDashboard");
                }

                var model =
                    await response.Content
                        .ReadFromJsonAsync<
                            AdminCompanyRequestViewModel>();

                if (model == null)
                {
                    TempData["AdminError"] =
                        "Unable to load the company request.";

                    return RedirectToAction(
                        "Index",
                        "AdminDashboard");
                }

                return View(model);
            }
            catch (HttpRequestException)
            {
                TempData["AdminError"] =
                    "Unable to connect to the WorkHub API.";

                return RedirectToAction(
                    "Index",
                    "AdminDashboard");
            }
        }


        // =========================================
        // APPROVE
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "AdminAuth");
            }

            var token =
                HttpContext.Session.GetString(
                    "AdminJwtToken")!;

            try
            {
                var client =
                    CreateAuthenticatedClient(token);

                var response =
                    await client.PostAsync(
                        $"api/Admin/CompanyApprovals/{id}/approve",
                        null);

                if (response.StatusCode ==
                    HttpStatusCode.Unauthorized ||
                    response.StatusCode ==
                    HttpStatusCode.Forbidden)
                {
                    HttpContext.Session.Clear();

                    return RedirectToAction(
                        "Login",
                        "AdminAuth");
                }

                if (!response.IsSuccessStatusCode)
                {
                    TempData["AdminError"] =
                        await GetApiMessageAsync(response);

                    return RedirectToAction(
                        "Details",
                        new { id });
                }

                TempData["AdminSuccess"] =
                    "Company registration approved successfully.";

                return RedirectToAction(
                    "Index",
                    "AdminDashboard");
            }
            catch (HttpRequestException)
            {
                TempData["AdminError"] =
                    "Unable to connect to the WorkHub API.";

                return RedirectToAction(
                    "Details",
                    new { id });
            }
        }


        // =========================================
        // REJECT
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "AdminAuth");
            }

            var token =
                HttpContext.Session.GetString(
                    "AdminJwtToken")!;

            try
            {
                var client =
                    CreateAuthenticatedClient(token);

                var response =
                    await client.PostAsync(
                        $"api/Admin/CompanyApprovals/{id}/reject",
                        null);

                if (response.StatusCode ==
                    HttpStatusCode.Unauthorized ||
                    response.StatusCode ==
                    HttpStatusCode.Forbidden)
                {
                    HttpContext.Session.Clear();

                    return RedirectToAction(
                        "Login",
                        "AdminAuth");
                }

                if (!response.IsSuccessStatusCode)
                {
                    TempData["AdminError"] =
                        await GetApiMessageAsync(response);

                    return RedirectToAction(
                        "Details",
                        new { id });
                }

                TempData["AdminSuccess"] =
                    "Company registration request rejected.";

                return RedirectToAction(
                    "Index",
                    "AdminDashboard");
            }
            catch (HttpRequestException)
            {
                TempData["AdminError"] =
                    "Unable to connect to the WorkHub API.";

                return RedirectToAction(
                    "Details",
                    new { id });
            }
        }


        private bool IsAdminLoggedIn()
        {
            var token =
                HttpContext.Session.GetString(
                    "AdminJwtToken");

            var role =
                HttpContext.Session.GetString(
                    "AdminRole");

            return
                !string.IsNullOrWhiteSpace(token)
                &&
                string.Equals(
                    role,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase);
        }


        private HttpClient
            CreateAuthenticatedClient(
                string token)
        {
            var client =
                _httpClientFactory.CreateClient(
                    "WorkHubApi");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            return client;
        }


        private static async Task<string>
            GetApiMessageAsync(
                HttpResponseMessage response)
        {
            try
            {
                var content =
                    await response.Content
                        .ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                {
                    return
                        "Unable to complete the request.";
                }

                using var document =
                    JsonDocument.Parse(content);

                if (document.RootElement.TryGetProperty(
                        "message",
                        out var message))
                {
                    return
                        message.GetString()
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