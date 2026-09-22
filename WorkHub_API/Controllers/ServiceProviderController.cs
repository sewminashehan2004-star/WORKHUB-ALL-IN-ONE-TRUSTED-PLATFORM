using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkHub.API.Data;
using WorkHub.API.DTOs;
using WorkHub.API.Models;

namespace WorkHub_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceProviderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceProviderController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // GET MY SERVICE PROVIDER PROFILE
        // =========================================

        [Authorize(Roles = "RegisteredUser")]
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
                await _context.ServiceProviderProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);

            if (profile == null)
            {
                return NotFound(new
                {
                    message =
                        "Service Provider profile not found. Activate it first."
                });
            }

            return Ok(new
            {
                profile.ServiceProviderProfileId,
                profile.DisplayName,
                profile.Bio,
                profile.Location,
                profile.YearsOfExperience,
                profile.StartingPrice,
                profile.VerificationStatus,
                profile.IsActive,
                profile.CreatedAt
            });
        }

        // =========================================
        // UPDATE MY PROFILE
        // =========================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(
            ServiceProviderProfileDto request)
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
                await _context.ServiceProviderProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);

            if (profile == null)
            {
                return NotFound(new
                {
                    message =
                        "Service Provider profile not found."
                });
            }

            profile.DisplayName =
                request.DisplayName?.Trim();

            profile.Bio =
                request.Bio?.Trim();

            profile.Location =
                request.Location?.Trim();

            profile.YearsOfExperience =
                request.YearsOfExperience;

            profile.StartingPrice =
                request.StartingPrice;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Service Provider profile updated successfully."
            });
        }

        // =========================================
        // CREATE SERVICE OFFERING
        // =========================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpPost("offerings")]
        public async Task<IActionResult> CreateOffering(
            ServiceOfferingDto request)
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
                await _context.ServiceProviderProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId &&
                        x.IsActive);

            if (profile == null)
            {
                return BadRequest(new
                {
                    message =
                        "Activate your Service Provider profile first."
                });
            }

            var category =
                await _context.ServiceCategories
                    .FirstOrDefaultAsync(x =>
                        x.ServiceCategoryId ==
                        request.ServiceCategoryId &&
                        x.IsActive);

            if (category == null)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid service category."
                });
            }

            var offering =
                new ServiceOffering
                {
                    ServiceProviderProfileId =
                        profile.ServiceProviderProfileId,

                    ServiceCategoryId =
                        request.ServiceCategoryId,

                    Title =
                        request.Title.Trim(),

                    Description =
                        request.Description?.Trim(),

                    StartingPrice =
                        request.StartingPrice,

                    Location =
                        request.Location?.Trim(),

                    IsAvailable =
                        request.IsAvailable,

                    CreatedAt =
                        DateTime.UtcNow
                };

            _context.ServiceOfferings.Add(
                offering);

            await _context.SaveChangesAsync();

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    message =
                        "Service offering created successfully.",

                    offering.ServiceOfferingId,

                    offering.Title,

                    category.CategoryName
                });
        }

        // =========================================
        // GET MY OFFERINGS
        // =========================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpGet("offerings")]
        public async Task<IActionResult> GetMyOfferings()
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
                await _context.ServiceProviderProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);

            if (profile == null)
            {
                return NotFound(new
                {
                    message =
                        "Service Provider profile not found."
                });
            }

            var offerings =
                await _context.ServiceOfferings
                    .Where(x =>
                        x.ServiceProviderProfileId ==
                        profile.ServiceProviderProfileId)
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Select(x => new
                    {
                        x.ServiceOfferingId,
                        x.Title,
                        x.Description,
                        x.StartingPrice,
                        x.Location,
                        x.IsAvailable,
                        x.CreatedAt,

                        Category = new
                        {
                            x.ServiceCategory
                                .ServiceCategoryId,

                            x.ServiceCategory
                                .CategoryName
                        }
                    })
                    .ToListAsync();

            return Ok(offerings);
        }

        // =========================================
        // UPDATE OFFERING
        // =========================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpPut("offerings/{id}")]
        public async Task<IActionResult> UpdateOffering(
            int id,
            ServiceOfferingDto request)
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
                await _context.ServiceProviderProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);

            if (profile == null)
            {
                return NotFound(new
                {
                    message =
                        "Service Provider profile not found."
                });
            }

            var offering =
                await _context.ServiceOfferings
                    .FirstOrDefaultAsync(x =>
                        x.ServiceOfferingId == id &&
                        x.ServiceProviderProfileId ==
                        profile.ServiceProviderProfileId);

            if (offering == null)
            {
                return NotFound(new
                {
                    message =
                        "Service offering not found."
                });
            }

            var category =
                await _context.ServiceCategories
                    .FirstOrDefaultAsync(x =>
                        x.ServiceCategoryId ==
                        request.ServiceCategoryId &&
                        x.IsActive);

            if (category == null)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid service category."
                });
            }

            offering.ServiceCategoryId =
                request.ServiceCategoryId;

            offering.Title =
                request.Title.Trim();

            offering.Description =
                request.Description?.Trim();

            offering.StartingPrice =
                request.StartingPrice;

            offering.Location =
                request.Location?.Trim();

            offering.IsAvailable =
                request.IsAvailable;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Service offering updated successfully."
            });
        }

        // =========================================
        // DELETE OFFERING
        // =========================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpDelete("offerings/{id}")]
        public async Task<IActionResult> DeleteOffering(
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

            var profile =
                await _context.ServiceProviderProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);

            if (profile == null)
            {
                return NotFound(new
                {
                    message =
                        "Service Provider profile not found."
                });
            }

            var offering =
                await _context.ServiceOfferings
                    .FirstOrDefaultAsync(x =>
                        x.ServiceOfferingId == id &&
                        x.ServiceProviderProfileId ==
                        profile.ServiceProviderProfileId);

            if (offering == null)
            {
                return NotFound(new
                {
                    message =
                        "Service offering not found."
                });
            }

            _context.ServiceOfferings.Remove(
                offering);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Service offering deleted successfully."
            });
        }
    }
}