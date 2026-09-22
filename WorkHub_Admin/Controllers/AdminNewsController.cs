using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WorkHub_Admin.Models;

namespace WorkHub_Admin.Controllers
{
    public class AdminNewsController : Controller
    {
        private readonly IHttpClientFactory
            _httpClientFactory;


        public AdminNewsController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory =
                httpClientFactory;
        }


        // =========================================
        // INDEX
        //
        // URL:
        // /AdminNews/Index
        //
        // VIEW:
        // Views/AdminNews/Index.cshtml
        // =========================================

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
                new AdminNewsViewModel
                {
                    SuccessMessage =
                        TempData["NewsSuccess"]
                            ?.ToString(),

                    ErrorMessage =
                        TempData["NewsError"]
                            ?.ToString()
                };


            var token =
                GetAdminToken();


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "AdminAuth");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                var response =
                    await client.GetAsync(
                        "api/NewsUpdates/admin");


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
                        await GetApiMessageAsync(
                            response);


                    return View(
                        "Index",
                        model);
                }


                model.News =
                    await response.Content
                        .ReadFromJsonAsync<
                            List<AdminNewsItemViewModel>>()
                    ??
                    new List<AdminNewsItemViewModel>();
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage =
                    "Unable to connect to the WorkHub API.";
            }
            catch (Exception ex)
            {
                model.ErrorMessage =
                    "Unable to load News & Updates. "
                    +
                    ex.Message;
            }


            return View(
                "Index",
                model);
        }


        // =========================================
        // CREATE PAGE
        //
        // URL:
        // /AdminNews/Create
        // =========================================

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "AdminAuth");
            }


            return View(
                "Create",
                new AdminNewsFormViewModel());
        }


        // =========================================
        // CREATE NEWS
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            AdminNewsFormViewModel model)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "AdminAuth");
            }


            ValidateNewsModel(model);


            if (!ModelState.IsValid)
            {
                return View(
                    "Create",
                    model);
            }


            var token =
                GetAdminToken();


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "AdminAuth");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                var request =
                    new
                    {
                        title =
                            model.Title.Trim(),

                        summary =
                            model.Summary.Trim(),

                        content =
                            model.Content.Trim(),

                        category =
                            string.IsNullOrWhiteSpace(
                                model.Category)
                                ? "General"
                                : model.Category.Trim(),

                        isPublished =
                            model.IsPublished
                    };


                var response =
                    await client.PostAsJsonAsync(
                        "api/NewsUpdates/admin",
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
                    model.ErrorMessage =
                        await GetApiMessageAsync(
                            response);


                    return View(
                        "Create",
                        model);
                }


                TempData["NewsSuccess"] =
                    model.IsPublished
                        ? "News published successfully."
                        : "News saved as draft successfully.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage =
                    "Unable to connect to the WorkHub API.";


                return View(
                    "Create",
                    model);
            }
            catch (Exception ex)
            {
                model.ErrorMessage =
                    "Unable to save news. "
                    +
                    ex.Message;


                return View(
                    "Create",
                    model);
            }
        }


        // =========================================
        // EDIT PAGE
        //
        // URL:
        // /AdminNews/Edit/1
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Edit(
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
                TempData["NewsError"] =
                    "Invalid news item.";


                return RedirectToAction(
                    nameof(Index));
            }


            var token =
                GetAdminToken();


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "AdminAuth");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                var response =
                    await client.GetAsync(
                        $"api/NewsUpdates/admin/{id}");


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
                    TempData["NewsError"] =
                        "News item not found.";


                    return RedirectToAction(
                        nameof(Index));
                }


                if (!response.IsSuccessStatusCode)
                {
                    TempData["NewsError"] =
                        await GetApiMessageAsync(
                            response);


                    return RedirectToAction(
                        nameof(Index));
                }


                var item =
                    await response.Content
                        .ReadFromJsonAsync<
                            AdminNewsItemViewModel>();


                if (item == null)
                {
                    TempData["NewsError"] =
                        "Unable to load news item.";


                    return RedirectToAction(
                        nameof(Index));
                }


                var model =
                    new AdminNewsFormViewModel
                    {
                        NewsUpdateId =
                            item.NewsUpdateId,

                        Title =
                            item.Title,

                        Summary =
                            item.Summary
                            ??
                            string.Empty,

                        Content =
                            item.Content,

                        Category =
                            item.Category,

                        IsPublished =
                            item.IsPublished
                    };


                return View(
                    "Edit",
                    model);
            }
            catch (HttpRequestException)
            {
                TempData["NewsError"] =
                    "Unable to connect to the WorkHub API.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["NewsError"] =
                    "Unable to load news item. "
                    +
                    ex.Message;


                return RedirectToAction(
                    nameof(Index));
            }
        }


        // =========================================
        // EDIT NEWS
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            AdminNewsFormViewModel model)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction(
                    "Login",
                    "AdminAuth");
            }


            if (model.NewsUpdateId <= 0)
            {
                TempData["NewsError"] =
                    "Invalid news item.";


                return RedirectToAction(
                    nameof(Index));
            }


            ValidateNewsModel(model);


            if (!ModelState.IsValid)
            {
                return View(
                    "Edit",
                    model);
            }


            var token =
                GetAdminToken();


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "AdminAuth");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                var request =
                    new
                    {
                        title =
                            model.Title.Trim(),

                        summary =
                            model.Summary.Trim(),

                        content =
                            model.Content.Trim(),

                        category =
                            string.IsNullOrWhiteSpace(
                                model.Category)
                                ? "General"
                                : model.Category.Trim(),

                        isPublished =
                            model.IsPublished
                    };


                var response =
                    await client.PutAsJsonAsync(
                        $"api/NewsUpdates/admin/{model.NewsUpdateId}",
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
                    model.ErrorMessage =
                        await GetApiMessageAsync(
                            response);


                    return View(
                        "Edit",
                        model);
                }


                TempData["NewsSuccess"] =
                    "News item updated successfully.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage =
                    "Unable to connect to the WorkHub API.";


                return View(
                    "Edit",
                    model);
            }
            catch (Exception ex)
            {
                model.ErrorMessage =
                    "Unable to update news. "
                    +
                    ex.Message;


                return View(
                    "Edit",
                    model);
            }
        }


        // =========================================
        // DELETE NEWS
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
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
                TempData["NewsError"] =
                    "Invalid news item.";


                return RedirectToAction(
                    nameof(Index));
            }


            var token =
                GetAdminToken();


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "AdminAuth");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                var response =
                    await client.DeleteAsync(
                        $"api/NewsUpdates/admin/{id}");


                if (IsUnauthorized(response))
                {
                    HttpContext.Session.Clear();


                    return RedirectToAction(
                        "Login",
                        "AdminAuth");
                }


                if (!response.IsSuccessStatusCode)
                {
                    TempData["NewsError"] =
                        await GetApiMessageAsync(
                            response);
                }
                else
                {
                    TempData["NewsSuccess"] =
                        "News item deleted successfully.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["NewsError"] =
                    "Unable to connect to the WorkHub API.";
            }
            catch (Exception ex)
            {
                TempData["NewsError"] =
                    "Unable to delete news item. "
                    +
                    ex.Message;
            }


            return RedirectToAction(
                nameof(Index));
        }


        // =========================================
        // VALIDATION
        // =========================================

        private void ValidateNewsModel(
            AdminNewsFormViewModel model)
        {
            model.Title =
                model.Title?.Trim()
                ??
                string.Empty;


            model.Summary =
                model.Summary?.Trim()
                ??
                string.Empty;


            model.Content =
                model.Content?.Trim()
                ??
                string.Empty;


            model.Category =
                model.Category?.Trim()
                ??
                "General";


            if (string.IsNullOrWhiteSpace(
                    model.Title))
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    "Title is required.");
            }


            if (model.Title.Length > 200)
            {
                ModelState.AddModelError(
                    nameof(model.Title),
                    "Title cannot exceed 200 characters.");
            }


            if (model.Summary.Length > 500)
            {
                ModelState.AddModelError(
                    nameof(model.Summary),
                    "Summary cannot exceed 500 characters.");
            }


            if (string.IsNullOrWhiteSpace(
                    model.Content))
            {
                ModelState.AddModelError(
                    nameof(model.Content),
                    "Content is required.");
            }


            if (model.Category.Length > 100)
            {
                ModelState.AddModelError(
                    nameof(model.Category),
                    "Category cannot exceed 100 characters.");
            }
        }


        // =========================================
        // ADMIN TOKEN
        // =========================================

        private string? GetAdminToken()
        {
            return
                HttpContext.Session
                    .GetString(
                        "JwtToken")
                ??
                HttpContext.Session
                    .GetString(
                        "AdminJwtToken")
                ??
                HttpContext.Session
                    .GetString(
                        "Token");
        }


        // =========================================
        // ADMIN LOGIN CHECK
        //
        // IMPORTANT:
        // Do NOT require Session Role = Admin here.
        //
        // Existing admin login already stores
        // the valid admin JWT token.
        // =========================================

        private bool IsAdminLoggedIn()
        {
            return
                !string.IsNullOrWhiteSpace(
                    GetAdminToken());
        }


        // =========================================
        // AUTHENTICATED API CLIENT
        // =========================================

        private HttpClient
            CreateAuthenticatedClient(
                string token)
        {
            var client =
                _httpClientFactory
                    .CreateClient(
                        "WorkHubApi");


            client.DefaultRequestHeaders
                .Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);


            return client;
        }


        // =========================================
        // UNAUTHORIZED CHECK
        // =========================================

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


                if (!string.IsNullOrWhiteSpace(
                        content))
                {
                    using var document =
                        JsonDocument.Parse(
                            content);


                    if (
                        document.RootElement
                            .TryGetProperty(
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