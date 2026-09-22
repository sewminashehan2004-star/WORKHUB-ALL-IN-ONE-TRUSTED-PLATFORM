using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkHub.API.Data;

namespace WorkHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketplaceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MarketplaceController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // GET MARKETPLACE CATEGORIES
        // =========================================

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories =
                await _context.MarketplaceCategories
                    .Where(x =>
                        x.IsActive)
                    .OrderBy(x =>
                        x.CategoryName)
                    .Select(x => new
                    {
                        x.MarketplaceCategoryId,
                        x.CategoryName,
                        x.Description
                    })
                    .ToListAsync();

            return Ok(categories);
        }

        // =========================================
        // GET ACTIVE MARKETPLACE LISTINGS
        // =========================================

        [HttpGet]
        public async Task<IActionResult> GetListings(
            string? search,
            int? categoryId,
            string? listingType,
            string? location,
            decimal? minPrice,
            decimal? maxPrice)
        {
            var query =
                _context.MarketplaceListings
                    .Where(x =>
                        x.Status == "Active" &&
                        x.MarketplaceSellerProfile.IsActive)
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(
                    search))
            {
                query =
                    query.Where(x =>
                        x.Title.Contains(search) ||
                        x.Description.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query =
                    query.Where(x =>
                        x.MarketplaceCategoryId ==
                        categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(
                    listingType))
            {
                query =
                    query.Where(x =>
                        x.ListingType ==
                        listingType);
            }

            if (!string.IsNullOrWhiteSpace(
                    location))
            {
                query =
                    query.Where(x =>
                        x.Location != null &&
                        x.Location.Contains(location));
            }

            if (minPrice.HasValue)
            {
                query =
                    query.Where(x =>
                        x.Price >=
                        minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query =
                    query.Where(x =>
                        x.Price <=
                        maxPrice.Value);
            }

            var listings =
                await query
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
                        x.CreatedAt,

                        Category =
                            x.MarketplaceCategory
                                .CategoryName,

                        Seller = new
                        {
                            x.MarketplaceSellerProfile
                                .MarketplaceSellerProfileId,

                            x.MarketplaceSellerProfile
                                .BusinessName,

                            x.MarketplaceSellerProfile
                                .Location,

                            x.MarketplaceSellerProfile
                                .VerificationStatus,

                            x.MarketplaceSellerProfile
                                .IsBusinessSeller
                        },

                        PrimaryImageId =
                            _context.MarketplaceImages
                                .Where(i =>
                                    i.MarketplaceListingId ==
                                    x.MarketplaceListingId)
                                .OrderByDescending(i => i.IsPrimary)
                                .ThenBy(i => i.MarketplaceImageId)
                                .Select(i => (int?)i.MarketplaceImageId)
                                .FirstOrDefault(),

                        PrimaryImage =
                            _context.MarketplaceImages
                                .Where(i =>
                                    i.MarketplaceListingId ==
                                    x.MarketplaceListingId)
                                .OrderByDescending(i => i.IsPrimary)
                                .ThenBy(i => i.MarketplaceImageId)
                                .Select(i =>
                                    i.ImagePath)
                                .FirstOrDefault()
                    })
                    .ToListAsync();

            return Ok(listings);
        }

        // =========================================
        // GET ONE MARKETPLACE LISTING
        // =========================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetListing(
            int id)
        {
            var listing =
                await _context.MarketplaceListings
                    .Where(x =>
                        x.MarketplaceListingId ==
                        id &&
                        x.Status == "Active")
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

                        Seller = new
                        {
                            x.MarketplaceSellerProfile
                                .MarketplaceSellerProfileId,

                            x.MarketplaceSellerProfile
                                .BusinessName,

                            x.MarketplaceSellerProfile
                                .Description,

                            x.MarketplaceSellerProfile
                                .Location,

                            x.MarketplaceSellerProfile
                                .VerificationStatus,

                            x.MarketplaceSellerProfile
                                .IsBusinessSeller
                        },

                        Images =
                            _context.MarketplaceImages
                                .Where(i =>
                                    i.MarketplaceListingId ==
                                    x.MarketplaceListingId)
                                .OrderByDescending(i =>
                                    i.IsPrimary)
                                .Select(i => new
                                {
                                    i.MarketplaceImageId,
                                    i.ImagePath,
                                    i.IsPrimary
                                })
                                .ToList()
                    })
                    .FirstOrDefaultAsync();

            if (listing == null)
            {
                return NotFound(new
                {
                    message =
                        "Marketplace listing not found."
                });
            }

            return Ok(listing);
        }
    }
}