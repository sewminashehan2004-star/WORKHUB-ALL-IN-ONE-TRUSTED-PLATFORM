using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WorkHub_Admin.Models;

namespace WorkHub_Admin.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;


        public AdminDashboardController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory =
                httpClientFactory;
        }


        // =========================================
        // ADMIN DASHBOARD
        //
        // GET:
        // /AdminDashboard
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // =====================================
            // ADMIN SESSION CHECK
            // =====================================

            if (!IsAdminLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "AdminAuth");
            }


            var token =
                HttpContext.Session.GetString(
                    "AdminJwtToken")!;


            var model =
                new AdminDashboardViewModel
                {
                    AdminFullName =
                        HttpContext.Session
                            .GetString(
                                "AdminFullName")
                        ?? "Administrator",

                    AdminEmail =
                        HttpContext.Session
                            .GetString(
                                "AdminEmail")
                        ?? string.Empty,

                    SuccessMessage =
                        TempData["AdminSuccess"]
                            ?.ToString(),

                    ErrorMessage =
                        TempData["AdminError"]
                            ?.ToString()
                };


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                // =================================
                // LOAD PENDING COMPANY REQUESTS
                // =================================

                var response =
                    await client.GetAsync(
                        "api/Admin/CompanyApprovals/pending");


                // =================================
                // ADMIN TOKEN EXPIRED / INVALID
                // =================================

                if (response.StatusCode ==
                    HttpStatusCode.Unauthorized)
                {
                    ClearAdminSession();


                    return RedirectToAction(
                        "Login",
                        "AdminAuth");
                }


                // =================================
                // ACCESS FORBIDDEN
                // =================================

                if (response.StatusCode ==
                    HttpStatusCode.Forbidden)
                {
                    ClearAdminSession();


                    TempData["AdminLoginError"] =
                        "Administrator access is required.";


                    return RedirectToAction(
                        "Login",
                        "AdminAuth");
                }


                // =================================
                // API ERROR
                // =================================

                if (!response.IsSuccessStatusCode)
                {
                    model.ErrorMessage =
                        await GetApiMessageAsync(
                            response);


                    return View(model);
                }


                // =================================
                // READ API DATA
                // =================================

                var pendingResponse =
                    await response.Content
                        .ReadFromJsonAsync<
                            PendingCompanyApiResponse>();


                if (pendingResponse != null)
                {
                    model.PendingCompanyRequestCount =
                        pendingResponse.Count;


                    model.PendingCompanies =
                        pendingResponse.Companies
                        ?? new List<
                            AdminPendingCompanyViewModel>();
                }


                // =================================
                // LOAD LIVE SYSTEM COUNTS
                // =================================

                var dashboardResponse =
                    await client.GetAsync(
                        "api/Admin/dashboard");

                if (dashboardResponse.IsSuccessStatusCode)
                {
                    var dashboard =
                        await dashboardResponse.Content
                            .ReadFromJsonAsync<
                                AdminSystemDashboardApiResponse>();

                    if (dashboard != null)
                    {
                        model.TotalUsers = dashboard.Users.TotalUsers;
                        model.TotalCompanies = dashboard.Companies.TotalCompanies;
                        model.TotalJobs = dashboard.Jobs.TotalJobs;
                        model.TotalApplications = dashboard.Jobs.TotalApplications;
                        model.TotalServiceProviders = dashboard.ServiceProviders.TotalServiceProviders;
                        model.TotalMarketplaceSellers = dashboard.MarketplaceSellers.TotalMarketplaceSellers;
                        model.TotalServiceRequests = dashboard.Services.TotalServiceRequests;
                        model.TotalMarketplaceListings = dashboard.Marketplace.TotalMarketplaceListings;
                        model.NewInquiries = dashboard.Inquiries.NewInquiries;
                    }
                }
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage =
                    "Unable to connect to the WorkHub API.";
            }
            catch
            {
                model.ErrorMessage =
                    "Unable to load the Admin Dashboard.";
            }


            return View(model);
        }


        // =========================================
        // ADMIN LOGIN CHECK
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
        // AUTHENTICATED API CLIENT
        // =========================================

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


        // =========================================
        // CLEAR ADMIN SESSION
        // =========================================

        private void ClearAdminSession()
        {
            HttpContext.Session.Clear();
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