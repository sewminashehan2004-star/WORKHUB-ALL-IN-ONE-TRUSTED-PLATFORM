using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Workhub_Web.Models;

namespace Workhub_Web.Controllers
{
    [Route("CVMatch")]
    public class CvAnalysisController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public CvAnalysisController(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

        [HttpGet("Job/{jobId:int}")]
        public async Task<IActionResult> Checkout(int jobId, string plan = "MATCH_ONLY")
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("JwtToken")))
                return RedirectToAction("Login", "Account");

            var model = new CvAnalysisCheckoutViewModel
            {
                JobId = jobId,
                PlanCode = string.Equals(plan, "MATCH_PLUS", StringComparison.OrdinalIgnoreCase) ? "MATCH_PLUS" : "MATCH_ONLY",
                ExpiryMonth = DateTime.UtcNow.Month,
                ExpiryYear = DateTime.UtcNow.Year + 2
            };

            try
            {
                var client = _httpClientFactory.CreateClient("WorkHubApi");
                var response = await client.GetAsync($"api/Jobs/{jobId}");
                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    if (doc.RootElement.TryGetProperty("title", out var title)) model.JobTitle = title.GetString() ?? "Selected job";
                }
            }
            catch { }

            return View(model);
        }

        [HttpPost("Job/{jobId:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(int jobId, CvAnalysisCheckoutViewModel model)
        {
            model.JobId = jobId;
            if (!ModelState.IsValid) return View(model);

            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrWhiteSpace(token)) return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient("WorkHubApi");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var payload = new
                {
                    jobId,
                    planCode = model.PlanCode,
                    payment = new
                    {
                        cardHolderName = model.CardHolderName,
                        cardNumber = model.CardNumber,
                        expiryMonth = model.ExpiryMonth,
                        expiryYear = model.ExpiryYear,
                        cvv = model.Cvv
                    }
                };

                var response = await client.PostAsJsonAsync("api/Payments/cv-analysis", payload);
                if (!response.IsSuccessStatusCode)
                {
                    model.ErrorMessage = await ReadMessage(response);
                    model.CardNumber = string.Empty;
                    model.Cvv = string.Empty;
                    return View(model);
                }

                var result = await response.Content.ReadFromJsonAsync<CvAnalysisPurchaseApiResponse>();
                if (result != null)
                {
                    model.JobTitle = result.Job.Title;
                    model.Report = result.Report;
                    model.Payment = result.Payment;
                }
                model.CardNumber = string.Empty;
                model.Cvv = string.Empty;
                return View(model);
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage = "Unable to connect to the WorkHub API.";
                model.CardNumber = string.Empty;
                model.Cvv = string.Empty;
                return View(model);
            }
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
