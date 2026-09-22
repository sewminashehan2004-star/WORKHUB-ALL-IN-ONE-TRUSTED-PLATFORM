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
    public class ProfilesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================
        // ACTIVATE PROFILE TYPES
        // ============================================

        [HttpPost("activate")]
        public async Task<IActionResult> ActivateProfiles(
            ProfileSelectionDto request)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user."
                });
            }

            if (!request.JobSeeker &&
                !request.ServiceProvider &&
                !request.MarketplaceSeller)
            {
                return BadRequest(new
                {
                    message =
                        "Please select at least one profile type."
                });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            // ----------------------------------------
            // JOB SEEKER PROFILE
            // ----------------------------------------

            if (request.JobSeeker)
            {
                var exists = await _context.JobSeekerProfiles
                    .AnyAsync(x => x.UserId == userId);

                if (!exists)
                {
                    var jobSeekerProfile =
                        new JobSeekerProfile
                        {
                            UserId = userId,
                            CurrentLocation = user.Location,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                    _context.JobSeekerProfiles.Add(
                        jobSeekerProfile
                    );
                }
            }

            // ----------------------------------------
            // SERVICE PROVIDER PROFILE
            // ----------------------------------------

            if (request.ServiceProvider)
            {
                var exists =
                    await _context.ServiceProviderProfiles
                        .AnyAsync(x => x.UserId == userId);

                if (!exists)
                {
                    var serviceProviderProfile =
                        new ServiceProviderProfile
                        {
                            UserId = userId,
                            DisplayName = user.FullName,
                            Location = user.Location,
                            VerificationStatus = "Pending",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                    _context.ServiceProviderProfiles.Add(
                        serviceProviderProfile
                    );
                }
            }

            // ----------------------------------------
            // MARKETPLACE SELLER PROFILE
            // ----------------------------------------

            if (request.MarketplaceSeller)
            {
                var exists =
                    await _context.MarketplaceSellerProfiles
                        .AnyAsync(x => x.UserId == userId);

                if (!exists)
                {
                    var sellerProfile =
                        new MarketplaceSellerProfile
                        {
                            UserId = userId,
                            Location = user.Location,
                            VerificationStatus = "Pending",
                            IsBusinessSeller = false,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                    _context.MarketplaceSellerProfiles.Add(
                        sellerProfile
                    );
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "WorkHub profiles activated successfully."
            });
        }

        // ============================================
        // GET MY ACTIVE PROFILES
        // ============================================

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfiles()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var jobSeeker =
                await _context.JobSeekerProfiles
                    .AnyAsync(x =>
                        x.UserId == userId &&
                        x.IsActive);

            var serviceProvider =
                await _context.ServiceProviderProfiles
                    .AnyAsync(x =>
                        x.UserId == userId &&
                        x.IsActive);

            var marketplaceSeller =
                await _context.MarketplaceSellerProfiles
                    .AnyAsync(x =>
                        x.UserId == userId &&
                        x.IsActive);

            return Ok(new
            {
                jobSeeker,
                serviceProvider,
                marketplaceSeller
            });
        }
    }
}