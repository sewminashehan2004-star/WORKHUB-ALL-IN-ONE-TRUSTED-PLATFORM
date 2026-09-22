using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Workhub_Web.Models;

namespace Workhub_Web.Controllers
{
    [Route("Seller")]
    public class MarketplaceSellerPortalController : Controller
    {
        private const int MaximumImagesPerListing = 8;

        private const long MaximumImageSize =
            5 * 1024 * 1024;

        private readonly IHttpClientFactory _httpClientFactory;


        public MarketplaceSellerPortalController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        // ============================================================
        // SELLER DASHBOARD
        // ============================================================

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var token =
                RequireRegisteredUserToken();


            if (token == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new
                    {
                        returnUrl = "/Seller"
                    });
            }


            var model =
                new MarketplaceSellerPortalViewModel();


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                var publicClient =
                    _httpClientFactory
                        .CreateClient(
                            "WorkHubApi");


                model.Categories =
                    await publicClient
                        .GetFromJsonAsync<List<MarketplaceCategoryItemViewModel>>(
                            "api/Marketplace/categories")
                    ?? new List<MarketplaceCategoryItemViewModel>();


                var profileResponse =
                    await client.GetAsync(
                        "api/MarketplaceSeller/profile");


                if (profileResponse.IsSuccessStatusCode)
                {
                    model.Profile =
                        await profileResponse.Content
                            .ReadFromJsonAsync<MarketplaceSellerProfileEditViewModel>()
                        ?? new MarketplaceSellerProfileEditViewModel();
                }
                else
                {
                    model.ErrorMessage =
                        await GetApiMessageAsync(
                            profileResponse);
                }


                var listingsResponse =
                    await client.GetAsync(
                        "api/MarketplaceSeller/listings");


                if (listingsResponse.IsSuccessStatusCode)
                {
                    model.Listings =
                        await listingsResponse.Content
                            .ReadFromJsonAsync<List<MyMarketplaceListingViewModel>>()
                        ?? new List<MyMarketplaceListingViewModel>();
                }
            }
            catch (HttpRequestException)
            {
                model.ErrorMessage =
                    "Unable to connect to the WorkHub API.";
            }


            return View(model);
        }


        // ============================================================
        // UPDATE PROFILE
        // ============================================================

        [HttpPost("profile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(
            MarketplaceSellerProfileEditViewModel profile)
        {
            var token =
                RequireRegisteredUserToken();


            if (token == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                var response =
                    await client.PutAsJsonAsync(
                        "api/MarketplaceSeller/profile",
                        new
                        {
                            profile.BusinessName,
                            profile.Description,
                            profile.Location,
                            profile.IsBusinessSeller
                        });


                TempData[
                    response.IsSuccessStatusCode
                        ? "SellerSuccess"
                        : "SellerError"] =
                    response.IsSuccessStatusCode
                        ? "Seller profile updated."
                        : await GetApiMessageAsync(
                            response);
            }
            catch (HttpRequestException)
            {
                TempData["SellerError"] =
                    "Unable to connect to the WorkHub API.";
            }


            return RedirectToAction(
                nameof(Index));
        }


        // ============================================================
        // CREATE LISTING
        // ============================================================

        [HttpPost("listings")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateListing(
            MarketplaceListingCreateViewModel newListing)
        {
            var token =
                RequireRegisteredUserToken();


            if (token == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new
                    {
                        returnUrl = "/Seller"
                    });
            }


            var images =
                newListing.Images?
                    .Where(
                        image =>
                            image != null &&
                            image.Length > 0)
                    .ToList()
                ?? new List<IFormFile>();


            if (images.Count >
                MaximumImagesPerListing)
            {
                TempData["SellerError"] =
                    $"You can upload a maximum of {MaximumImagesPerListing} photos.";


                return RedirectToAction(
                    nameof(Index));
            }


            foreach (var image in images)
            {
                var validationError =
                    ValidateMarketplaceImage(
                        image);


                if (!string.IsNullOrWhiteSpace(
                    validationError))
                {
                    TempData["SellerError"] =
                        validationError;


                    return RedirectToAction(
                        nameof(Index));
                }
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                var response =
                    await client.PostAsJsonAsync(
                        "api/MarketplaceSeller/listings",
                        new
                        {
                            newListing.MarketplaceCategoryId,
                            newListing.Title,
                            newListing.Description,
                            newListing.Price,
                            newListing.ListingType,
                            newListing.Condition,
                            newListing.Location
                        });


                if (!response.IsSuccessStatusCode)
                {
                    TempData["SellerError"] =
                        await GetApiMessageAsync(
                            response);


                    return RedirectToAction(
                        nameof(Index));
                }


                var result =
                    await response.Content
                        .ReadFromJsonAsync<CreateListingResponse>();


                if (result == null ||
                    result.MarketplaceListingId <= 0)
                {
                    TempData["SellerError"] =
                        "The listing was created, but the listing ID could not be read.";


                    return RedirectToAction(
                        nameof(Index));
                }


                // ====================================================
                // UPLOAD PHOTOS
                // ====================================================

                var successfulUploads = 0;
                var failedUploads = 0;


                for (var i = 0;
                     i < images.Count;
                     i++)
                {
                    var image =
                        images[i];


                    var isPrimary =
                        i == 0;


                    var uploadResult =
                        await UploadMarketplaceImageAsync(
                            client,
                            result.MarketplaceListingId,
                            image,
                            isPrimary);


                    if (uploadResult)
                    {
                        successfulUploads++;
                    }
                    else
                    {
                        failedUploads++;
                    }
                }


                if (failedUploads > 0)
                {
                    TempData["SellerError"] =
                        $"Listing created. {successfulUploads} photo(s) uploaded successfully and {failedUploads} photo(s) failed.";
                }
                else
                {
                    TempData["SellerSuccess"] =
                        images.Count > 0
                            ? $"Marketplace listing published with {images.Count} photo(s)."
                            : "Marketplace listing published successfully.";
                }
            }
            catch (HttpRequestException)
            {
                TempData["SellerError"] =
                    "Unable to connect to the WorkHub API.";
            }


            return RedirectToAction(
                nameof(Index));
        }


        // ============================================================
        // MARK SOLD
        // ============================================================

        [HttpPost("listings/{id:int}/sold")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkSold(
            int id)
        {
            var token =
                RequireRegisteredUserToken();


            if (token == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                var response =
                    await client.PatchAsync(
                        $"api/MarketplaceSeller/listings/{id}/sold",
                        null);


                TempData[
                    response.IsSuccessStatusCode
                        ? "SellerSuccess"
                        : "SellerError"] =
                    response.IsSuccessStatusCode
                        ? "Listing marked as sold."
                        : await GetApiMessageAsync(
                            response);
            }
            catch (HttpRequestException)
            {
                TempData["SellerError"] =
                    "Unable to connect to the WorkHub API.";
            }


            return RedirectToAction(
                nameof(Index));
        }


        // ============================================================
        // DELETE LISTING
        // ============================================================

        [HttpPost("listings/{id:int}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteListing(
            int id)
        {
            var token =
                RequireRegisteredUserToken();


            if (token == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            try
            {
                var client =
                    CreateAuthenticatedClient(
                        token);


                var response =
                    await client.DeleteAsync(
                        $"api/MarketplaceSeller/listings/{id}");


                TempData[
                    response.IsSuccessStatusCode
                        ? "SellerSuccess"
                        : "SellerError"] =
                    response.IsSuccessStatusCode
                        ? "Listing removed."
                        : await GetApiMessageAsync(
                            response);
            }
            catch (HttpRequestException)
            {
                TempData["SellerError"] =
                    "Unable to connect to the WorkHub API.";
            }


            return RedirectToAction(
                nameof(Index));
        }


        // ============================================================
        // IMAGE UPLOAD
        // ============================================================

        private static async Task<bool> UploadMarketplaceImageAsync(
            HttpClient client,
            int listingId,
            IFormFile image,
            bool isPrimary)
        {
            try
            {
                using var form =
                    new MultipartFormDataContent();


                await using var stream =
                    image.OpenReadStream();


                using var fileContent =
                    new StreamContent(
                        stream);


                fileContent.Headers.ContentType =
                    new MediaTypeHeaderValue(
                        GetSafeContentType(
                            image));


                var safeFileName =
                    Path.GetFileName(
                        image.FileName);


                form.Add(
                    fileContent,
                    "File",
                    safeFileName);


                form.Add(
                    new StringContent(
                        isPrimary
                            ? "true"
                            : "false"),
                    "IsPrimary");


                var response =
                    await client.PostAsync(
                        $"api/MarketplaceImages/listing/{listingId}",
                        form);


                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }


        // ============================================================
        // IMAGE VALIDATION
        // ============================================================

        private static string? ValidateMarketplaceImage(
            IFormFile image)
        {
            if (image.Length <= 0)
            {
                return "One of the selected photos is empty.";
            }


            if (image.Length >
                MaximumImageSize)
            {
                return $"'{image.FileName}' is larger than 5 MB.";
            }


            var extension =
                Path.GetExtension(
                        image.FileName)
                    .ToLowerInvariant();


            var allowedExtensions =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    ".jpg",
                    ".jpeg",
                    ".png",
                    ".webp"
                };


            if (!allowedExtensions.Contains(
                extension))
            {
                return $"'{image.FileName}' is not supported. Use JPG, JPEG, PNG or WEBP.";
            }


            return null;
        }


        private static string GetSafeContentType(
            IFormFile image)
        {
            var extension =
                Path.GetExtension(
                        image.FileName)
                    .ToLowerInvariant();


            return extension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }


        // ============================================================
        // AUTH
        // ============================================================

        private string? RequireRegisteredUserToken()
        {
            var token =
                HttpContext.Session
                    .GetString(
                        "JwtToken");


            var role =
                HttpContext.Session
                    .GetString(
                        "Role");


            if (string.IsNullOrWhiteSpace(
                token))
            {
                return null;
            }


            if (!string.Equals(
                    role,
                    "RegisteredUser",
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }


            return token;
        }


        // ============================================================
        // CLIENT
        // ============================================================

        private HttpClient CreateAuthenticatedClient(
            string token)
        {
            var client =
                _httpClientFactory
                    .CreateClient(
                        "WorkHubApi");


            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);


            return client;
        }


        // ============================================================
        // API ERROR
        // ============================================================

        private static async Task<string> GetApiMessageAsync(
            HttpResponseMessage response)
        {
            var text =
                await response.Content
                    .ReadAsStringAsync();


            if (string.IsNullOrWhiteSpace(
                text))
            {
                return $"Request failed ({(int)response.StatusCode}).";
            }


            try
            {
                using var json =
                    JsonDocument.Parse(
                        text);


                if (json.RootElement.TryGetProperty(
                    "message",
                    out var message))
                {
                    return message.GetString()
                           ?? text;
                }


                if (json.RootElement.TryGetProperty(
                    "title",
                    out var title))
                {
                    return title.GetString()
                           ?? text;
                }
            }
            catch (JsonException)
            {
            }


            return text.Trim('"');
        }


        private sealed class CreateListingResponse
        {
            public int MarketplaceListingId { get; set; }
        }
    }
}