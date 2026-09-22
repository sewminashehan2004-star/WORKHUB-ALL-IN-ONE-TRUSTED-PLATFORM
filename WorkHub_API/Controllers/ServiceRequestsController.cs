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
    public class ServiceRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // CREATE SERVICE REQUEST
        // =========================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpPost]
        public async Task<IActionResult> CreateRequest(
            CreateServiceRequestDto request)
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

            if (request.PreferredDate.HasValue &&
                request.PreferredDate.Value <
                DateTime.UtcNow)
            {
                return BadRequest(new
                {
                    message =
                        "Preferred date cannot be in the past."
                });
            }

            var serviceRequest =
                new ServiceRequest
                {
                    UserId = userId,

                    ServiceCategoryId =
                        request.ServiceCategoryId,

                    Title =
                        request.Title.Trim(),

                    Description =
                        request.Description.Trim(),

                    Location =
                        request.Location?.Trim(),

                    PreferredDate =
                        request.PreferredDate,

                    Budget =
                        request.Budget,

                    Status =
                        "Open",

                    CreatedAt =
                        DateTime.UtcNow
                };

            _context.ServiceRequests.Add(
                serviceRequest);

            await _context.SaveChangesAsync();

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    message =
                        "Service request created successfully.",

                    serviceRequest.ServiceRequestId,

                    serviceRequest.Title,

                    category.CategoryName,

                    serviceRequest.Status
                });
        }

        // =========================================
        // GET MY SERVICE REQUESTS
        // =========================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyRequests()
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

            var requests =
                await _context.ServiceRequests
                    .Where(x =>
                        x.UserId == userId)
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Select(x => new
                    {
                        x.ServiceRequestId,
                        x.Title,
                        x.Description,
                        x.Location,
                        x.PreferredDate,
                        x.Budget,
                        x.Status,
                        x.CreatedAt,

                        Category =
                            x.ServiceCategory
                                .CategoryName,

                        OfferCount =
                            _context.ServiceOffers
                                .Count(o =>
                                    o.ServiceRequestId ==
                                    x.ServiceRequestId)
                    })
                    .ToListAsync();

            return Ok(requests);
        }

        // =========================================
        // GET OPEN REQUESTS
        // =========================================

        [AllowAnonymous]
        [HttpGet("open")]
        public async Task<IActionResult> GetOpenRequests(
            int? categoryId,
            string? location)
        {
            var query =
                _context.ServiceRequests
                    .Where(x =>
                        x.Status == "Open")
                    .AsQueryable();

            if (categoryId.HasValue)
            {
                query =
                    query.Where(x =>
                        x.ServiceCategoryId ==
                        categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(
                    location))
            {
                query =
                    query.Where(x =>
                        x.Location != null &&
                        x.Location.Contains(
                            location));
            }

            var requests =
                await query
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Select(x => new
                    {
                        x.ServiceRequestId,
                        x.Title,
                        x.Description,
                        x.Location,
                        x.PreferredDate,
                        x.Budget,
                        x.Status,
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

            return Ok(requests);
        }

        // =========================================
        // GET REQUEST DETAILS
        // =========================================

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRequest(
            int id)
        {
            var request =
                await _context.ServiceRequests
                    .Where(x =>
                        x.ServiceRequestId == id)
                    .Select(x => new
                    {
                        x.ServiceRequestId,
                        x.Title,
                        x.Description,
                        x.Location,
                        x.PreferredDate,
                        x.Budget,
                        x.Status,
                        x.CreatedAt,

                        Category = new
                        {
                            x.ServiceCategory
                                .ServiceCategoryId,

                            x.ServiceCategory
                                .CategoryName
                        }
                    })
                    .FirstOrDefaultAsync();

            if (request == null)
            {
                return NotFound(new
                {
                    message =
                        "Service request not found."
                });
            }

            return Ok(request);
        }

        // =========================================
        // CANCEL MY REQUEST
        // =========================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> CancelRequest(
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

            var request =
                await _context.ServiceRequests
                    .FirstOrDefaultAsync(x =>
                        x.ServiceRequestId == id &&
                        x.UserId == userId);

            if (request == null)
            {
                return NotFound(new
                {
                    message =
                        "Service request not found."
                });
            }

            if (request.Status != "Open")
            {
                return BadRequest(new
                {
                    message =
                        "Only an open service request can be cancelled."
                });
            }

            request.Status =
                "Cancelled";

            var offers =
                await _context.ServiceOffers
                    .Where(x =>
                        x.ServiceRequestId == id &&
                        x.Status == "Pending")
                    .ToListAsync();

            foreach (var offer in offers)
            {
                offer.Status =
                    "Cancelled";
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Service request cancelled successfully."
            });
        }

        // =========================================
        // COMPLETE SERVICE REQUEST
        // =========================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpPatch("{id}/complete")]
        public async Task<IActionResult> CompleteRequest(
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

            var serviceRequest =
                await _context.ServiceRequests
                    .FirstOrDefaultAsync(x =>
                        x.ServiceRequestId == id &&
                        x.UserId == userId);

            if (serviceRequest == null)
            {
                return NotFound(new
                {
                    message =
                        "Service request not found."
                });
            }

            if (serviceRequest.Status != "Assigned")
            {
                return BadRequest(new
                {
                    message =
                        "Only an assigned service request can be completed."
                });
            }

            var acceptedOffer =
                await _context.ServiceOffers
                    .Include(x => x.ServiceProviderProfile)
                    .FirstOrDefaultAsync(x =>
                        x.ServiceRequestId == id &&
                        x.Status == "Accepted");

            if (acceptedOffer == null)
            {
                return BadRequest(new
                {
                    message =
                        "No accepted provider offer was found."
                });
            }

            serviceRequest.Status =
                "Completed";

            _context.UserNotifications.Add(
                new UserNotification
                {
                    UserId = acceptedOffer.ServiceProviderProfile.UserId,
                    Title = "Service completed",
                    Message = $"The customer marked '{serviceRequest.Title}' as completed.",
                    NotificationType = "ServiceCompleted",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Service marked as completed successfully.",

                serviceRequest.ServiceRequestId,

                serviceRequest.Status,

                providerId =
                    acceptedOffer.ServiceProviderProfileId
            });
        }
    }
}