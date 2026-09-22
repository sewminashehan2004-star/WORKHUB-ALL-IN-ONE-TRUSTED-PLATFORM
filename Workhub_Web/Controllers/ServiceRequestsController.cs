using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Workhub_Web.Models;

namespace Workhub_Web.Controllers
{
    [Route("ServiceRequests")]
    public class ServiceRequestsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public ServiceRequestsController(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var token = RequireRegisteredUserToken();
            if (token == null) return RedirectToAction("Login", "Account", new { returnUrl = "/ServiceRequests" });

            var model = new ServiceRequestDashboardViewModel();
            try
            {
                var client = CreateAuthenticatedClient(token);
                var publicClient = _httpClientFactory.CreateClient("WorkHubApi");
                model.Categories = await publicClient.GetFromJsonAsync<List<ServiceCategoryItemViewModel>>("api/Services/categories") ?? new();

                var requestsResponse = await client.GetAsync("api/ServiceRequests/me");
                if (requestsResponse.IsSuccessStatusCode)
                    model.Requests = await requestsResponse.Content.ReadFromJsonAsync<List<MyServiceRequestViewModel>>() ?? new();
                else
                    model.ErrorMessage = await GetApiMessageAsync(requestsResponse);

                foreach (var request in model.Requests.Where(x => x.OfferCount > 0 && x.Status != "Cancelled"))
                {
                    var offersResponse = await client.GetAsync($"api/ServiceOffers/request/{request.ServiceRequestId}");
                    if (offersResponse.IsSuccessStatusCode)
                        model.OffersByRequest[request.ServiceRequestId] = await offersResponse.Content.ReadFromJsonAsync<List<ServiceRequestOfferViewModel>>() ?? new();
                }
            }
            catch (HttpRequestException) { model.ErrorMessage = "Unable to connect to the WorkHub API."; }
            return View(model);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateServiceRequestViewModel newRequest)
        {
            var token = RequireRegisteredUserToken();
            if (token == null) return RedirectToAction("Login", "Account");
            try
            {
                var client = CreateAuthenticatedClient(token);
                var response = await client.PostAsJsonAsync("api/ServiceRequests", new
                {
                    newRequest.ServiceCategoryId,
                    newRequest.Title,
                    newRequest.Description,
                    newRequest.Location,
                    newRequest.PreferredDate,
                    newRequest.Budget
                });
                TempData[response.IsSuccessStatusCode ? "RequestSuccess" : "RequestError"] =
                    response.IsSuccessStatusCode ? "Your service request is now live. Providers can send offers." : await GetApiMessageAsync(response);
            }
            catch (HttpRequestException) { TempData["RequestError"] = "Unable to connect to the WorkHub API."; }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("offers/{offerId:int}/accept")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> AcceptOffer(int offerId) => PatchAction($"api/ServiceOffers/{offerId}/accept", "Offer accepted. The provider has been selected.");

        [HttpPost("offers/{offerId:int}/reject")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> RejectOffer(int offerId) => PatchAction($"api/ServiceOffers/{offerId}/reject", "Offer rejected.");

        [HttpPost("{id:int}/cancel")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Cancel(int id) => PatchAction($"api/ServiceRequests/{id}/cancel", "Service request cancelled.");

        [HttpPost("{id:int}/complete")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Complete(int id) => PatchAction($"api/ServiceRequests/{id}/complete", "Service marked as completed.");

        private async Task<IActionResult> PatchAction(string url, string successMessage)
        {
            var token = RequireRegisteredUserToken();
            if (token == null) return RedirectToAction("Login", "Account");
            try
            {
                var client = CreateAuthenticatedClient(token);
                var response = await client.PatchAsync(url, null);
                TempData[response.IsSuccessStatusCode ? "RequestSuccess" : "RequestError"] =
                    response.IsSuccessStatusCode ? successMessage : await GetApiMessageAsync(response);
            }
            catch (HttpRequestException) { TempData["RequestError"] = "Unable to connect to the WorkHub API."; }
            return RedirectToAction(nameof(Index));
        }

        private string? RequireRegisteredUserToken()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var role = HttpContext.Session.GetString("Role");
            return !string.IsNullOrWhiteSpace(token) && string.Equals(role, "RegisteredUser", StringComparison.OrdinalIgnoreCase) ? token : null;
        }

        private HttpClient CreateAuthenticatedClient(string token)
        {
            var client = _httpClientFactory.CreateClient("WorkHubApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private static async Task<string> GetApiMessageAsync(HttpResponseMessage response)
        {
            var text = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(text)) return $"Request failed ({(int)response.StatusCode}).";
            try
            {
                using var json = JsonDocument.Parse(text);
                if (json.RootElement.TryGetProperty("message", out var message)) return message.GetString() ?? text;
            }
            catch (JsonException) { }
            return text.Trim('"');
        }
    }
}
