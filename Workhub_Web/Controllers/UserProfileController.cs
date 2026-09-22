using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Workhub_Web.Models;

namespace Workhub_Web.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UserProfileController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        // =========================================
        // EDIT PROFILE
        // =========================================

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var token =
                HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }

            try
            {
                var client =
                    CreateAuthenticatedClient(token);

                var response =
                    await client.GetAsync(
                        "api/UserProfile/me");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ProfileError"] =
                        await GetApiMessageAsync(response);

                    return RedirectToAction(
                        "Profile",
                        "Account");
                }

                var apiProfile =
                    await response.Content
                        .ReadFromJsonAsync<UserProfileApiResponse>();

                if (apiProfile == null)
                {
                    TempData["ProfileError"] =
                        "Unable to load your profile.";

                    return RedirectToAction(
                        "Profile",
                        "Account");
                }

                var version =
                    DateTime.UtcNow.Ticks;

                var model =
                    new EditUserProfileViewModel
                    {
                        UserId =
                            apiProfile.UserId,

                        FirstName =
                            apiProfile.FirstName
                            ?? string.Empty,

                        LastName =
                            apiProfile.LastName
                            ?? string.Empty,

                        Email =
                            apiProfile.Email
                            ?? string.Empty,

                        About =
                            apiProfile.About,

                        DateOfBirth =
                            apiProfile.DateOfBirth,

                        NationalIdNumber =
                            apiProfile.NationalIdNumber,

                        Country =
                            apiProfile.Country,

                        Location =
                            apiProfile.Location,

                        Phone =
                            apiProfile.Phone,

                        Website =
                            apiProfile.Website,

                        LinkedInUrl =
                            apiProfile.LinkedInUrl,

                        FacebookUrl =
                            apiProfile.FacebookUrl,

                        GitHubUrl =
                            apiProfile.GitHubUrl,

                        InstagramUrl =
                            apiProfile.InstagramUrl,

                        ProfileImageUrl =
                            string.IsNullOrWhiteSpace(
                                apiProfile.ProfileImageUrl)
                                ? null
                                : Url.Action(
                                    "ProfileImage",
                                    "UserProfile",
                                    new
                                    {
                                        v = version
                                    }),

                        CoverImageUrl =
                            string.IsNullOrWhiteSpace(
                                apiProfile.CoverImageUrl)
                                ? null
                                : Url.Action(
                                    "CoverImage",
                                    "UserProfile",
                                    new
                                    {
                                        v = version
                                    })
                    };

                return View(model);
            }
            catch (HttpRequestException)
            {
                TempData["ProfileError"] =
                    "Unable to connect to WorkHub API.";

                return RedirectToAction(
                    "Profile",
                    "Account");
            }
        }


        // =========================================
        // UPDATE PROFILE
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            EditUserProfileViewModel model)
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

            if (string.IsNullOrWhiteSpace(
                    model.FirstName) ||
                string.IsNullOrWhiteSpace(
                    model.LastName))
            {
                TempData["ProfileError"] =
                    "First name and last name are required.";

                return RedirectToAction(
                    "Edit");
            }

            try
            {
                var client =
                    CreateAuthenticatedClient(token);

                var request =
                    new
                    {
                        firstName =
                            model.FirstName.Trim(),

                        lastName =
                            model.LastName.Trim(),

                        about =
                            Clean(model.About),

                        dateOfBirth =
                            model.DateOfBirth,

                        nationalIdNumber =
                            Clean(model.NationalIdNumber),

                        country =
                            Clean(model.Country),

                        location =
                            Clean(model.Location),

                        phone =
                            Clean(model.Phone),

                        website =
                            Clean(model.Website),

                        linkedInUrl =
                            Clean(model.LinkedInUrl),

                        facebookUrl =
                            Clean(model.FacebookUrl),

                        gitHubUrl =
                            Clean(model.GitHubUrl),

                        instagramUrl =
                            Clean(model.InstagramUrl)
                    };

                var response =
                    await client.PutAsJsonAsync(
                        "api/UserProfile/me",
                        request);

                if (response.IsSuccessStatusCode)
                {
                    var fullName =
                        $"{model.FirstName.Trim()} {model.LastName.Trim()}"
                        .Trim();

                    HttpContext.Session.SetString(
                        "FullName",
                        fullName);

                    TempData["ProfileSuccess"] =
                        "Profile updated successfully.";
                }
                else
                {
                    TempData["ProfileError"] =
                        await GetApiMessageAsync(
                            response);
                }
            }
            catch (HttpRequestException)
            {
                TempData["ProfileError"] =
                    "Unable to connect to WorkHub API.";
            }

            return RedirectToAction(
                "Edit");
        }


        // =========================================
        // DASHBOARD SUMMARY
        // =========================================

        [HttpGet]
        public async Task<IActionResult>
            DashboardSummary()
        {
            var token =
                HttpContext.Session.GetString(
                    "JwtToken");

            if (string.IsNullOrWhiteSpace(token))
            {
                return Unauthorized();
            }

            try
            {
                var client =
                    CreateAuthenticatedClient(token);

                var response =
                    await client.GetAsync(
                        "api/UserProfile/me");

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(
                        (int)response.StatusCode);
                }

                var profile =
                    await response.Content
                        .ReadFromJsonAsync<
                            UserProfileApiResponse>();

                if (profile == null)
                {
                    return NotFound();
                }

                return Json(new
                {
                    profile.FirstName,
                    profile.LastName,
                    profile.About,
                    profile.DateOfBirth,
                    profile.Country,
                    profile.Location,
                    profile.Phone,

                    profile.Website,
                    profile.LinkedInUrl,
                    profile.FacebookUrl,
                    profile.GitHubUrl,
                    profile.InstagramUrl,

                    hasProfileImage =
                        !string.IsNullOrWhiteSpace(
                            profile.ProfileImageUrl),

                    hasCoverImage =
                        !string.IsNullOrWhiteSpace(
                            profile.CoverImageUrl)
                });
            }
            catch
            {
                return StatusCode(500);
            }
        }


        // =========================================
        // PROFILE IMAGE
        // =========================================

        [HttpGet]
        public async Task<IActionResult> ProfileImage()
        {
            var userIdValue =
                HttpContext.Session.GetString(
                    "UserId");

            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return NotFound();
            }

            try
            {
                var client =
                    _httpClientFactory.CreateClient(
                        "WorkHubApi");

                var response =
                    await client.GetAsync(
                        $"api/UserProfile/profile-image/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var bytes =
                    await response.Content
                        .ReadAsByteArrayAsync();

                var contentType =
                    response.Content.Headers
                        .ContentType?
                        .MediaType
                    ?? "image/jpeg";

                DisableImageCaching();

                return File(
                    bytes,
                    contentType);
            }
            catch
            {
                return NotFound();
            }
        }


        // =========================================
        // COVER IMAGE
        // =========================================

        [HttpGet]
        public async Task<IActionResult> CoverImage()
        {
            var userIdValue =
                HttpContext.Session.GetString(
                    "UserId");

            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return NotFound();
            }

            try
            {
                var client =
                    _httpClientFactory.CreateClient(
                        "WorkHubApi");

                var response =
                    await client.GetAsync(
                        $"api/UserProfile/cover-image/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    return NotFound();
                }

                var bytes =
                    await response.Content
                        .ReadAsByteArrayAsync();

                var contentType =
                    response.Content.Headers
                        .ContentType?
                        .MediaType
                    ?? "image/jpeg";

                DisableImageCaching();

                return File(
                    bytes,
                    contentType);
            }
            catch
            {
                return NotFound();
            }
        }


        // =========================================
        // UPLOAD PROFILE IMAGE
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            UploadProfileImage(
                IFormFile profileImage)
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

            if (profileImage == null ||
                profileImage.Length == 0)
            {
                TempData["ProfileError"] =
                    "Please select a profile image.";

                return RedirectToAction(
                    "Edit");
            }

            try
            {
                var client =
                    CreateAuthenticatedClient(token);

                using var content =
                    new MultipartFormDataContent();

                await using var stream =
                    profileImage.OpenReadStream();

                using var fileContent =
                    new StreamContent(stream);

                if (!string.IsNullOrWhiteSpace(
                        profileImage.ContentType))
                {
                    fileContent.Headers.ContentType =
                        new MediaTypeHeaderValue(
                            profileImage.ContentType);
                }

                content.Add(
                    fileContent,
                    "file",
                    profileImage.FileName);

                var response =
                    await client.PostAsync(
                        "api/UserProfile/profile-image",
                        content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["ProfileSuccess"] =
                        "Profile picture updated successfully.";
                }
                else
                {
                    TempData["ProfileError"] =
                        await GetApiMessageAsync(
                            response);
                }
            }
            catch (HttpRequestException)
            {
                TempData["ProfileError"] =
                    "Unable to upload profile picture.";
            }

            return RedirectToAction(
                "Edit");
        }


        // =========================================
        // UPLOAD COVER IMAGE
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            UploadCoverImage(
                IFormFile coverImage)
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

            if (coverImage == null ||
                coverImage.Length == 0)
            {
                TempData["ProfileError"] =
                    "Please select a cover image.";

                return RedirectToAction(
                    "Edit");
            }

            try
            {
                var client =
                    CreateAuthenticatedClient(token);

                using var content =
                    new MultipartFormDataContent();

                await using var stream =
                    coverImage.OpenReadStream();

                using var fileContent =
                    new StreamContent(stream);

                if (!string.IsNullOrWhiteSpace(
                        coverImage.ContentType))
                {
                    fileContent.Headers.ContentType =
                        new MediaTypeHeaderValue(
                            coverImage.ContentType);
                }

                content.Add(
                    fileContent,
                    "file",
                    coverImage.FileName);

                var response =
                    await client.PostAsync(
                        "api/UserProfile/cover-image",
                        content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["ProfileSuccess"] =
                        "Cover image updated successfully.";
                }
                else
                {
                    TempData["ProfileError"] =
                        await GetApiMessageAsync(
                            response);
                }
            }
            catch (HttpRequestException)
            {
                TempData["ProfileError"] =
                    "Unable to upload cover image.";
            }

            return RedirectToAction(
                "Edit");
        }


        // =========================================
        // CACHE
        // =========================================

        private void DisableImageCaching()
        {
            Response.Headers.CacheControl =
                "no-store, no-cache, must-revalidate, max-age=0";

            Response.Headers.Pragma =
                "no-cache";

            Response.Headers.Expires =
                "0";
        }


        // =========================================
        // API CLIENT
        // =========================================

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


        // =========================================
        // CLEAN
        // =========================================

        private static string? Clean(
            string? value)
        {
            return string.IsNullOrWhiteSpace(
                    value)
                ? null
                : value.Trim();
        }


        // =========================================
        // API ERROR
        // =========================================

        private static async Task<string>
            GetApiMessageAsync(
                HttpResponseMessage response)
        {
            try
            {
                var text =
                    await response.Content
                        .ReadAsStringAsync();

                using var document =
                    JsonDocument.Parse(text);

                if (document.RootElement
                    .TryGetProperty(
                        "message",
                        out var message))
                {
                    return message.GetString()
                           ?? "Request failed.";
                }

                if (document.RootElement
                    .TryGetProperty(
                        "title",
                        out var title))
                {
                    return title.GetString()
                           ?? "Request failed.";
                }
            }
            catch
            {
            }

            return
                $"Request failed. Status {(int)response.StatusCode}.";
        }


        // =========================================
        // API MODEL
        // =========================================

        private class UserProfileApiResponse
        {
            public int UserId { get; set; }

            public string? FullName { get; set; }

            public string? FirstName { get; set; }

            public string? LastName { get; set; }

            public string? Email { get; set; }

            public string? About { get; set; }

            public DateTime? DateOfBirth { get; set; }

            public string? NationalIdNumber { get; set; }

            public string? Country { get; set; }

            public string? Location { get; set; }

            public string? Phone { get; set; }

            public string? Website { get; set; }

            public string? LinkedInUrl { get; set; }

            public string? FacebookUrl { get; set; }

            public string? GitHubUrl { get; set; }

            public string? InstagramUrl { get; set; }

            public string? ProfileImageUrl { get; set; }

            public string? CoverImageUrl { get; set; }
        }
    }
}