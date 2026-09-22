using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Workhub_Web.Models;

namespace Workhub_Web.Controllers
{
    [Route("Marketplace/Messages")]
    public class MarketplaceMessagesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public MarketplaceMessagesController(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var token = GetToken();
            if (token == null) return RedirectToAction("Login", "Account");

            var model = new MarketplaceInboxViewModel
            {
                SuccessMessage = TempData["MessageSuccess"]?.ToString(),
                ErrorMessage = TempData["MessageError"]?.ToString()
            };

            try
            {
                var client = CreateClient(token);
                var response = await client.GetAsync("api/MarketplaceMessages/conversations");
                if (response.StatusCode is System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden)
                    return RedirectToAction("Login", "Account");
                if (!response.IsSuccessStatusCode)
                {
                    model.ErrorMessage = "Unable to load marketplace messages.";
                    return View(model);
                }
                model.Conversations = await response.Content.ReadFromJsonAsync<List<MarketplaceConversationItemViewModel>>() ?? new();
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage = "Unable to connect to the WorkHub API.";
            }

            return View(model);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Chat(int id)
        {
            var token = GetToken();
            if (token == null) return RedirectToAction("Login", "Account");

            try
            {
                var client = CreateClient(token);
                var response = await client.GetAsync($"api/MarketplaceMessages/conversations/{id}");
                if (!response.IsSuccessStatusCode) return RedirectToAction(nameof(Index));
                var api = await response.Content.ReadFromJsonAsync<MarketplaceChatApiResponse>();
                if (api == null) return RedirectToAction(nameof(Index));
                return View(new MarketplaceChatViewModel { Conversation = api.Conversation, Messages = api.Messages });
            }
            catch (HttpRequestException)
            {
                return View(new MarketplaceChatViewModel { ErrorMessage = "Unable to connect to the WorkHub API." });
            }
        }

        [HttpPost("{id:int}/send")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int id, string message)
        {
            var token = GetToken();
            if (token == null) return RedirectToAction("Login", "Account");
            if (string.IsNullOrWhiteSpace(message)) return RedirectToAction(nameof(Chat), new { id });

            try
            {
                var client = CreateClient(token);
                var response = await client.PostAsJsonAsync($"api/MarketplaceMessages/conversations/{id}/messages", new { message });
                if (!response.IsSuccessStatusCode)
                    TempData["MessageError"] = "Unable to send the message.";
            }
            catch (HttpRequestException)
            {
                TempData["MessageError"] = "Unable to connect to the WorkHub API.";
            }
            return RedirectToAction(nameof(Chat), new { id });
        }

        private string? GetToken() => HttpContext.Session.GetString("JwtToken");
        private HttpClient CreateClient(string token)
        {
            var client = _httpClientFactory.CreateClient("WorkHubApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
}
