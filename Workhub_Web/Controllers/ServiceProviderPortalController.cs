using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Workhub_Web.Models;

namespace Workhub_Web.Controllers
{
    [Route("Provider")]
    public class ServiceProviderPortalController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public ServiceProviderPortalController(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var token = RequireRegisteredUserToken();
            if (token == null) return RedirectToAction("Login", "Account", new { returnUrl = "/Provider" });

            var model = new ServiceProviderPortalViewModel();
            try
            {
                var client = CreateAuthenticatedClient(token);
                var publicClient = _httpClientFactory.CreateClient("WorkHubApi");

                model.Categories = await publicClient.GetFromJsonAsync<List<ServiceCategoryItemViewModel>>("api/Services/categories") ?? new();
                model.OpenRequests = await publicClient.GetFromJsonAsync<List<OpenServiceRequestViewModel>>("api/ServiceRequests/open") ?? new();

                var profileResponse = await client.GetAsync("api/ServiceProvider/profile");
                if (profileResponse.IsSuccessStatusCode)
                    model.Profile = await profileResponse.Content.ReadFromJsonAsync<ServiceProviderProfileEditViewModel>() ?? new();
                else
                    model.ErrorMessage = await GetApiMessageAsync(profileResponse);

                var offeringsResponse = await client.GetAsync("api/ServiceProvider/offerings");
                if (offeringsResponse.IsSuccessStatusCode)
                    model.Offerings = await offeringsResponse.Content.ReadFromJsonAsync<List<MyServiceOfferingViewModel>>() ?? new();

                var offersResponse = await client.GetAsync("api/ServiceOffers/me");
                if (offersResponse.IsSuccessStatusCode)
                    model.MyOffers = await offersResponse.Content.ReadFromJsonAsync<List<MyServiceOfferViewModel>>() ?? new();
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage = "Unable to connect to the WorkHub API.";
            }

            return View(model);
        }

        [HttpPost("profile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ServiceProviderProfileEditViewModel profile)
        {
            var token = RequireRegisteredUserToken();
            if (token == null) return RedirectToAction("Login", "Account");

            try
            {
                var client = CreateAuthenticatedClient(token);
                var response = await client.PutAsJsonAsync("api/ServiceProvider/profile", new
                {
                    profile.DisplayName,
                    profile.Bio,
                    profile.Location,
                    profile.YearsOfExperience,
                    profile.StartingPrice
                });
                TempData[response.IsSuccessStatusCode ? "ProviderSuccess" : "ProviderError"] =
                    response.IsSuccessStatusCode ? "Service provider profile updated." : await GetApiMessageAsync(response);
            }
            catch (HttpRequestException)
            {
                TempData["ProviderError"] = "Unable to connect to the WorkHub API.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("offerings")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOffering(ServiceOfferingCreateViewModel newOffering)
        {
            var token = RequireRegisteredUserToken();
            if (token == null) return RedirectToAction("Login", "Account");

            try
            {
                var client = CreateAuthenticatedClient(token);
                var response = await client.PostAsJsonAsync("api/ServiceProvider/offerings", new
                {
                    newOffering.ServiceCategoryId,
                    newOffering.Title,
                    newOffering.Description,
                    newOffering.StartingPrice,
                    newOffering.Location,
                    newOffering.IsAvailable
                });
                TempData[response.IsSuccessStatusCode ? "ProviderSuccess" : "ProviderError"] =
                    response.IsSuccessStatusCode ? "Service published successfully." : await GetApiMessageAsync(response);
            }
            catch (HttpRequestException)
            {
                TempData["ProviderError"] = "Unable to connect to the WorkHub API.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("offerings/{id:int}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOffering(int id)
        {
            var token = RequireRegisteredUserToken();
            if (token == null) return RedirectToAction("Login", "Account");
            try
            {
                var client = CreateAuthenticatedClient(token);
                var response = await client.DeleteAsync($"api/ServiceProvider/offerings/{id}");
                TempData[response.IsSuccessStatusCode ? "ProviderSuccess" : "ProviderError"] =
                    response.IsSuccessStatusCode ? "Service offering removed." : await GetApiMessageAsync(response);
            }
            catch (HttpRequestException) { TempData["ProviderError"] = "Unable to connect to the WorkHub API."; }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("requests/{requestId:int}/offer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendOffer(int requestId, decimal offeredPrice, string? message)
        {
            var token = RequireRegisteredUserToken();
            if (token == null) return RedirectToAction("Login", "Account");
            try
            {
                var client = CreateAuthenticatedClient(token);
                var response = await client.PostAsJsonAsync($"api/ServiceOffers/request/{requestId}", new { offeredPrice, message });
                TempData[response.IsSuccessStatusCode ? "ProviderSuccess" : "ProviderError"] =
                    response.IsSuccessStatusCode ? "Your offer was sent to the customer." : await GetApiMessageAsync(response);
            }
            catch (HttpRequestException) { TempData["ProviderError"] = "Unable to connect to the WorkHub API."; }
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
