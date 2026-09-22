using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Workhub_Web.Models;

namespace Workhub_Web.Controllers
{
    [Route("CareerIntelligence")]
    public class CareerIntelligenceController
        : Controller
    {
        private readonly IHttpClientFactory
            _httpClientFactory;


        public CareerIntelligenceController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory =
                httpClientFactory;
        }


        // =========================================
        // PAGE
        // GET: /CareerIntelligence
        // =========================================

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var token =
                HttpContext.Session.GetString(
                    "JwtToken");


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            var model =
                new CareerIntelligenceViewModel();


            try
            {
                model.Categories =
                    await LoadCategoriesAsync();
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage =
                    "Unable to connect to the WorkHub API.";
            }


            return View(model);
        }


        // =========================================
        // VIEW CAREER BENCHMARK
        // POST: /CareerIntelligence
        // =========================================

        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            CareerIntelligenceViewModel model)
        {
            var token =
                HttpContext.Session.GetString(
                    "JwtToken");


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            try
            {
                await PrepareSelectionsAsync(
                    model);


                if (!ValidateSelection(
                        model))
                {
                    return View(model);
                }


                model.Benchmark =
                    await LoadBenchmarkAsync(
                        model.SelectedCareerRoleId,
                        token);


                if (model.Benchmark == null)
                {
                    model.ErrorMessage =
                        "Unable to load the career benchmark.";
                }


                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                HttpContext.Session.Clear();

                return RedirectToAction(
                    "Login",
                    "Account");
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage =
                    "Unable to connect to the WorkHub API.";

                return View(model);
            }
        }


        // =========================================
        // COMPARE MY CV
        // POST:
        // /CareerIntelligence/CompareMyCV
        // =========================================

        [HttpPost("CompareMyCV")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            CompareMyCV(
                CareerIntelligenceViewModel model)
        {
            var token =
                HttpContext.Session.GetString(
                    "JwtToken");


            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            try
            {
                // =================================
                // LOAD CATEGORY + ROLE
                // =================================

                await PrepareSelectionsAsync(
                    model);


                if (!ValidateSelection(
                        model))
                {
                    return View(
                        "Index",
                        model);
                }


                // =================================
                // RELOAD BENCHMARK
                // =================================

                model.Benchmark =
                    await LoadBenchmarkAsync(
                        model.SelectedCareerRoleId,
                        token);


                if (model.Benchmark == null)
                {
                    model.ErrorMessage =
                        "Unable to load the career benchmark.";

                    return View(
                        "Index",
                        model);
                }


                // =================================
                // AUTHENTICATED CLIENT
                // =================================

                var client =
                    CreateAuthenticatedClient(
                        token);


                // =================================
                // CALL CV INTELLIGENCE API
                // =================================

                var response =
                    await client.PostAsync(
                        $"api/CVIntelligence/analyse/{model.SelectedCareerRoleId}",
                        null);


                // =================================
                // TOKEN EXPIRED
                // =================================

                if (response.StatusCode ==
                    HttpStatusCode.Unauthorized)
                {
                    HttpContext.Session.Clear();

                    return RedirectToAction(
                        "Login",
                        "Account");
                }


                // =================================
                // FORBIDDEN
                // =================================

                if (response.StatusCode ==
                    HttpStatusCode.Forbidden)
                {
                    model.ErrorMessage =
                        "CV Intelligence is available to registered Job Seeker users.";

                    return View(
                        "Index",
                        model);
                }


                // =================================
                // API ERROR
                // =================================

                if (!response.IsSuccessStatusCode)
                {
                    model.ErrorMessage =
                        await GetApiMessageAsync(
                            response);

                    return View(
                        "Index",
                        model);
                }


                // =================================
                // ANALYSIS RESULT
                // =================================

                model.CVAnalysis =
                    await response.Content
                        .ReadFromJsonAsync<
                            CVIntelligenceApiResponse>();


                if (model.CVAnalysis == null)
                {
                    model.ErrorMessage =
                        "WorkHub could not read the CV Intelligence result.";
                }
                else
                {
                    model.SuccessMessage =
                        "Your CV analysis has been completed.";
                }


                return View(
                    "Index",
                    model);
            }
            catch (UnauthorizedAccessException)
            {
                HttpContext.Session.Clear();

                return RedirectToAction(
                    "Login",
                    "Account");
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage =
                    "Unable to connect to the WorkHub API.";

                return View(
                    "Index",
                    model);
            }
        }


        // =========================================
        // AJAX - ROLES BY CATEGORY
        // =========================================

        [HttpGet("roles")]
        public async Task<IActionResult> Roles(
            int categoryId)
        {
            var token =
                HttpContext.Session.GetString(
                    "JwtToken");


            if (string.IsNullOrWhiteSpace(token))
            {
                return Unauthorized(new
                {
                    message =
                        "Please sign in again."
                });
            }


            if (categoryId <= 0)
            {
                return Json(new
                {
                    success = false,

                    roles =
                        Array.Empty<object>()
                });
            }


            try
            {
                var roles =
                    await LoadRolesAsync(
                        categoryId);


                return Json(new
                {
                    success = true,

                    roles =
                        roles.Select(x => new
                        {
                            careerRoleId =
                                x.CareerRoleId,

                            roleName =
                                x.RoleName
                        })
                });
            }
            catch (HttpRequestException)
            {
                return StatusCode(
                    StatusCodes
                        .Status503ServiceUnavailable,
                    new
                    {
                        message =
                            "Unable to connect to the WorkHub API."
                    });
            }
        }


        // =========================================
        // PREPARE PAGE DATA
        // =========================================

        private async Task PrepareSelectionsAsync(
            CareerIntelligenceViewModel model)
        {
            model.Categories =
                await LoadCategoriesAsync();


            if (model.SelectedCareerCategoryId > 0)
            {
                model.Roles =
                    await LoadRolesAsync(
                        model
                            .SelectedCareerCategoryId);
            }
        }


        // =========================================
        // VALIDATE CATEGORY + ROLE
        // =========================================

        private static bool ValidateSelection(
            CareerIntelligenceViewModel model)
        {
            var validCategory =
                model.Categories.Any(x =>
                    x.CareerCategoryId ==
                    model
                        .SelectedCareerCategoryId);


            if (!validCategory)
            {
                model.ErrorMessage =
                    "Please select a valid career category.";

                return false;
            }


            var validRole =
                model.Roles.Any(x =>
                    x.CareerRoleId ==
                    model.SelectedCareerRoleId);


            if (!validRole)
            {
                model.ErrorMessage =
                    "Please select a valid target career role.";

                return false;
            }


            return true;
        }


        // =========================================
        // LOAD BENCHMARK
        // =========================================

        private async Task<CareerBenchmarkViewModel?>
            LoadBenchmarkAsync(
                int careerRoleId,
                string token)
        {
            var client =
                CreateAuthenticatedClient(
                    token);


            var response =
                await client.GetAsync(
                    $"api/CareerBenchmark/role/{careerRoleId}");


            if (response.StatusCode ==
                HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }


            if (!response.IsSuccessStatusCode)
            {
                return null;
            }


            return await response.Content
                .ReadFromJsonAsync<
                    CareerBenchmarkViewModel>();
        }


        // =========================================
        // LOAD CATEGORIES
        // =========================================

        private async Task<
            List<CareerCategoryOptionViewModel>>
            LoadCategoriesAsync()
        {
            var client =
                _httpClientFactory.CreateClient(
                    "WorkHubApi");


            var response =
                await client.GetAsync(
                    "api/CareerCatalog/categories");


            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    "Unable to load career categories.");
            }


            return
                await response.Content
                    .ReadFromJsonAsync<
                        List<
                            CareerCategoryOptionViewModel>>()
                ??
                new List<
                    CareerCategoryOptionViewModel>();
        }


        // =========================================
        // LOAD ROLES
        // =========================================

        private async Task<
            List<CareerRoleOptionViewModel>>
            LoadRolesAsync(
                int categoryId)
        {
            var client =
                _httpClientFactory.CreateClient(
                    "WorkHubApi");


            var response =
                await client.GetAsync(
                    $"api/CareerCatalog/categories/{categoryId}/roles");


            if (!response.IsSuccessStatusCode)
            {
                return new List<
                    CareerRoleOptionViewModel>();
            }


            var result =
                await response.Content
                    .ReadFromJsonAsync<
                        CareerRolesApiResponse>();


            return result?.Roles
                ??
                new List<
                    CareerRoleOptionViewModel>();
        }


        // =========================================
        // AUTHENTICATED CLIENT
        // =========================================

        private HttpClient
            CreateAuthenticatedClient(
                string token)
        {
            var client =
                _httpClientFactory.CreateClient(
                    "WorkHubApi");


            client.DefaultRequestHeaders
                .Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);


            return client;
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


                if (string.IsNullOrWhiteSpace(
                        content))
                {
                    return
                        "Unable to complete the request.";
                }


                using var document =
                    JsonDocument.Parse(
                        content);


                if (document.RootElement
                    .TryGetProperty(
                        "message",
                        out var message))
                {
                    return message.GetString()
                        ??
                        "Unable to complete the request.";
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