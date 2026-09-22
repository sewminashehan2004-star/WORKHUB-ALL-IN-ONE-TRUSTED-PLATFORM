using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkHub.API.Data;
using WorkHub.API.DTOs;
using WorkHub.API.Interfaces;

namespace WorkHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public AdminController(
            ApplicationDbContext context,
            IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        // =========================================
        // DASHBOARD
        // =========================================

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var totalUsers =
                await _context.Users.CountAsync();

            var totalCompanies =
                await _context.CompanyProfiles.CountAsync();

            var pendingCompanies =
                await _context.CompanyProfiles
                    .CountAsync(x =>
                        x.VerificationStatus == "Pending");

            var totalJobSeekers =
                await _context.JobSeekerProfiles.CountAsync();

            var totalServiceProviders =
                await _context.ServiceProviderProfiles.CountAsync();

            var pendingServiceProviders =
                await _context.ServiceProviderProfiles
                    .CountAsync(x =>
                        x.VerificationStatus == "Pending");

            var totalMarketplaceSellers =
                await _context.MarketplaceSellerProfiles.CountAsync();

            var pendingMarketplaceSellers =
                await _context.MarketplaceSellerProfiles
                    .CountAsync(x =>
                        x.VerificationStatus == "Pending");

            var totalJobs =
                await _context.Jobs.CountAsync();

            var activeJobs =
                await _context.Jobs
                    .CountAsync(x =>
                        x.Status == "Active");

            var totalApplications =
                await _context.JobApplications.CountAsync();

            var totalServiceRequests =
                await _context.ServiceRequests.CountAsync();

            var totalMarketplaceListings =
                await _context.MarketplaceListings.CountAsync();

            var totalInquiries =
                await _context.Inquiries.CountAsync();

            var newInquiries =
                await _context.Inquiries
                    .CountAsync(x =>
                        x.Status == "New");

            return Ok(new
            {
                users = new
                {
                    totalUsers,
                    totalJobSeekers
                },

                companies = new
                {
                    totalCompanies,
                    pendingCompanies
                },

                serviceProviders = new
                {
                    totalServiceProviders,
                    pendingServiceProviders
                },

                marketplaceSellers = new
                {
                    totalMarketplaceSellers,
                    pendingMarketplaceSellers
                },

                jobs = new
                {
                    totalJobs,
                    activeJobs,
                    totalApplications
                },

                services = new
                {
                    totalServiceRequests
                },

                marketplace = new
                {
                    totalMarketplaceListings
                },

                inquiries = new
                {
                    totalInquiries,
                    newInquiries
                }
            });
        }

        // =========================================
        // USERS - GET ALL
        // =========================================

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers(
            string? search,
            string? role,
            string? status)
        {
            var query =
                _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.FullName.Contains(search) ||
                    x.Email.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(x =>
                    x.Role == role);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x =>
                    x.AccountStatus == status);
            }

            var users =
                await query
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Select(x => new
                    {
                        x.UserId,
                        x.FullName,
                        x.Email,
                        x.Phone,
                        x.Location,
                        x.Role,
                        x.AccountStatus,
                        x.CreatedAt
                    })
                    .ToListAsync();

            return Ok(users);
        }

        // =========================================
        // USERS - GET ONE
        // =========================================

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUser(
            int userId)
        {
            var user =
                await _context.Users
                    .Where(x =>
                        x.UserId == userId)
                    .Select(x => new
                    {
                        x.UserId,
                        x.FullName,
                        x.Email,
                        x.Phone,
                        x.Location,
                        x.Role,
                        x.AccountStatus,
                        x.CreatedAt,

                        JobSeeker =
                            x.JobSeekerProfile == null
                                ? null
                                : new
                                {
                                    x.JobSeekerProfile
                                        .JobSeekerProfileId,

                                    x.JobSeekerProfile
                                        .IsActive
                                },

                        ServiceProvider =
                            x.ServiceProviderProfile == null
                                ? null
                                : new
                                {
                                    x.ServiceProviderProfile
                                        .ServiceProviderProfileId,

                                    x.ServiceProviderProfile
                                        .VerificationStatus,

                                    x.ServiceProviderProfile
                                        .IsActive
                                },

                        MarketplaceSeller =
                            x.MarketplaceSellerProfile == null
                                ? null
                                : new
                                {
                                    x.MarketplaceSellerProfile
                                        .MarketplaceSellerProfileId,

                                    x.MarketplaceSellerProfile
                                        .VerificationStatus,

                                    x.MarketplaceSellerProfile
                                        .IsActive
                                },

                        Company =
                            x.CompanyProfile == null
                                ? null
                                : new
                                {
                                    x.CompanyProfile
                                        .CompanyProfileId,

                                    x.CompanyProfile
                                        .CompanyName,

                                    x.CompanyProfile
                                        .VerificationStatus,

                                    x.CompanyProfile
                                        .IsActive
                                }
                    })
                    .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new
                {
                    message =
                        "User not found."
                });
            }

            return Ok(user);
        }

        // =========================================
        // USERS - UPDATE ACCOUNT STATUS
        // =========================================

        [HttpPatch("users/{userId}/status")]
        public async Task<IActionResult> UpdateUserStatus(
            int userId,
            UpdateUserAccountStatusDto request)
        {
            var user =
                await _context.Users
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);

            if (user == null)
            {
                return NotFound(new
                {
                    message =
                        "User not found."
                });
            }

            if (user.Role == "Admin")
            {
                return BadRequest(new
                {
                    message =
                        "Admin account status cannot be changed from this endpoint."
                });
            }

            var allowedStatuses =
                new[]
                {
                    "Active",
                    "Suspended"
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
                        "Status must be Active or Suspended."
                });
            }

            var oldStatus =
                user.AccountStatus;

            user.AccountStatus =
                newStatus;

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                action: "UpdateUserStatus",
                entityType: "User",
                entityId: user.UserId.ToString(),
                description:
                    $"User '{user.FullName}' account status changed from '{oldStatus}' to '{newStatus}'."
            );

            return Ok(new
            {
                message =
                    "User account status updated successfully.",

                user.UserId,
                user.FullName,
                user.Email,
                user.AccountStatus
            });
        }

        // =========================================
        // COMPANY - GET ALL
        // =========================================

        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies(
            string? verificationStatus)
        {
            var query =
                _context.CompanyProfiles
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(
                    verificationStatus))
            {
                query =
                    query.Where(x =>
                        x.VerificationStatus ==
                        verificationStatus);
            }

            var companies =
                await query
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Select(x => new
                    {
                        x.CompanyProfileId,
                        x.CompanyName,
                        x.Industry,
                        x.Website,
                        x.Address,
                        x.VerificationStatus,
                        x.IsActive,
                        x.CreatedAt,

                        User = new
                        {
                            x.User.UserId,
                            x.User.FullName,
                            x.User.Email
                        }
                    })
                    .ToListAsync();

            return Ok(companies);
        }

        // =========================================
        // COMPANY - VERIFICATION
        // =========================================

        [HttpPatch("companies/{companyId}/verification")]
        public async Task<IActionResult>
            UpdateCompanyVerification(
                int companyId,
                UpdateVerificationStatusDto request)
        {
            var company =
                await _context.CompanyProfiles
                    .FirstOrDefaultAsync(x =>
                        x.CompanyProfileId ==
                        companyId);

            if (company == null)
            {
                return NotFound(new
                {
                    message =
                        "Company not found."
                });
            }

            var newStatus =
                GetVerificationStatus(
                    request.Status);

            if (newStatus == null)
            {
                return BadRequest(new
                {
                    message =
                        "Verification status must be Pending, Approved or Rejected."
                });
            }

            var oldStatus =
                company.VerificationStatus;

            company.VerificationStatus =
                newStatus;

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                action: "UpdateCompanyVerification",
                entityType: "Company",
                entityId:
                    company.CompanyProfileId.ToString(),
                description:
                    $"Company '{company.CompanyName}' verification changed from '{oldStatus}' to '{newStatus}'."
            );

            return Ok(new
            {
                message =
                    "Company verification status updated successfully.",

                company.CompanyProfileId,

                company.CompanyName,

                company.VerificationStatus
            });
        }

        // =========================================
        // SERVICE PROVIDERS - GET ALL
        // =========================================

        [HttpGet("service-providers")]
        public async Task<IActionResult>
            GetServiceProviders(
                string? verificationStatus)
        {
            var query =
                _context.ServiceProviderProfiles
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(
                    verificationStatus))
            {
                query =
                    query.Where(x =>
                        x.VerificationStatus ==
                        verificationStatus);
            }

            var providers =
                await query
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Select(x => new
                    {
                        x.ServiceProviderProfileId,
                        x.DisplayName,
                        x.Bio,
                        x.Location,
                        x.YearsOfExperience,
                        x.StartingPrice,
                        x.VerificationStatus,
                        x.IsActive,
                        x.CreatedAt,

                        User = new
                        {
                            x.User.UserId,
                            x.User.FullName,
                            x.User.Email
                        }
                    })
                    .ToListAsync();

            return Ok(providers);
        }

        // =========================================
        // SERVICE PROVIDER - VERIFICATION
        // =========================================

        [HttpPatch(
            "service-providers/{providerId}/verification")]
        public async Task<IActionResult>
            UpdateServiceProviderVerification(
                int providerId,
                UpdateVerificationStatusDto request)
        {
            var provider =
                await _context.ServiceProviderProfiles
                    .FirstOrDefaultAsync(x =>
                        x.ServiceProviderProfileId ==
                        providerId);

            if (provider == null)
            {
                return NotFound(new
                {
                    message =
                        "Service Provider not found."
                });
            }

            var newStatus =
                GetVerificationStatus(
                    request.Status);

            if (newStatus == null)
            {
                return BadRequest(new
                {
                    message =
                        "Verification status must be Pending, Approved or Rejected."
                });
            }

            var oldStatus =
                provider.VerificationStatus;

            provider.VerificationStatus =
                newStatus;

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                action:
                    "UpdateServiceProviderVerification",
                entityType:
                    "ServiceProvider",
                entityId:
                    provider.ServiceProviderProfileId
                        .ToString(),
                description:
                    $"Service Provider '{provider.DisplayName}' verification changed from '{oldStatus}' to '{newStatus}'."
            );

            return Ok(new
            {
                message =
                    "Service Provider verification updated successfully.",

                provider.ServiceProviderProfileId,

                provider.DisplayName,

                provider.VerificationStatus
            });
        }

        // =========================================
        // MARKETPLACE SELLERS - GET ALL
        // =========================================

        [HttpGet("marketplace-sellers")]
        public async Task<IActionResult>
            GetMarketplaceSellers(
                string? verificationStatus)
        {
            var query =
                _context.MarketplaceSellerProfiles
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(
                    verificationStatus))
            {
                query =
                    query.Where(x =>
                        x.VerificationStatus ==
                        verificationStatus);
            }

            var sellers =
                await query
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Select(x => new
                    {
                        x.MarketplaceSellerProfileId,
                        x.BusinessName,
                        x.Description,
                        x.Location,
                        x.VerificationStatus,
                        x.IsBusinessSeller,
                        x.IsActive,
                        x.CreatedAt,

                        User = new
                        {
                            x.User.UserId,
                            x.User.FullName,
                            x.User.Email
                        }
                    })
                    .ToListAsync();

            return Ok(sellers);
        }

        // =========================================
        // MARKETPLACE SELLER - VERIFICATION
        // =========================================

        [HttpPatch(
            "marketplace-sellers/{sellerId}/verification")]
        public async Task<IActionResult>
            UpdateMarketplaceSellerVerification(
                int sellerId,
                UpdateVerificationStatusDto request)
        {
            var seller =
                await _context.MarketplaceSellerProfiles
                    .FirstOrDefaultAsync(x =>
                        x.MarketplaceSellerProfileId ==
                        sellerId);

            if (seller == null)
            {
                return NotFound(new
                {
                    message =
                        "Marketplace Seller not found."
                });
            }

            var newStatus =
                GetVerificationStatus(
                    request.Status);

            if (newStatus == null)
            {
                return BadRequest(new
                {
                    message =
                        "Verification status must be Pending, Approved or Rejected."
                });
            }

            var oldStatus =
                seller.VerificationStatus;

            seller.VerificationStatus =
                newStatus;

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                action:
                    "UpdateMarketplaceSellerVerification",
                entityType:
                    "MarketplaceSeller",
                entityId:
                    seller.MarketplaceSellerProfileId
                        .ToString(),
                description:
                    $"Marketplace Seller '{seller.BusinessName}' verification changed from '{oldStatus}' to '{newStatus}'."
            );

            return Ok(new
            {
                message =
                    "Marketplace Seller verification updated successfully.",

                seller.MarketplaceSellerProfileId,

                seller.BusinessName,

                seller.VerificationStatus
            });
        }

        // =========================================
        // JOBS - GET ALL
        // =========================================

        [HttpGet("jobs")]
        public async Task<IActionResult> GetJobs(
            string? status)
        {
            var query =
                _context.Jobs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(
                    status))
            {
                query =
                    query.Where(x =>
                        x.Status == status);
            }

            var jobs =
                await query
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Select(x => new
                    {
                        x.JobId,
                        x.Title,
                        x.Category,
                        x.Location,
                        x.JobType,
                        x.Status,
                        x.CreatedAt,
                        x.ClosingDate,

                        Company = new
                        {
                            x.CompanyProfile
                                .CompanyProfileId,

                            x.CompanyProfile
                                .CompanyName
                        },

                        ApplicationCount =
                            _context.JobApplications
                                .Count(a =>
                                    a.JobId ==
                                    x.JobId)
                    })
                    .ToListAsync();

            return Ok(jobs);
        }

        // =========================================
        // JOB - UPDATE STATUS
        // =========================================

        [HttpPatch("jobs/{jobId}/status")]
        public async Task<IActionResult> UpdateJobStatus(
            int jobId,
            UpdateJobStatusDto request)
        {
            var job =
                await _context.Jobs
                    .FirstOrDefaultAsync(x =>
                        x.JobId == jobId);

            if (job == null)
            {
                return NotFound(new
                {
                    message =
                        "Job not found."
                });
            }

            var allowedStatuses =
                new[]
                {
                    "Active",
                    "Closed",
                    "Removed"
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
                        "Job status must be Active, Closed or Removed."
                });
            }

            var oldStatus =
                job.Status;

            job.Status =
                newStatus;

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                action:
                    "UpdateJobStatus",
                entityType:
                    "Job",
                entityId:
                    job.JobId.ToString(),
                description:
                    $"Job '{job.Title}' status changed from '{oldStatus}' to '{newStatus}'."
            );

            return Ok(new
            {
                message =
                    "Job status updated successfully.",

                job.JobId,

                job.Title,

                job.Status
            });
        }

        // =========================================
        // MARKETPLACE LISTINGS - GET ALL
        // =========================================

        [HttpGet("marketplace-listings")]
        public async Task<IActionResult>
            GetMarketplaceListings(
                string? status)
        {
            var query =
                _context.MarketplaceListings
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(
                    status))
            {
                query =
                    query.Where(x =>
                        x.Status == status);
            }

            var listings =
                await query
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Select(x => new
                    {
                        x.MarketplaceListingId,
                        x.Title,
                        x.Price,
                        x.ListingType,
                        x.Condition,
                        x.Location,
                        x.Status,
                        x.CreatedAt,

                        Seller = new
                        {
                            x.MarketplaceSellerProfile
                                .MarketplaceSellerProfileId,

                            x.MarketplaceSellerProfile
                                .BusinessName
                        }
                    })
                    .ToListAsync();

            return Ok(listings);
        }

        // =========================================
        // MARKETPLACE LISTING - UPDATE STATUS
        // =========================================

        [HttpPatch(
            "marketplace-listings/{listingId}/status")]
        public async Task<IActionResult>
            UpdateMarketplaceListingStatus(
                int listingId,
                UpdateMarketplaceListingStatusDto request)
        {
            var listing =
                await _context.MarketplaceListings
                    .FirstOrDefaultAsync(x =>
                        x.MarketplaceListingId ==
                        listingId);

            if (listing == null)
            {
                return NotFound(new
                {
                    message =
                        "Marketplace listing not found."
                });
            }

            var allowedStatuses =
                new[]
                {
                    "Active",
                    "Sold",
                    "Removed"
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
                        "Listing status must be Active, Sold or Removed."
                });
            }

            var oldStatus =
                listing.Status;

            listing.Status =
                newStatus;

            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                action:
                    "UpdateMarketplaceListingStatus",
                entityType:
                    "MarketplaceListing",
                entityId:
                    listing.MarketplaceListingId
                        .ToString(),
                description:
                    $"Marketplace listing '{listing.Title}' status changed from '{oldStatus}' to '{newStatus}'."
            );

            return Ok(new
            {
                message =
                    "Marketplace listing status updated successfully.",

                listing.MarketplaceListingId,

                listing.Title,

                listing.Status
            });
        }

        // =========================================
        // SHARED VERIFICATION VALIDATION
        // =========================================

        private static string? GetVerificationStatus(
            string status)
        {
            var allowedStatuses =
                new[]
                {
                    "Pending",
                    "Approved",
                    "Rejected"
                };

            return allowedStatuses
                .FirstOrDefault(x =>
                    x.Equals(
                        status.Trim(),
                        StringComparison.OrdinalIgnoreCase));
        }
    }
}