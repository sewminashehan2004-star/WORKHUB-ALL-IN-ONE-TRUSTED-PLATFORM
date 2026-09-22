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
    public class ServiceReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceReviewsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // CUSTOMER - CREATE REVIEW
        // =========================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpPost("request/{requestId}")]
        public async Task<IActionResult> CreateReview(
            int requestId,
            CreateServiceReviewDto request)
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
                        x.ServiceRequestId == requestId &&
                        x.UserId == userId);

            if (serviceRequest == null)
            {
                return NotFound(new
                {
                    message =
                        "Service request not found or does not belong to you."
                });
            }

            if (serviceRequest.Status != "Completed")
            {
                return BadRequest(new
                {
                    message =
                        "You can only review a completed service."
                });
            }

            var acceptedOffer =
                await _context.ServiceOffers
                    .FirstOrDefaultAsync(x =>
                        x.ServiceRequestId == requestId &&
                        x.Status == "Accepted");

            if (acceptedOffer == null)
            {
                return BadRequest(new
                {
                    message =
                        "Accepted service provider could not be found."
                });
            }

            var existingReview =
                await _context.ServiceReviews
                    .FirstOrDefaultAsync(x =>
                        x.ServiceRequestId == requestId);

            if (existingReview != null)
            {
                return BadRequest(new
                {
                    message =
                        "You have already reviewed this service."
                });
            }

            var review =
                new ServiceReview
                {
                    ServiceRequestId =
                        serviceRequest.ServiceRequestId,

                    ServiceProviderProfileId =
                        acceptedOffer.ServiceProviderProfileId,

                    UserId =
                        userId,

                    Rating =
                        request.Rating,

                    Comment =
                        request.Comment?.Trim(),

                    CreatedAt =
                        DateTime.UtcNow
                };

            _context.ServiceReviews.Add(
                review);

            await _context.SaveChangesAsync();

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    message =
                        "Review submitted successfully.",

                    review.ServiceReviewId,

                    review.ServiceRequestId,

                    review.ServiceProviderProfileId,

                    review.Rating,

                    review.Comment,

                    review.CreatedAt
                });
        }

        // =========================================
        // CUSTOMER - GET MY REVIEW FOR REQUEST
        // =========================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpGet("request/{requestId}")]
        public async Task<IActionResult> GetMyReview(
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

            var review =
                await _context.ServiceReviews
                    .Where(x =>
                        x.ServiceRequestId == requestId &&
                        x.UserId == userId)
                    .Select(x => new
                    {
                        x.ServiceReviewId,

                        x.ServiceRequestId,

                        x.ServiceProviderProfileId,

                        x.Rating,

                        x.Comment,

                        x.CreatedAt
                    })
                    .FirstOrDefaultAsync();

            if (review == null)
            {
                return NotFound(new
                {
                    message =
                        "Review not found."
                });
            }

            return Ok(review);
        }

        // =========================================
        // PUBLIC - GET PROVIDER REVIEWS
        // =========================================

        [AllowAnonymous]
        [HttpGet("provider/{providerId}")]
        public async Task<IActionResult> GetProviderReviews(
            int providerId)
        {
            var provider =
                await _context.ServiceProviderProfiles
                    .FirstOrDefaultAsync(x =>
                        x.ServiceProviderProfileId ==
                        providerId &&
                        x.IsActive);

            if (provider == null)
            {
                return NotFound(new
                {
                    message =
                        "Service Provider not found."
                });
            }

            var reviews =
                await _context.ServiceReviews
                    .Where(x =>
                        x.ServiceProviderProfileId ==
                        providerId)
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Select(x => new
                    {
                        x.ServiceReviewId,

                        x.Rating,

                        x.Comment,

                        x.CreatedAt,

                        CustomerName =
                            x.User.FullName,

                        ServiceRequest = new
                        {
                            x.ServiceRequest
                                .ServiceRequestId,

                            x.ServiceRequest
                                .Title
                        }
                    })
                    .ToListAsync();

            decimal averageRating = 0;

            if (reviews.Count > 0)
            {
                averageRating =
                    Math.Round(
                        (decimal)reviews.Average(
                            x => x.Rating),
                        2);
            }

            return Ok(new
            {
                provider = new
                {
                    provider
                        .ServiceProviderProfileId,

                    provider
                        .DisplayName,

                    provider
                        .Location,

                    provider
                        .YearsOfExperience,

                    provider
                        .VerificationStatus
                },

                averageRating,

                totalReviews =
                    reviews.Count,

                reviews
            });
        }

        // =========================================
        // PUBLIC - GET ONE REVIEW
        // =========================================

        [AllowAnonymous]
        [HttpGet("{reviewId}")]
        public async Task<IActionResult> GetReview(
            int reviewId)
        {
            var review =
                await _context.ServiceReviews
                    .Where(x =>
                        x.ServiceReviewId ==
                        reviewId)
                    .Select(x => new
                    {
                        x.ServiceReviewId,

                        x.Rating,

                        x.Comment,

                        x.CreatedAt,

                        Provider = new
                        {
                            x.ServiceProviderProfile
                                .ServiceProviderProfileId,

                            x.ServiceProviderProfile
                                .DisplayName
                        },

                        Customer = new
                        {
                            x.User.FullName
                        },

                        ServiceRequest = new
                        {
                            x.ServiceRequest
                                .ServiceRequestId,

                            x.ServiceRequest
                                .Title
                        }
                    })
                    .FirstOrDefaultAsync();

            if (review == null)
            {
                return NotFound(new
                {
                    message =
                        "Review not found."
                });
            }

            return Ok(review);
        }
    }
}