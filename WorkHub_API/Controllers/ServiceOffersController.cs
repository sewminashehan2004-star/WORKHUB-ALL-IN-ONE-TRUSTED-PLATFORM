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
    public class ServiceOffersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceOffersController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // PROVIDER - SEND OFFER
        // =========================================

        [HttpPost("request/{requestId}")]
        public async Task<IActionResult> CreateOffer(
            int requestId,
            CreateServiceOfferDto request)
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

            var provider =
                await _context.ServiceProviderProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId &&
                        x.IsActive);

            if (provider == null)
            {
                return BadRequest(new
                {
                    message =
                        "Activate your Service Provider profile first."
                });
            }

            var serviceRequest =
                await _context.ServiceRequests
                    .FirstOrDefaultAsync(x =>
                        x.ServiceRequestId ==
                        requestId);

            if (serviceRequest == null)
            {
                return NotFound(new
                {
                    message =
                        "Service request not found."
                });
            }

            if (serviceRequest.Status != "Open")
            {
                return BadRequest(new
                {
                    message =
                        "This service request is no longer open."
                });
            }

            if (serviceRequest.UserId == userId)
            {
                return BadRequest(new
                {
                    message =
                        "You cannot submit an offer to your own service request."
                });
            }

            var alreadyOffered =
                await _context.ServiceOffers
                    .AnyAsync(x =>
                        x.ServiceRequestId ==
                        requestId &&
                        x.ServiceProviderProfileId ==
                        provider.ServiceProviderProfileId);

            if (alreadyOffered)
            {
                return BadRequest(new
                {
                    message =
                        "You have already submitted an offer for this request."
                });
            }

            var offer =
                new ServiceOffer
                {
                    ServiceRequestId =
                        requestId,

                    ServiceProviderProfileId =
                        provider.ServiceProviderProfileId,

                    OfferedPrice =
                        request.OfferedPrice,

                    Message =
                        request.Message?.Trim(),

                    Status =
                        "Pending",

                    CreatedAt =
                        DateTime.UtcNow
                };

            _context.ServiceOffers.Add(
                offer);

            _context.UserNotifications.Add(
                new UserNotification
                {
                    UserId = serviceRequest.UserId,
                    Title = "New service offer",
                    Message = $"A provider sent a ${offer.OfferedPrice:0.00} offer for '{serviceRequest.Title}'.",
                    NotificationType = "ServiceOffer",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });

            await _context.SaveChangesAsync();

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    message =
                        "Service offer submitted successfully.",

                    offer.ServiceOfferId,

                    offer.ServiceRequestId,

                    offer.OfferedPrice,

                    offer.Status
                });
        }

        // =========================================
        // PROVIDER - GET MY OFFERS
        // =========================================

        [HttpGet("me")]
        public async Task<IActionResult> GetMyOffers()
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

            var provider =
                await _context.ServiceProviderProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);

            if (provider == null)
            {
                return NotFound(new
                {
                    message =
                        "Service Provider profile not found."
                });
            }

            var offers =
                await _context.ServiceOffers
                    .Where(x =>
                        x.ServiceProviderProfileId ==
                        provider.ServiceProviderProfileId)
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Select(x => new
                    {
                        x.ServiceOfferId,
                        x.ServiceRequestId,
                        x.OfferedPrice,
                        x.Message,
                        x.Status,
                        x.CreatedAt,

                        Request = new
                        {
                            x.ServiceRequest.Title,
                            x.ServiceRequest.Location,
                            x.ServiceRequest.Budget,
                            x.ServiceRequest.Status
                        }
                    })
                    .ToListAsync();

            return Ok(offers);
        }

        // =========================================
        // REQUESTER - GET OFFERS FOR MY REQUEST
        // =========================================

        [HttpGet("request/{requestId}")]
        public async Task<IActionResult> GetRequestOffers(
            int requestId)
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
                        x.ServiceRequestId ==
                        requestId &&
                        x.UserId ==
                        userId);

            if (serviceRequest == null)
            {
                return NotFound(new
                {
                    message =
                        "Service request not found or does not belong to you."
                });
            }

            var offers =
                await _context.ServiceOffers
                    .Where(x =>
                        x.ServiceRequestId ==
                        requestId)
                    .OrderBy(x =>
                        x.OfferedPrice)
                    .Select(x => new
                    {
                        x.ServiceOfferId,
                        x.OfferedPrice,
                        x.Message,
                        x.Status,
                        x.CreatedAt,

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
                    .ToListAsync();

            return Ok(offers);
        }

        // =========================================
        // REQUESTER - ACCEPT OFFER
        // =========================================

        [HttpPatch("{offerId}/accept")]
        public async Task<IActionResult> AcceptOffer(
            int offerId)
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

            var offer =
                await _context.ServiceOffers
                    .Include(x => x.ServiceProviderProfile)
                    .FirstOrDefaultAsync(x =>
                        x.ServiceOfferId ==
                        offerId);

            if (offer == null)
            {
                return NotFound(new
                {
                    message =
                        "Service offer not found."
                });
            }

            var serviceRequest =
                await _context.ServiceRequests
                    .FirstOrDefaultAsync(x =>
                        x.ServiceRequestId ==
                        offer.ServiceRequestId);

            if (serviceRequest == null ||
                serviceRequest.UserId != userId)
            {
                return Forbid();
            }

            if (serviceRequest.Status != "Open")
            {
                return BadRequest(new
                {
                    message =
                        "This service request is no longer open."
                });
            }

            var allOffers =
                await _context.ServiceOffers
                    .Where(x =>
                        x.ServiceRequestId ==
                        serviceRequest.ServiceRequestId)
                    .ToListAsync();

            foreach (var item in allOffers)
            {
                item.Status =
                    item.ServiceOfferId ==
                    offerId
                        ? "Accepted"
                        : "Rejected";
            }

            serviceRequest.Status =
                "Assigned";

            _context.UserNotifications.Add(
                new UserNotification
                {
                    UserId = offer.ServiceProviderProfile.UserId,
                    Title = "Service offer accepted",
                    Message = $"Your offer for '{serviceRequest.Title}' was accepted. You can now coordinate the work with the customer.",
                    NotificationType = "ServiceOfferAccepted",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Service offer accepted successfully.",

                offer.ServiceOfferId,

                serviceRequest.ServiceRequestId,

                serviceRequest.Status
            });
        }

        // =========================================
        // REQUESTER - REJECT OFFER
        // =========================================

        [HttpPatch("{offerId}/reject")]
        public async Task<IActionResult> RejectOffer(
            int offerId)
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

            var offer =
                await _context.ServiceOffers
                    .Include(x => x.ServiceProviderProfile)
                    .FirstOrDefaultAsync(x =>
                        x.ServiceOfferId ==
                        offerId);

            if (offer == null)
            {
                return NotFound(new
                {
                    message =
                        "Service offer not found."
                });
            }

            var serviceRequest =
                await _context.ServiceRequests
                    .FirstOrDefaultAsync(x =>
                        x.ServiceRequestId ==
                        offer.ServiceRequestId);

            if (serviceRequest == null ||
                serviceRequest.UserId != userId)
            {
                return Forbid();
            }

            if (offer.Status != "Pending")
            {
                return BadRequest(new
                {
                    message =
                        "Only a pending offer can be rejected."
                });
            }

            offer.Status =
                "Rejected";

            _context.UserNotifications.Add(
                new UserNotification
                {
                    UserId = offer.ServiceProviderProfile.UserId,
                    Title = "Service offer update",
                    Message = $"Your offer for '{serviceRequest.Title}' was not selected.",
                    NotificationType = "ServiceOfferRejected",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Service offer rejected successfully."
            });
        }
    }
}