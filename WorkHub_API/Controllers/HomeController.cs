using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkHub.API.Data;

namespace WorkHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // PUBLIC HOME PAGE DATA
        // =====================================================

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetHomePage()
        {
            var now = DateTime.UtcNow;

            var jobCategories =
                await _context.Jobs
                    .Where(x =>
                        x.Status == "Active" &&
                        (!x.ClosingDate.HasValue ||
                         x.ClosingDate.Value >= now) &&
                        x.Category != null &&
                        x.Category != "")
                    .Select(x => x.Category!)
                    .Distinct()
                    .OrderBy(x => x)
                    .Take(8)
                    .ToListAsync();

            var jobs =
                await _context.Jobs
                    .Where(x =>
                        x.Status == "Active" &&
                        (!x.ClosingDate.HasValue ||
                         x.ClosingDate.Value >= now))
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(6)
                    .Select(x => new
                    {
                        x.JobId,
                        x.Title,
                        x.Category,
                        x.Location,
                        x.JobType,
                        x.SalaryMin,
                        x.SalaryMax,
                        x.CreatedAt,

                        Company = new
                        {
                            x.CompanyProfile.CompanyProfileId,
                            x.CompanyProfile.CompanyName,
                            x.CompanyProfile.LogoPath,
                            x.CompanyProfile.VerificationStatus
                        }
                    })
                    .ToListAsync();

            var services =
                await _context.ServiceOfferings
                    .Where(x =>
                        x.IsAvailable &&
                        x.ServiceProviderProfile.IsActive)
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(6)
                    .Select(x => new
                    {
                        x.ServiceOfferingId,
                        x.Title,
                        x.Description,
                        x.StartingPrice,
                        x.Location,
                        x.CreatedAt,

                        Category = new
                        {
                            x.ServiceCategory.ServiceCategoryId,
                            x.ServiceCategory.CategoryName
                        },

                        Provider = new
                        {
                            x.ServiceProviderProfile.ServiceProviderProfileId,
                            x.ServiceProviderProfile.DisplayName,
                            x.ServiceProviderProfile.Location,
                            x.ServiceProviderProfile.VerificationStatus
                        },

                        AverageRating =
                            _context.ServiceReviews
                                .Where(r =>
                                    r.ServiceProviderProfileId ==
                                    x.ServiceProviderProfileId)
                                .Select(r => (double?)r.Rating)
                                .Average() ?? 0,

                        ReviewCount =
                            _context.ServiceReviews
                                .Count(r =>
                                    r.ServiceProviderProfileId ==
                                    x.ServiceProviderProfileId)
                    })
                    .ToListAsync();

            var marketplace =
                await _context.MarketplaceListings
                    .Where(x =>
                        x.Status == "Active" &&
                        x.MarketplaceSellerProfile.IsActive)
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(6)
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

                        Category = new
                        {
                            x.MarketplaceCategory.MarketplaceCategoryId,
                            x.MarketplaceCategory.CategoryName
                        },

                        Seller = new
                        {
                            x.MarketplaceSellerProfile.MarketplaceSellerProfileId,
                            x.MarketplaceSellerProfile.BusinessName,
                            x.MarketplaceSellerProfile.VerificationStatus
                        },

                        PrimaryImageId =
                            _context.MarketplaceImages
                                .Where(i =>
                                    i.MarketplaceListingId ==
                                    x.MarketplaceListingId)
                                .OrderByDescending(i => i.IsPrimary)
                                .ThenBy(i => i.MarketplaceImageId)
                                .Select(i => (int?)i.MarketplaceImageId)
                                .FirstOrDefault()
                    })
                    .ToListAsync();

            return Ok(new
            {
                jobCategories,
                jobs,
                services,
                marketplace
            });
        }

        // =====================================================
        // LOCATION SEARCH
        // =====================================================

        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<IActionResult> SearchByLocation(
            string? location)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                return BadRequest(new
                {
                    message = "Please enter a location."
                });
            }

            var searchLocation =
                location.Trim();

            var now =
                DateTime.UtcNow;

            // =================================================
            // JOBS
            // =================================================

            var jobs =
                await _context.Jobs
                    .Where(x =>
                        x.Status == "Active" &&

                        (!x.ClosingDate.HasValue ||
                         x.ClosingDate.Value >= now) &&

                        x.Location != null &&

                        EF.Functions.Like(
                            x.Location,
                            "%" + searchLocation + "%"))
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new
                    {
                        x.JobId,
                        x.Title,
                        x.Category,
                        x.Location,
                        x.JobType,
                        x.SalaryMin,
                        x.SalaryMax,
                        x.CreatedAt,

                        Company = new
                        {
                            x.CompanyProfile.CompanyProfileId,
                            x.CompanyProfile.CompanyName
                        }
                    })
                    .ToListAsync();

            // =================================================
            // SERVICES
            // =================================================

            var services =
                await _context.ServiceOfferings
                    .Where(x =>
                        x.IsAvailable &&

                        x.ServiceProviderProfile.IsActive &&

                        x.Location != null &&

                        EF.Functions.Like(
                            x.Location,
                            "%" + searchLocation + "%"))
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new
                    {
                        x.ServiceOfferingId,
                        x.Title,
                        x.Description,
                        x.StartingPrice,
                        x.Location,
                        x.CreatedAt,

                        Category =
                            x.ServiceCategory.CategoryName,

                        Provider = new
                        {
                            x.ServiceProviderProfile.ServiceProviderProfileId,
                            x.ServiceProviderProfile.DisplayName,
                            x.ServiceProviderProfile.Location
                        }
                    })
                    .ToListAsync();

            // =================================================
            // MARKETPLACE
            // IMPORTANT:
            // Only LISTING location is searched.
            // Seller profile location is not used.
            // =================================================

            var marketplace =
                await _context.MarketplaceListings
                    .Where(x =>
                        x.Status == "Active" &&

                        x.MarketplaceSellerProfile.IsActive &&

                        x.Location != null &&

                        EF.Functions.Like(
                            x.Location,
                            "%" + searchLocation + "%"))
                    .OrderByDescending(x => x.CreatedAt)
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
                            x.MarketplaceCategory.CategoryName,

                        Seller = new
                        {
                            x.MarketplaceSellerProfile.MarketplaceSellerProfileId,
                            x.MarketplaceSellerProfile.BusinessName
                        },

                        PrimaryImageId =
                            _context.MarketplaceImages
                                .Where(i =>
                                    i.MarketplaceListingId ==
                                    x.MarketplaceListingId)
                                .OrderByDescending(i => i.IsPrimary)
                                .ThenBy(i => i.MarketplaceImageId)
                                .Select(i => (int?)i.MarketplaceImageId)
                                .FirstOrDefault()
                    })
                    .ToListAsync();

            return Ok(new
            {
                searchedLocation = searchLocation,

                totalResults =
                    jobs.Count +
                    services.Count +
                    marketplace.Count,

                jobs,
                services,
                marketplace
            });
        }
    }
}