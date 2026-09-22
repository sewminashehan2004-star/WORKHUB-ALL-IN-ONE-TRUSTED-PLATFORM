using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;
using Workhub_Web.Models;

namespace Workhub_Web.Controllers
{
    [Route("Contact")]
    public class ContactController : Controller
    {
        private readonly IHttpClientFactory
            _httpClientFactory;


        public ContactController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory =
                httpClientFactory;
        }


        // =====================================================
        // CONTACT PAGE
        //
        // GET:
        // /Contact
        // =====================================================

        [HttpGet("")]
        [HttpGet("/Home/Contact")]
        public IActionResult Index()
        {
            var model =
                new ContactViewModel
                {
                    SuccessMessage =
                        TempData["ContactSuccess"]
                            ?.ToString(),

                    ErrorMessage =
                        TempData["ContactError"]
                            ?.ToString()
                };


            // =================================================
            // If user is logged in,
            // prefill name/email where possible.
            // =================================================

            var fullName =
                HttpContext.Session.GetString(
                    "FullName");


            var email =
                HttpContext.Session.GetString(
                    "Email");


            if (!string.IsNullOrWhiteSpace(
                    fullName))
            {
                model.FullName =
                    fullName;
            }


            if (!string.IsNullOrWhiteSpace(
                    email))
            {
                model.Email =
                    email;
            }


            return View(
                "~/Views/Home/Contact.cshtml",
                model);
        }


        // =====================================================
        // SUBMIT CONTACT INQUIRY
        //
        // POST:
        // /Contact
        // =====================================================

        [HttpPost("")]
        [HttpPost("/Home/Contact")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(
                    "~/Views/Home/Contact.cshtml",
                    model);
            }


            try
            {
                // =============================================
                // GET LOGGED-IN USER ID
                // =============================================

                var userId =
                    GetLoggedInUserId();


                // =============================================
                // CREATE API REQUEST
                // =============================================

                var request =
                    new
                    {
                        userId = userId,

                        fullName =
                            model.FullName.Trim(),

                        email =
                            model.Email.Trim(),

                        topic =
                            model.Topic.Trim(),

                        message =
                            model.Message.Trim()
                    };


                var client =
                    _httpClientFactory
                        .CreateClient(
                            "WorkHubApi");


                var response =
                    await client.PostAsJsonAsync(
                        "api/ContactInquiries",
                        request);


                if (!response.IsSuccessStatusCode)
                {
                    model.ErrorMessage =
                        await GetApiMessageAsync(
                            response);


                    return View(
                        "~/Views/Home/Contact.cshtml",
                        model);
                }


                TempData["ContactSuccess"] =
                    "Your inquiry has been submitted successfully.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage =
                    "Unable to connect to WorkHub. Please try again.";


                return View(
                    "~/Views/Home/Contact.cshtml",
                    model);
            }
            catch (Exception)
            {
                model.ErrorMessage =
                    "Something went wrong while submitting your inquiry.";


                return View(
                    "~/Views/Home/Contact.cshtml",
                    model);
            }
        }


        // =====================================================
        // FIND LOGGED-IN USER ID
        //
        // Supports both:
        // Session.SetInt32("UserId", ...)
        // and
        // Session.SetString("UserId", ...)
        // =====================================================

        private int? GetLoggedInUserId()
        {
            var intUserId =
                HttpContext.Session.GetInt32(
                    "UserId");


            if (intUserId.HasValue
                &&
                intUserId.Value > 0)
            {
                return intUserId.Value;
            }


            var stringUserId =
                HttpContext.Session.GetString(
                    "UserId");


            if (
                int.TryParse(
                    stringUserId,
                    out var parsedUserId)
                &&
                parsedUserId > 0
            )
            {
                return parsedUserId;
            }


            return null;
        }


        // =====================================================
        // API ERROR MESSAGE
        // =====================================================

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
                            "Unable to submit your inquiry.";
                    }
                }
            }
            catch
            {
            }


            return
                "Unable to submit your inquiry.";
        }
    }
}