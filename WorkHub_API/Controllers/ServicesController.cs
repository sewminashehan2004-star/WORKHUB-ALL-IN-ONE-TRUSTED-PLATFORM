using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkHub.API.Data;

namespace WorkHub_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // PUBLIC - SERVICE CATEGORIES
        // =========================================

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories =
                await _context.ServiceCategories
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.CategoryName)
                    .Select(x => new
                    {
                        x.ServiceCategoryId,
                        x.CategoryName,
                        x.Description
                    })
                    .ToListAsync();

            return Ok(categories);
        }

        // =========================================
        // PUBLIC - ALL SERVICES
        // =========================================

        [HttpGet]
        public async Task<IActionResult> GetServices(
            string? search,
            int? categoryId,
            string? location)
        {
            var query =
                _context.ServiceOfferings
                    .Where(x =>
                        x.IsAvailable &&
                        x.ServiceProviderProfile.IsActive)
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query =
                    query.Where(x =>
                        x.Title.Contains(search) ||
                        (x.Description != null &&
                         x.Description.Contains(search)));
            }

            if (categoryId.HasValue)
            {
                query =
                    query.Where(x =>
                        x.ServiceCategoryId ==
                        categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                query =
                    query.Where(x =>
                        x.Location != null &&
                        x.Location.Contains(location));
            }

            var services =
                await query
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Select(x => new
                    {
                        x.ServiceOfferingId,
                        x.Title,
                        x.Description,
                        x.StartingPrice,
                        x.Location,

                        Category =
                            x.ServiceCategory.CategoryName,

                        Provider = new
                        {
                            x.ServiceProviderProfile
                                .ServiceProviderProfileId,

                            x.ServiceProviderProfile
                                .DisplayName,

                            x.ServiceProviderProfile
                                .YearsOfExperience,

                            x.ServiceProviderProfile
                                .VerificationStatus
                        }
                    })
                    .ToListAsync();

            return Ok(services);
        }

        // =========================================
        // PUBLIC - SERVICE DETAILS
        // =========================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetService(
            int id)
        {
            var service =
                await _context.ServiceOfferings
                    .Where(x =>
                        x.ServiceOfferingId == id &&
                        x.IsAvailable &&
                        x.ServiceProviderProfile.IsActive)
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
                            x.ServiceCategory
                                .ServiceCategoryId,

                            x.ServiceCategory
                                .CategoryName
                        },

                        Provider = new
                        {
                            x.ServiceProviderProfile
                                .ServiceProviderProfileId,

                            x.ServiceProviderProfile
                                .DisplayName,

                            x.ServiceProviderProfile
                                .Bio,

                            x.ServiceProviderProfile
                                .Location,

                            x.ServiceProviderProfile
                                .YearsOfExperience,

                            x.ServiceProviderProfile
                                .VerificationStatus
                        }
                    })
                    .FirstOrDefaultAsync();

            if (service == null)
            {
                return NotFound(new
                {
                    message =
                        "Service not found."
                });
            }

            return Ok(service);
        }
    }
}