using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using Workhub_Web.Models;

namespace Workhub_Web.Controllers
{
    public class ServicesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public ServicesController(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        [HttpGet]
        public async Task<IActionResult> Index(string? search, int? categoryId, string? location)
        {
            var model = new ServicesPageViewModel
            {
                Search = search,
                CategoryId = categoryId,
                Location = location
            };

            try
            {
                var client = _httpClientFactory.CreateClient("WorkHubApi");
                model.Categories = await client.GetFromJsonAsync<List<ServiceCategoryItemViewModel>>("api/Services/categories") ?? new();

                var query = new List<string>();
                if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
                if (categoryId.HasValue) query.Add($"categoryId={categoryId.Value}");
                if (!string.IsNullOrWhiteSpace(location)) query.Add($"location={Uri.EscapeDataString(location)}");

                var url = "api/Services" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
                model.Services = await client.GetFromJsonAsync<List<ServiceCardViewModel>>(url) ?? new();
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage = "Unable to connect to the WorkHub API.";
            }

            return View(model);
        }
    }
}
