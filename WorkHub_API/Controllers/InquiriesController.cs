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
    public class InquiriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InquiriesController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // PUBLIC - CREATE INQUIRY
        // =========================================

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateInquiry(
            CreateInquiryDto request)
        {
            int? userId = null;

            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdValue =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier);

                if (int.TryParse(
                        userIdValue,
                        out var authenticatedUserId))
                {
                    userId =
                        authenticatedUserId;
                }
            }

            var allowedTypes =
                new[]
                {
                    "General",
                    "Job",
                    "Service",
                    "Marketplace",
                    "Company",
                    "Technical"
                };

            var inquiryType =
                allowedTypes
                    .FirstOrDefault(x =>
                        x.Equals(
                            request.InquiryType.Trim(),
                            StringComparison.OrdinalIgnoreCase));

            if (inquiryType == null)
            {
                return BadRequest(new
                {
                    message =
                        "Inquiry type must be General, Job, Service, Marketplace, Company or Technical."
                });
            }

            var inquiry =
                new Inquiry
                {
                    UserId =
                        userId,

                    Name =
                        request.Name.Trim(),

                    Email =
                        request.Email.Trim().ToLower(),

                    Phone =
                        request.Phone?.Trim(),

                    InquiryType =
                        inquiryType,

                    Subject =
                        request.Subject.Trim(),

                    Message =
                        request.Message.Trim(),

                    Status =
                        "New",

                    CreatedAt =
                        DateTime.UtcNow
                };

            _context.Inquiries.Add(
                inquiry);

            await _context.SaveChangesAsync();

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    message =
                        "Your inquiry has been submitted successfully.",

                    inquiry.InquiryId,

                    inquiry.InquiryType,

                    inquiry.Subject,

                    inquiry.Status,

                    inquiry.CreatedAt
                });
        }

        // =========================================
        // REGISTERED USER - GET MY INQUIRIES
        // =========================================

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyInquiries()
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

            var inquiries =
                await _context.Inquiries
                    .Where(x =>
                        x.UserId == userId)
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Select(x => new
                    {
                        x.InquiryId,
                        x.InquiryType,
                        x.Subject,
                        x.Message,
                        x.Status,
                        x.CreatedAt,
                        x.UpdatedAt
                    })
                    .ToListAsync();

            return Ok(inquiries);
        }

        // =========================================
        // ADMIN - GET ALL INQUIRIES
        // =========================================

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public async Task<IActionResult> GetAllInquiries(
            string? status,
            string? type,
            string? search)
        {
            var query =
                _context.Inquiries
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(
                    status))
            {
                query =
                    query.Where(x =>
                        x.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(
                    type))
            {
                query =
                    query.Where(x =>
                        x.InquiryType == type);
            }

            if (!string.IsNullOrWhiteSpace(
                    search))
            {
                search =
                    search.Trim();

                query =
                    query.Where(x =>
                        x.Name.Contains(search) ||
                        x.Email.Contains(search) ||
                        x.Subject.Contains(search) ||
                        x.Message.Contains(search));
            }

            var inquiries =
                await query
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Select(x => new
                    {
                        x.InquiryId,
                        x.UserId,
                        x.Name,
                        x.Email,
                        x.Phone,
                        x.InquiryType,
                        x.Subject,
                        x.Status,
                        x.CreatedAt,
                        x.UpdatedAt
                    })
                    .ToListAsync();

            return Ok(inquiries);
        }

        // =========================================
        // ADMIN - GET ONE INQUIRY
        // =========================================

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/{inquiryId}")]
        public async Task<IActionResult> GetInquiry(
            int inquiryId)
        {
            var inquiry =
                await _context.Inquiries
                    .Where(x =>
                        x.InquiryId ==
                        inquiryId)
                    .Select(x => new
                    {
                        x.InquiryId,
                        x.UserId,
                        x.Name,
                        x.Email,
                        x.Phone,
                        x.InquiryType,
                        x.Subject,
                        x.Message,
                        x.Status,
                        x.AdminNote,
                        x.CreatedAt,
                        x.UpdatedAt,

                        RegisteredUser =
                            x.User == null
                                ? null
                                : new
                                {
                                    x.User.UserId,
                                    x.User.FullName,
                                    x.User.Email,
                                    x.User.Role
                                }
                    })
                    .FirstOrDefaultAsync();

            if (inquiry == null)
            {
                return NotFound(new
                {
                    message =
                        "Inquiry not found."
                });
            }

            return Ok(inquiry);
        }

        // =========================================
        // ADMIN - UPDATE INQUIRY
        // =========================================

        [Authorize(Roles = "Admin")]
        [HttpPatch("admin/{inquiryId}")]
        public async Task<IActionResult> UpdateInquiry(
            int inquiryId,
            UpdateInquiryDto request)
        {
            var inquiry =
                await _context.Inquiries
                    .FirstOrDefaultAsync(x =>
                        x.InquiryId ==
                        inquiryId);

            if (inquiry == null)
            {
                return NotFound(new
                {
                    message =
                        "Inquiry not found."
                });
            }

            var allowedStatuses =
                new[]
                {
                    "New",
                    "InProgress",
                    "Resolved",
                    "Closed"
                };

            var newStatus =
                allowedStatuses
                    .FirstOrDefault(x =>
                        x.Equals(
                            request.Status.Trim(),
                            StringComparison.OrdinalIgnoreCase));

            if (newStatus == null)
            {
                return BadRequest(new
                {
                    message =
                        "Status must be New, InProgress, Resolved or Closed."
                });
            }

            inquiry.Status =
                newStatus;

            inquiry.AdminNote =
                request.AdminNote?.Trim();

            inquiry.UpdatedAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Inquiry updated successfully.",

                inquiry.InquiryId,

                inquiry.Status,

                inquiry.AdminNote,

                inquiry.UpdatedAt
            });
        }
    }
}