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
    [Authorize(Roles = "RegisteredUser")]
    public class MarketplaceSellerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MarketplaceSellerController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // GET MY MARKETPLACE SELLER PROFILE
        // =========================================

        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return Unauthorized();
            }

            var profile =
                await _context.MarketplaceSellerProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);

            if (profile == null)
            {
                return NotFound(new
                {
                    message =
                        "Marketplace Seller profile not found. Activate it first."
                });
            }

            return Ok(new
            {
                profile.MarketplaceSellerProfileId,
                profile.BusinessName,
                profile.Description,
                profile.Location,
                profile.VerificationStatus,
                profile.IsBusinessSeller,
                profile.IsActive,
                profile.CreatedAt
            });
        }

        // =========================================
        // UPDATE MY MARKETPLACE SELLER PROFILE
        // =========================================

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(
            MarketplaceSellerProfileDto request)
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return Unauthorized();
            }

            var profile =
                await _context.MarketplaceSellerProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);

            if (profile == null)
            {
                return NotFound(new
                {
                    message =
                        "Marketplace Seller profile not found."
                });
            }

            profile.BusinessName =
                request.BusinessName?.Trim();

            profile.Description =
                request.Description?.Trim();

            profile.Location =
                request.Location?.Trim();

            profile.IsBusinessSeller =
                request.IsBusinessSeller;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Marketplace Seller profile updated successfully."
            });
        }

        // =========================================
        // CREATE MARKETPLACE LISTING
        // =========================================

        [HttpPost("listings")]
        public async Task<IActionResult> CreateListing(
            MarketplaceListingDto request)
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return Unauthorized();
            }

            var seller =
                await _context.MarketplaceSellerProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId &&
                        x.IsActive);

            if (seller == null)
            {
                return BadRequest(new
                {
                    message =
                        "Activate your Marketplace Seller profile first."
                });
            }

            var category =
                await _context.MarketplaceCategories
                    .FirstOrDefaultAsync(x =>
                        x.MarketplaceCategoryId ==
                        request.MarketplaceCategoryId &&
                        x.IsActive);

            if (category == null)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid marketplace category."
                });
            }

            var allowedListingTypes =
                new[]
                {
                    "Sale",
                    "Rent"
                };

            var listingType =
                allowedListingTypes
                    .FirstOrDefault(x =>
                        x.Equals(
                            request.ListingType.Trim(),
                            StringComparison.OrdinalIgnoreCase));

            if (listingType == null)
            {
                return BadRequest(new
                {
                    message =
                        "Listing type must be Sale or Rent."
                });
            }

            var allowedConditions =
                new[]
                {
                    "New",
                    "Used",
                    "Refurbished"
                };

            var condition =
                allowedConditions
                    .FirstOrDefault(x =>
                        x.Equals(
                            request.Condition.Trim(),
                            StringComparison.OrdinalIgnoreCase));

            if (condition == null)
            {
                return BadRequest(new
                {
                    message =
                        "Condition must be New, Used or Refurbished."
                });
            }

            var listing =
                new MarketplaceListing
                {
                    MarketplaceSellerProfileId =
                        seller.MarketplaceSellerProfileId,

                    MarketplaceCategoryId =
                        request.MarketplaceCategoryId,

                    Title =
                        request.Title.Trim(),

                    Description =
                        request.Description.Trim(),

                    Price =
                        request.Price,

                    ListingType =
                        listingType,

                    Condition =
                        condition,

                    Location =
                        request.Location?.Trim(),

                    Status =
                        "Active",

                    CreatedAt =
                        DateTime.UtcNow
                };

            _context.MarketplaceListings.Add(
                listing);

            await _context.SaveChangesAsync();

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    message =
                        "Marketplace listing created successfully.",

                    listing.MarketplaceListingId,

                    listing.Title,

                    listing.ListingType,

                    listing.Price,

                    listing.Status
                });
        }

        // =========================================
        // GET MY LISTINGS
        // =========================================

        [HttpGet("listings")]
        public async Task<IActionResult> GetMyListings()
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return Unauthorized();
            }

            var seller =
                await _context.MarketplaceSellerProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);

            if (seller == null)
            {
                return NotFound(new
                {
                    message =
                        "Marketplace Seller profile not found."
                });
            }

            var listings =
                await _context.MarketplaceListings
                    .Where(x =>
                        x.MarketplaceSellerProfileId ==
                        seller.MarketplaceSellerProfileId)
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Select(x => new
                    {
                        x.MarketplaceListingId,
                        x.Title,
                        x.Description,
                        x.Price,
                        x.ListingType,
                        x.Condition,
                        x.Location,
                        x.Status,
                        x.CreatedAt,

                        Category = new
                        {
                            x.MarketplaceCategory
                                .MarketplaceCategoryId,

                            x.MarketplaceCategory
                                .CategoryName
                        },

                        ImageCount =
                            _context.MarketplaceImages
                                .Count(i =>
                                    i.MarketplaceListingId ==
                                    x.MarketplaceListingId)
                    })
                    .ToListAsync();

            return Ok(listings);
        }

        // =========================================
        // UPDATE MY LISTING
        // =========================================

        [HttpPut("listings/{id}")]
        public async Task<IActionResult> UpdateListing(
            int id,
            MarketplaceListingDto request)
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return Unauthorized();
            }

            var seller =
                await _context.MarketplaceSellerProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);

            if (seller == null)
            {
                return NotFound(new
                {
                    message =
                        "Marketplace Seller profile not found."
                });
            }

            var listing =
                await _context.MarketplaceListings
                    .FirstOrDefaultAsync(x =>
                        x.MarketplaceListingId == id &&
                        x.MarketplaceSellerProfileId ==
                        seller.MarketplaceSellerProfileId);

            if (listing == null)
            {
                return NotFound(new
                {
                    message =
                        "Marketplace listing not found."
                });
            }

            var category =
                await _context.MarketplaceCategories
                    .FirstOrDefaultAsync(x =>
                        x.MarketplaceCategoryId ==
                        request.MarketplaceCategoryId &&
                        x.IsActive);

            if (category == null)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid marketplace category."
                });
            }

            var allowedListingTypes =
                new[]
                {
                    "Sale",
                    "Rent"
                };

            var listingType =
                allowedListingTypes
                    .FirstOrDefault(x =>
                        x.Equals(
                            request.ListingType.Trim(),
                            StringComparison.OrdinalIgnoreCase));

            if (listingType == null)
            {
                return BadRequest(new
                {
                    message =
                        "Listing type must be Sale or Rent."
                });
            }

            var allowedConditions =
                new[]
                {
                    "New",
                    "Used",
                    "Refurbished"
                };

            var condition =
                allowedConditions
                    .FirstOrDefault(x =>
                        x.Equals(
                            request.Condition.Trim(),
                            StringComparison.OrdinalIgnoreCase));

            if (condition == null)
            {
                return BadRequest(new
                {
                    message =
                        "Condition must be New, Used or Refurbished."
                });
            }

            listing.MarketplaceCategoryId =
                request.MarketplaceCategoryId;

            listing.Title =
                request.Title.Trim();

            listing.Description =
                request.Description.Trim();

            listing.Price =
                request.Price;

            listing.ListingType =
                listingType;

            listing.Condition =
                condition;

            listing.Location =
                request.Location?.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Marketplace listing updated successfully."
            });
        }

        // =========================================
        // MARK LISTING AS SOLD
        // =========================================

        [HttpPatch("listings/{id}/sold")]
        public async Task<IActionResult> MarkAsSold(
            int id)
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return Unauthorized();
            }

            var seller =
                await _context.MarketplaceSellerProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);

            if (seller == null)
            {
                return NotFound(new
                {
                    message =
                        "Marketplace Seller profile not found."
                });
            }

            var listing =
                await _context.MarketplaceListings
                    .FirstOrDefaultAsync(x =>
                        x.MarketplaceListingId == id &&
                        x.MarketplaceSellerProfileId ==
                        seller.MarketplaceSellerProfileId);

            if (listing == null)
            {
                return NotFound(new
                {
                    message =
                        "Marketplace listing not found."
                });
            }

            if (listing.Status != "Active")
            {
                return BadRequest(new
                {
                    message =
                        "Only an active listing can be marked as sold."
                });
            }

            listing.Status =
                "Sold";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Marketplace listing marked as sold successfully."
            });
        }

        // =========================================
        // DELETE MY LISTING
        // =========================================

        [HttpDelete("listings/{id}")]
        public async Task<IActionResult> DeleteListing(
            int id)
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return Unauthorized();
            }

            var seller =
                await _context.MarketplaceSellerProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);

            if (seller == null)
            {
                return NotFound(new
                {
                    message =
                        "Marketplace Seller profile not found."
                });
            }

            var listing =
                await _context.MarketplaceListings
                    .FirstOrDefaultAsync(x =>
                        x.MarketplaceListingId == id &&
                        x.MarketplaceSellerProfileId ==
                        seller.MarketplaceSellerProfileId);

            if (listing == null)
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
                        x.MarketplaceListingId == id)
                    .ToListAsync();

            _context.MarketplaceImages.RemoveRange(
                images);

            _context.MarketplaceListings.Remove(
                listing);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Marketplace listing deleted successfully."
            });
        }
    }
}