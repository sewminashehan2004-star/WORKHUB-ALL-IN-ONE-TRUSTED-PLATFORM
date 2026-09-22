using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Workhub_Web.Models;

namespace Workhub_Web.Controllers
{
    public class MarketplaceController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MarketplaceController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            int? categoryId,
            string? listingType,
            string? location,
            decimal? minPrice,
            decimal? maxPrice)
        {
            var model = new MarketplacePageViewModel
            {
                Search = search,
                CategoryId = categoryId,
                ListingType = listingType,
                Location = location,
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            try
            {
                var client = _httpClientFactory.CreateClient("WorkHubApi");
                model.Categories =
                    await client.GetFromJsonAsync<List<MarketplaceCategoryItemViewModel>>("api/Marketplace/categories")
                    ?? new();

                var query = new List<string>();
                if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
                if (categoryId.HasValue) query.Add($"categoryId={categoryId.Value}");
                if (!string.IsNullOrWhiteSpace(listingType)) query.Add($"listingType={Uri.EscapeDataString(listingType)}");
                if (!string.IsNullOrWhiteSpace(location)) query.Add($"location={Uri.EscapeDataString(location)}");
                if (minPrice.HasValue) query.Add($"minPrice={minPrice.Value}");
                if (maxPrice.HasValue) query.Add($"maxPrice={maxPrice.Value}");

                var url = "api/Marketplace" +
                          (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);

                model.Listings =
                    await client.GetFromJsonAsync<List<MarketplaceListingCardViewModel>>(url)
                    ?? new();
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage = "Unable to connect to the WorkHub API.";
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("WorkHubApi");
                var response = await client.GetAsync($"api/Marketplace/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var model = await response.Content.ReadFromJsonAsync<MarketplaceDetailsViewModel>();
                if (model == null)
                {
                    return NotFound();
                }

                model.ErrorMessage = TempData["MarketplaceError"]?.ToString();
                return View(model);
            }
            catch (HttpRequestException)
            {
                return View(new MarketplaceDetailsViewModel
                {
                    MarketplaceListingId = id,
                    ErrorMessage = "Unable to connect to the WorkHub API."
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MessageSeller(int id, string? message)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrWhiteSpace(token) ||
                !string.Equals(role, "RegisteredUser", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Login", "Account", new
                {
                    returnUrl = Url.Action("Details", "Marketplace", new { id })
                });
            }

            try
            {
                var client = _httpClientFactory.CreateClient("WorkHubApi");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsync(
                    $"api/MarketplaceMessages/listing/{id}/conversation",
                    null);

                if (!response.IsSuccessStatusCode)
                {
                    TempData["MarketplaceError"] = await ReadApiMessageAsync(response);
                    return RedirectToAction(nameof(Details), new { id });
                }

                var result =
                    await response.Content.ReadFromJsonAsync<StartConversationResponse>();

                if (result == null)
                {
                    TempData["MarketplaceError"] = "Unable to start the marketplace conversation.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                if (!string.IsNullOrWhiteSpace(message))
                {
                    var cleanMessage = message.Trim();
                    if (cleanMessage.Length > 2000)
                    {
                        cleanMessage = cleanMessage[..2000];
                    }

                    var messageResponse = await client.PostAsJsonAsync(
                        $"api/MarketplaceMessages/conversations/{result.MarketplaceConversationId}/messages",
                        new { message = cleanMessage });

                    if (!messageResponse.IsSuccessStatusCode)
                    {
                        TempData["MessageError"] = await ReadApiMessageAsync(messageResponse);
                    }
                }

                return RedirectToAction(
                    "Chat",
                    "MarketplaceMessages",
                    new { id = result.MarketplaceConversationId });
            }
            catch (HttpRequestException)
            {
                TempData["MarketplaceError"] = "Unable to connect to the WorkHub API.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        private static async Task<string> ReadApiMessageAsync(HttpResponseMessage response)
        {
            var text = await response.Content.ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(text);
                if (doc.RootElement.TryGetProperty("message", out var message))
                {
                    return message.GetString() ?? "Request failed.";
                }
            }
            catch
            {
                // Ignore invalid JSON and use a friendly fallback.
            }

            return "Request failed.";
        }

        private sealed class StartConversationResponse
        {
            public int MarketplaceConversationId { get; set; }
        }
    }
}
