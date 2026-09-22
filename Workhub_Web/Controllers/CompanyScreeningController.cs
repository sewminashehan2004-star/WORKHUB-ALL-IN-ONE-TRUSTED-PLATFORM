using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Workhub_Web.Models;

namespace Workhub_Web.Controllers
{
    [Route("Employer/Screening")]
    public class CompanyScreeningController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public CompanyScreeningController(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        [HttpGet("{jobId:int}")]
        public async Task<IActionResult> Index(int jobId)
        {
            var token = HttpContext.Session.GetString("JwtToken") ?? HttpContext.Session.GetString("CompanyJwtToken");
            if (string.IsNullOrWhiteSpace(token)) return RedirectToAction("Login", "CompanyPortal");

            var model = new CompanyScreeningViewModel
            {
                JobId = jobId,
                SuccessMessage = TempData["ScreeningSuccess"]?.ToString(),
                ErrorMessage = TempData["ScreeningError"]?.ToString()
            };

            try
            {
                var client = CreateClient(token);
                var jobResponse = await client.GetAsync($"api/Jobs/{jobId}");
                if (jobResponse.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(await jobResponse.Content.ReadAsStringAsync());
                    if (doc.RootElement.TryGetProperty("title", out var title)) model.JobTitle = title.GetString() ?? $"Job #{jobId}";
                }

                var response = await client.GetAsync($"api/JobScreening/company/job/{jobId}");
                if (!response.IsSuccessStatusCode)
                {
                    model.ErrorMessage = await ReadMessage(response);
                    return View(model);
                }
                model.Questions = await response.Content.ReadFromJsonAsync<List<JobScreeningQuestionViewModel>>() ?? new();
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage = "Unable to connect to the WorkHub API.";
            }
            return View(model);
        }

        [HttpPost("{jobId:int}/Add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int jobId, CompanyScreeningViewModel model)
        {
            var token = HttpContext.Session.GetString("JwtToken") ?? HttpContext.Session.GetString("CompanyJwtToken");
            if (string.IsNullOrWhiteSpace(token)) return RedirectToAction("Login", "CompanyPortal");

            if (string.IsNullOrWhiteSpace(model.NewQuestionText))
            {
                TempData["ScreeningError"] = "Question text is required.";
                return RedirectToAction(nameof(Index), new { jobId });
            }

            var options = (model.NewOptions ?? string.Empty)
                .Split(new[] { '\n', '\r', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            try
            {
                var client = CreateClient(token);
                var response = await client.PostAsJsonAsync($"api/JobScreening/company/job/{jobId}", new
                {
                    questionText = model.NewQuestionText,
                    questionType = model.NewQuestionType,
                    options,
                    isRequired = model.NewQuestionRequired
                });

                TempData[response.IsSuccessStatusCode ? "ScreeningSuccess" : "ScreeningError"] =
                    response.IsSuccessStatusCode ? "Screening question added." : await ReadMessage(response);
            }
            catch (HttpRequestException)
            {
                TempData["ScreeningError"] = "Unable to connect to the WorkHub API.";
            }

            return RedirectToAction(nameof(Index), new { jobId });
        }

        [HttpPost("Question/{questionId:int}/Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int questionId, int jobId)
        {
            var token = HttpContext.Session.GetString("JwtToken") ?? HttpContext.Session.GetString("CompanyJwtToken");
            if (string.IsNullOrWhiteSpace(token)) return RedirectToAction("Login", "CompanyPortal");

            try
            {
                var client = CreateClient(token);
                var response = await client.DeleteAsync($"api/JobScreening/company/question/{questionId}");
                TempData[response.IsSuccessStatusCode ? "ScreeningSuccess" : "ScreeningError"] =
                    response.IsSuccessStatusCode ? "Question deleted." : await ReadMessage(response);
            }
            catch (HttpRequestException)
            {
                TempData["ScreeningError"] = "Unable to connect to the WorkHub API.";
            }
            return RedirectToAction(nameof(Index), new { jobId });
        }

        private HttpClient CreateClient(string token)
        {
            var client = _httpClientFactory.CreateClient("WorkHubApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private static async Task<string> ReadMessage(HttpResponseMessage response)
        {
            var text = await response.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(text);
                if (doc.RootElement.TryGetProperty("message", out var message)) return message.GetString() ?? "Request failed.";
            }
            catch { }
            return "Request failed.";
        }
    }
}
