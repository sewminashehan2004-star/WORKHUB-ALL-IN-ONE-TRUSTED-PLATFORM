using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkHub.API.Data;
using WorkHub.API.DTOs;
using WorkHub.API.Models;

namespace WorkHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketplaceImagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        private const long MaxFileSize = 5 * 1024 * 1024;
        private const int MaxImagesPerListing = 8;

        private static readonly string[] AllowedExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        private static readonly string[] AllowedContentTypes =
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };


        public MarketplaceImagesController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }


        // ============================================================
        // SELLER - UPLOAD IMAGE
        // ============================================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpPost("listing/{listingId:int}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage(
            int listingId,
            [FromForm] UploadMarketplaceImageDto request)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);


            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }


            var listing =
                await _context.MarketplaceListings
                    .Include(x => x.MarketplaceSellerProfile)
                    .FirstOrDefaultAsync(x =>
                        x.MarketplaceListingId == listingId &&
                        x.MarketplaceSellerProfile.UserId == userId);


            if (listing == null)
            {
                return NotFound(new
                {
                    message =
                        "Marketplace listing not found or does not belong to you."
                });
            }


            if (request.File == null ||
                request.File.Length == 0)
            {
                return BadRequest(new
                {
                    message =
                        "Please select an image."
                });
            }


            if (request.File.Length > MaxFileSize)
            {
                return BadRequest(new
                {
                    message =
                        "Image size cannot exceed 5 MB."
                });
            }


            var extension =
                Path.GetExtension(request.File.FileName)
                    .ToLowerInvariant();


            if (!AllowedExtensions.Contains(extension))
            {
                return BadRequest(new
                {
                    message =
                        "Only JPG, JPEG, PNG and WEBP images are allowed."
                });
            }


            var contentType =
                request.File.ContentType
                    ?.ToLowerInvariant()
                ?? string.Empty;


            if (!AllowedContentTypes.Contains(contentType))
            {
                return BadRequest(new
                {
                    message =
                        "Invalid image file type."
                });
            }


            var currentImages =
                await _context.MarketplaceImages
                    .Where(x =>
                        x.MarketplaceListingId == listingId)
                    .ToListAsync();


            if (currentImages.Count >= MaxImagesPerListing)
            {
                return BadRequest(new
                {
                    message =
                        $"A marketplace listing can have a maximum of {MaxImagesPerListing} images."
                });
            }


            var shouldBePrimary =
                request.IsPrimary ||
                currentImages.Count == 0;


            if (shouldBePrimary)
            {
                foreach (var image in currentImages)
                {
                    image.IsPrimary = false;
                }
            }


            var storedFileName =
                $"{Guid.NewGuid():N}{extension}";


            var uploadFolder =
                Path.Combine(
                    _environment.ContentRootPath,
                    "Uploads",
                    "Marketplace");


            Directory.CreateDirectory(uploadFolder);


            var physicalFilePath =
                Path.Combine(
                    uploadFolder,
                    storedFileName);


            await using (
                var stream =
                    new FileStream(
                        physicalFilePath,
                        FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }


            var marketplaceImage =
                new MarketplaceImage
                {
                    MarketplaceListingId = listingId,

                    ImagePath =
                        Path.Combine(
                            "Uploads",
                            "Marketplace",
                            storedFileName),

                    IsPrimary =
                        shouldBePrimary
                };


            _context.MarketplaceImages.Add(
                marketplaceImage);


            await _context.SaveChangesAsync();


            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    message =
                        "Marketplace image uploaded successfully.",

                    marketplaceImage.MarketplaceImageId,

                    marketplaceImage.MarketplaceListingId,

                    marketplaceImage.IsPrimary,

                    imageUrl =
                        $"/api/MarketplaceImages/{marketplaceImage.MarketplaceImageId}/file"
                });
        }


        // ============================================================
        // PUBLIC - GET ALL IMAGES FOR LISTING
        // ============================================================

        [AllowAnonymous]
        [HttpGet("listing/{listingId:int}")]
        public async Task<IActionResult> GetListingImages(
            int listingId)
        {
            var listingExists =
                await _context.MarketplaceListings
                    .AnyAsync(x =>
                        x.MarketplaceListingId == listingId);


            if (!listingExists)
            {
                return NotFound(new
                {
                    message =
                        "Marketplace listing not found."
                });
            }


            var images =
                await _context.MarketplaceImages
                    .Where(x =>
                        x.MarketplaceListingId == listingId)
                    .OrderByDescending(x =>
                        x.IsPrimary)
                    .ThenBy(x =>
                        x.MarketplaceImageId)
                    .Select(x => new
                    {
                        x.MarketplaceImageId,

                        x.MarketplaceListingId,

                        x.IsPrimary,

                        imageUrl =
                            $"/api/MarketplaceImages/{x.MarketplaceImageId}/file"
                    })
                    .ToListAsync();


            return Ok(images);
        }


        // ============================================================
        // PUBLIC - VIEW IMAGE
        // ============================================================

        [AllowAnonymous]
        [HttpGet("{imageId:int}/file")]
        public async Task<IActionResult> GetImageFile(
            int imageId)
        {
            var image =
                await _context.MarketplaceImages
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MarketplaceImageId == imageId);


            if (image == null)
            {
                return NotFound(new
                {
                    message =
                        "Marketplace image not found."
                });
            }


            if (string.IsNullOrWhiteSpace(image.ImagePath))
            {
                return NotFound(new
                {
                    message =
                        "Marketplace image path is missing."
                });
            }


            var physicalFilePath =
                Path.Combine(
                    _environment.ContentRootPath,
                    image.ImagePath);


            if (!System.IO.File.Exists(physicalFilePath))
            {
                return NotFound(new
                {
                    message =
                        "Image file could not be found on the server."
                });
            }


            var extension =
                Path.GetExtension(image.ImagePath)
                    .ToLowerInvariant();


            var fileContentType =
                extension switch
                {
                    ".jpg" => "image/jpeg",
                    ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".webp" => "image/webp",
                    _ => "application/octet-stream"
                };


            return PhysicalFile(
                physicalFilePath,
                fileContentType);
        }


        // ============================================================
        // SELLER - SET PRIMARY
        // ============================================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpPut("{imageId:int}/primary")]
        public async Task<IActionResult> SetPrimaryImage(
            int imageId)
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }


            var selectedImage =
                await _context.MarketplaceImages
                    .Include(x =>
                        x.MarketplaceListing)
                    .ThenInclude(x =>
                        x.MarketplaceSellerProfile)
                    .FirstOrDefaultAsync(x =>
                        x.MarketplaceImageId == imageId &&
                        x.MarketplaceListing
                            .MarketplaceSellerProfile
                            .UserId == userId);


            if (selectedImage == null)
            {
                return NotFound(new
                {
                    message =
                        "Marketplace image not found or does not belong to you."
                });
            }


            var images =
                await _context.MarketplaceImages
                    .Where(x =>
                        x.MarketplaceListingId ==
                        selectedImage.MarketplaceListingId)
                    .ToListAsync();


            foreach (var image in images)
            {
                image.IsPrimary =
                    image.MarketplaceImageId == imageId;
            }


            await _context.SaveChangesAsync();


            return Ok(new
            {
                message =
                    "Primary marketplace image updated successfully."
            });
        }


        // ============================================================
        // SELLER - DELETE IMAGE
        // ============================================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpDelete("{imageId:int}")]
        public async Task<IActionResult> DeleteImage(
            int imageId)
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }


            var image =
                await _context.MarketplaceImages
                    .Include(x =>
                        x.MarketplaceListing)
                    .ThenInclude(x =>
                        x.MarketplaceSellerProfile)
                    .FirstOrDefaultAsync(x =>
                        x.MarketplaceImageId == imageId &&
                        x.MarketplaceListing
                            .MarketplaceSellerProfile
                            .UserId == userId);


            if (image == null)
            {
                return NotFound(new
                {
                    message =
                        "Marketplace image not found or does not belong to you."
                });
            }


            var listingId =
                image.MarketplaceListingId;


            var wasPrimary =
                image.IsPrimary;


            var physicalFilePath =
                Path.Combine(
                    _environment.ContentRootPath,
                    image.ImagePath);


            _context.MarketplaceImages.Remove(image);


            await _context.SaveChangesAsync();


            if (System.IO.File.Exists(physicalFilePath))
            {
                try
                {
                    System.IO.File.Delete(
                        physicalFilePath);
                }
                catch (IOException)
                {
                    // Database record is already removed.
                    // A locked file can be cleaned up later.
                }
            }


            if (wasPrimary)
            {
                var nextImage =
                    await _context.MarketplaceImages
                        .Where(x =>
                            x.MarketplaceListingId == listingId)
                        .OrderBy(x =>
                            x.MarketplaceImageId)
                        .FirstOrDefaultAsync();


                if (nextImage != null)
                {
                    nextImage.IsPrimary = true;

                    await _context.SaveChangesAsync();
                }
            }


            return Ok(new
            {
                message =
                    "Marketplace image deleted successfully."
            });
        }
    }
}