using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WorkHub_Admin.Models;

namespace WorkHub_Admin.Controllers
{
    [Route("AdminOperations")]
    public class AdminOperationsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public AdminOperationsController(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "AdminAuth");
            var token = HttpContext.Session.GetString("AdminJwtToken")!;
            var model = new AdminOperationsViewModel
            {
                SuccessMessage = TempData["OpsSuccess"]?.ToString(),
                ErrorMessage = TempData["OpsError"]?.ToString()
            };

            try
            {
                var client = CreateClient(token);
                var usersTask = LoadAsync<List<AdminUserItem>>(client, "api/Admin/users");
                var companiesTask = LoadAsync<List<AdminCompanyItem>>(client, "api/Admin/companies");
                var providersTask = LoadAsync<List<AdminProviderItem>>(client, "api/Admin/service-providers");
                var sellersTask = LoadAsync<List<AdminSellerItem>>(client, "api/Admin/marketplace-sellers");
                var jobsTask = LoadAsync<List<AdminJobItem>>(client, "api/Admin/jobs");
                var listingsTask = LoadAsync<List<AdminListingItem>>(client, "api/Admin/marketplace-listings");

                await Task.WhenAll(usersTask, companiesTask, providersTask, sellersTask, jobsTask, listingsTask);
                model.Users = await usersTask ?? new();
                model.Companies = await companiesTask ?? new();
                model.ServiceProviders = await providersTask ?? new();
                model.MarketplaceSellers = await sellersTask ?? new();
                model.Jobs = await jobsTask ?? new();
                model.MarketplaceListings = await listingsTask ?? new();
            }
            catch (UnauthorizedAccessException)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "AdminAuth");
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage = "Unable to connect to the WorkHub API.";
            }
            catch
            {
                model.ErrorMessage = "Unable to load admin operations.";
            }

            return View(model);
        }

        [HttpPost("users/{id:int}/status")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> UpdateUserStatus(int id, string status) =>
            PatchAndReturn($"api/Admin/users/{id}/status", new { status }, "User account status updated.", "users");

        [HttpPost("companies/{id:int}/verification")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> UpdateCompanyVerification(int id, string status) =>
            PatchAndReturn($"api/Admin/companies/{id}/verification", new { status }, "Company verification updated.", "companies");

        [HttpPost("providers/{id:int}/verification")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> UpdateProviderVerification(int id, string status) =>
            PatchAndReturn($"api/Admin/service-providers/{id}/verification", new { status }, "Service provider verification updated.", "providers");

        [HttpPost("sellers/{id:int}/verification")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> UpdateSellerVerification(int id, string status) =>
            PatchAndReturn($"api/Admin/marketplace-sellers/{id}/verification", new { status }, "Marketplace seller verification updated.", "sellers");

        [HttpPost("jobs/{id:int}/status")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> UpdateJobStatus(int id, string status) =>
            PatchAndReturn($"api/Admin/jobs/{id}/status", new { status }, "Job status updated.", "jobs");

        [HttpPost("listings/{id:int}/status")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> UpdateListingStatus(int id, string status) =>
            PatchAndReturn($"api/Admin/marketplace-listings/{id}/status", new { status }, "Marketplace listing status updated.", "listings");

        private async Task<IActionResult> PatchAndReturn(string url, object payload, string success, string anchor)
        {
            if (!IsAdminLoggedIn()) return RedirectToAction("Login", "AdminAuth");
            var token = HttpContext.Session.GetString("AdminJwtToken")!;
            try
            {
                var client = CreateClient(token);
                using var request = new HttpRequestMessage(HttpMethod.Patch, url) { Content = JsonContent.Create(payload) };
                var response = await client.SendAsync(request);
                if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                {
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login", "AdminAuth");
                }
                TempData[response.IsSuccessStatusCode ? "OpsSuccess" : "OpsError"] =
                    response.IsSuccessStatusCode ? success : await GetApiMessageAsync(response);
            }
            catch (HttpRequestException)
            {
                TempData["OpsError"] = "Unable to connect to the WorkHub API.";
            }
            return Redirect($"/AdminOperations#{anchor}");
        }

        private async Task<T?> LoadAsync<T>(HttpClient client, string url)
        {
            var response = await client.GetAsync(url);
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                throw new UnauthorizedAccessException();
            if (!response.IsSuccessStatusCode) return default;
            return await response.Content.ReadFromJsonAsync<T>();
        }

        private bool IsAdminLoggedIn()
        {
            var token = HttpContext.Session.GetString("AdminJwtToken");
            var role = HttpContext.Session.GetString("AdminRole");
            return !string.IsNullOrWhiteSpace(token) && string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        private HttpClient CreateClient(string token)
        {
            var client = _httpClientFactory.CreateClient("WorkHubApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private static async Task<string> GetApiMessageAsync(HttpResponseMessage response)
        {
            var text = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(text);
                if (doc.RootElement.TryGetProperty("message", out var message)) return message.GetString() ?? "Request failed.";
            }
            catch (JsonException) { }
            return string.IsNullOrWhiteSpace(text) ? "Request failed." : text.Trim('"');
        }
    }
}
