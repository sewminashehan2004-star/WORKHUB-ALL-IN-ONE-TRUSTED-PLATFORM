using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WorkHub_Admin.Models;

namespace WorkHub_Admin.Controllers
{
    public class AdminInquiriesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminInquiriesController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "AdminAuth");
            }

            var model =
                new AdminInquiriesViewModel
                {
                    SuccessMessage =
                        TempData["InquirySuccess"]?.ToString(),

                    ErrorMessage =
                        TempData["InquiryError"]?.ToString()
                };

            var token = GetAdminToken();

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "AdminAuth");
            }

            try
            {
                var client =
                    CreateAuthenticatedClient(token);

                var response =
                    await client.GetAsync(
                        "api/ContactInquiries/admin");

                if (IsUnauthorized(response))
                {
                    HttpContext.Session.Clear();

                    return RedirectToAction(
                        "Login",
                        "AdminAuth");
                }

                if (!response.IsSuccessStatusCode)
                {
                    model.ErrorMessage =
                        await GetApiMessageAsync(response);

                    return View(model);
                }

                model.Inquiries =
                    await response.Content
                        .ReadFromJsonAsync<
                            List<AdminInquiryItemViewModel>>()
                    ??
                    new List<AdminInquiryItemViewModel>();
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage =
                    "Unable to connect to the WorkHub API.";
            }

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "AdminAuth");
            }

            if (id <= 0)
            {
                TempData["InquiryError"] =
                    "Invalid inquiry.";

                return RedirectToAction(
                    nameof(Index));
            }

            var token = GetAdminToken();

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "AdminAuth");
            }

            try
            {
                var client =
                    CreateAuthenticatedClient(token);

                var response =
                    await client.GetAsync(
                        $"api/ContactInquiries/admin/{id}");

                if (IsUnauthorized(response))
                {
                    HttpContext.Session.Clear();

                    return RedirectToAction(
                        "Login",
                        "AdminAuth");
                }

                if (response.StatusCode ==
                    HttpStatusCode.NotFound)
                {
                    TempData["InquiryError"] =
                        "Inquiry not found.";

                    return RedirectToAction(
                        nameof(Index));
                }

                if (!response.IsSuccessStatusCode)
                {
                    TempData["InquiryError"] =
                        await GetApiMessageAsync(response);

                    return RedirectToAction(
                        nameof(Index));
                }

                var model =
                    await response.Content
                        .ReadFromJsonAsync<
                            AdminInquiryDetailsViewModel>();

                if (model == null)
                {
                    TempData["InquiryError"] =
                        "Unable to load inquiry.";

                    return RedirectToAction(
                        nameof(Index));
                }

                model.SuccessMessage =
                    TempData["InquirySuccess"]?.ToString();

                model.ErrorMessage =
                    TempData["InquiryError"]?.ToString();

                return View(model);
            }
            catch (HttpRequestException)
            {
                TempData["InquiryError"] =
                    "Unable to connect to the WorkHub API.";

                return RedirectToAction(
                    nameof(Index));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(
            int id,
            string replyMessage)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "AdminAuth");
            }

            replyMessage =
                replyMessage?.Trim()
                ??
                string.Empty;

            if (id <= 0)
            {
                TempData["InquiryError"] =
                    "Invalid inquiry.";

                return RedirectToAction(
                    nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(
                    replyMessage))
            {
                TempData["InquiryError"] =
                    "Please enter a reply.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            if (replyMessage.Length > 4000)
            {
                TempData["InquiryError"] =
                    "Reply cannot exceed 4000 characters.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            var token = GetAdminToken();

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "AdminAuth");
            }

            try
            {
                var client =
                    CreateAuthenticatedClient(token);

                var request =
                    new
                    {
                        replyMessage
                    };

                var response =
                    await client.PutAsJsonAsync(
                        $"api/ContactInquiries/admin/{id}/reply",
                        request);

                if (IsUnauthorized(response))
                {
                    HttpContext.Session.Clear();

                    return RedirectToAction(
                        "Login",
                        "AdminAuth");
                }

                if (!response.IsSuccessStatusCode)
                {
                    TempData["InquiryError"] =
                        await GetApiMessageAsync(response);

                    return RedirectToAction(
                        nameof(Details),
                        new { id });
                }

                TempData["InquirySuccess"] =
                    "Reply saved successfully.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }
            catch (HttpRequestException)
            {
                TempData["InquiryError"] =
                    "Unable to connect to the WorkHub API.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }
        }


        private string? GetAdminToken()
        {
            return
                HttpContext.Session.GetString("JwtToken")
                ??
                HttpContext.Session.GetString(
                    "AdminJwtToken")
                ??
                HttpContext.Session.GetString("Token");
        }


        private bool IsAdminLoggedIn()
        {
            return
                !string.IsNullOrWhiteSpace(
                    GetAdminToken());
        }


        private HttpClient CreateAuthenticatedClient(
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


        private static bool IsUnauthorized(
            HttpResponseMessage response)
        {
            return
                response.StatusCode ==
                HttpStatusCode.Unauthorized
                ||
                response.StatusCode ==
                HttpStatusCode.Forbidden;
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

                if (!string.IsNullOrWhiteSpace(content))
                {
                    using var document =
                        JsonDocument.Parse(content);

                    if (
                        document.RootElement.TryGetProperty(
                            "message",
                            out var message)
                    )
                    {
                        return
                            message.GetString()
                            ??
                            "Unable to complete the request.";
                    }
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