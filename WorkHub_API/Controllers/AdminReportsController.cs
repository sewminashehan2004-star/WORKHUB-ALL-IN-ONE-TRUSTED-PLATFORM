using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkHub.API.Data;

namespace WorkHub.API.Controllers
{
    [Route("api/Admin/Reports")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminReportsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // OVERALL SYSTEM SUMMARY
        // =========================================

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var totalUsers =
                await _context.Users.CountAsync();

            var activeUsers =
                await _context.Users
                    .CountAsync(x =>
                        x.AccountStatus == "Active");

            var suspendedUsers =
                await _context.Users
                    .CountAsync(x =>
                        x.AccountStatus == "Suspended");

            var registeredUsers =
                await _context.Users
                    .CountAsync(x =>
                        x.Role == "RegisteredUser");

            var companyUsers =
                await _context.Users
                    .CountAsync(x =>
                        x.Role == "Company");

            var adminUsers =
                await _context.Users
                    .CountAsync(x =>
                        x.Role == "Admin");

            var totalCompanies =
                await _context.CompanyProfiles.CountAsync();

            var approvedCompanies =
                await _context.CompanyProfiles
                    .CountAsync(x =>
                        x.VerificationStatus == "Approved");

            var pendingCompanies =
                await _context.CompanyProfiles
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

            var totalServiceProviders =
                await _context.ServiceProviderProfiles.CountAsync();

            var totalServiceRequests =
                await _context.ServiceRequests.CountAsync();

            var completedServiceRequests =
                await _context.ServiceRequests
                    .CountAsync(x =>
                        x.Status == "Completed");

            var totalServiceReviews =
                await _context.ServiceReviews.CountAsync();

            var totalMarketplaceSellers =
                await _context.MarketplaceSellerProfiles.CountAsync();

            var totalMarketplaceListings =
                await _context.MarketplaceListings.CountAsync();

            var activeMarketplaceListings =
                await _context.MarketplaceListings
                    .CountAsync(x =>
                        x.Status == "Active");

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
                    activeUsers,
                    suspendedUsers,
                    registeredUsers,
                    companyUsers,
                    adminUsers
                },

                companies = new
                {
                    totalCompanies,
                    approvedCompanies,
                    pendingCompanies
                },

                jobs = new
                {
                    totalJobs,
                    activeJobs,
                    totalApplications
                },

                services = new
                {
                    totalServiceProviders,
                    totalServiceRequests,
                    completedServiceRequests,
                    totalServiceReviews
                },

                marketplace = new
                {
                    totalMarketplaceSellers,
                    totalMarketplaceListings,
                    activeMarketplaceListings
                },

                inquiries = new
                {
                    totalInquiries,
                    newInquiries
                }
            });
        }

        // =========================================
        // USER REPORT
        // =========================================

        [HttpGet("users")]
        public async Task<IActionResult> GetUserReport()
        {
            var totalUsers =
                await _context.Users.CountAsync();

            var usersByRole =
                await _context.Users
                    .GroupBy(x => x.Role)
                    .Select(g => new
                    {
                        role = g.Key,
                        count = g.Count()
                    })
                    .OrderByDescending(x =>
                        x.count)
                    .ToListAsync();

            var usersByStatus =
                await _context.Users
                    .GroupBy(x => x.AccountStatus)
                    .Select(g => new
                    {
                        status = g.Key,
                        count = g.Count()
                    })
                    .OrderByDescending(x =>
                        x.count)
                    .ToListAsync();

            var recentUsers =
                await _context.Users
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Take(10)
                    .Select(x => new
                    {
                        x.UserId,
                        x.FullName,
                        x.Email,
                        x.Role,
                        x.AccountStatus,
                        x.CreatedAt
                    })
                    .ToListAsync();

            return Ok(new
            {
                totalUsers,
                usersByRole,
                usersByStatus,
                recentUsers
            });
        }

        // =========================================
        // JOB REPORT
        // =========================================

        [HttpGet("jobs")]
        public async Task<IActionResult> GetJobReport()
        {
            var totalJobs =
                await _context.Jobs.CountAsync();

            var activeJobs =
                await _context.Jobs
                    .CountAsync(x =>
                        x.Status == "Active");

            var closedJobs =
                await _context.Jobs
                    .CountAsync(x =>
                        x.Status == "Closed");

            var removedJobs =
                await _context.Jobs
                    .CountAsync(x =>
                        x.Status == "Removed");

            var totalApplications =
                await _context.JobApplications.CountAsync();

            var applicationsByStatus =
                await _context.JobApplications
                    .GroupBy(x => x.Status)
                    .Select(g => new
                    {
                        status = g.Key,
                        count = g.Count()
                    })
                    .OrderByDescending(x =>
                        x.count)
                    .ToListAsync();

            var jobsByCategory =
                await _context.Jobs
                    .Where(x =>
                        x.Category != null &&
                        x.Category != "")
                    .GroupBy(x => x.Category)
                    .Select(g => new
                    {
                        category = g.Key,
                        count = g.Count()
                    })
                    .OrderByDescending(x =>
                        x.count)
                    .Take(10)
                    .ToListAsync();

            var jobsByLocation =
                await _context.Jobs
                    .Where(x =>
                        x.Location != null &&
                        x.Location != "")
                    .GroupBy(x => x.Location)
                    .Select(g => new
                    {
                        location = g.Key,
                        count = g.Count()
                    })
                    .OrderByDescending(x =>
                        x.count)
                    .Take(10)
                    .ToListAsync();

            var topJobs =
                await _context.Jobs
                    .Select(x => new
                    {
                        x.JobId,
                        x.Title,
                        x.Status,

                        CompanyName =
                            x.CompanyProfile.CompanyName,

                        ApplicationCount =
                            _context.JobApplications
                                .Count(a =>
                                    a.JobId == x.JobId)
                    })
                    .OrderByDescending(x =>
                        x.ApplicationCount)
                    .Take(10)
                    .ToListAsync();

            return Ok(new
            {
                totalJobs,
                activeJobs,
                closedJobs,
                removedJobs,
                totalApplications,
                applicationsByStatus,
                jobsByCategory,
                jobsByLocation,
                topJobs
            });
        }

        // =========================================
        // CV MATCHING REPORT
        // =========================================

        [HttpGet("cv-matching")]
        public async Task<IActionResult> GetCVMatchingReport()
        {
            var totalMatches =
                await _context.CVMatchResults.CountAsync();

            double averageOverallScore = 0;

            if (totalMatches > 0)
            {
                averageOverallScore =
                    await _context.CVMatchResults
                        .AverageAsync(x =>
                            (double)x.OverallScore);
            }

            var highMatches =
                await _context.CVMatchResults
                    .CountAsync(x =>
                        x.OverallScore >= 75);

            var mediumMatches =
                await _context.CVMatchResults
                    .CountAsync(x =>
                        x.OverallScore >= 50 &&
                        x.OverallScore < 75);

            var lowMatches =
                await _context.CVMatchResults
                    .CountAsync(x =>
                        x.OverallScore < 50);

            var topMatches =
                await _context.CVMatchResults
                    .OrderByDescending(x =>
                        x.OverallScore)
                    .Take(10)
                    .Select(x => new
                    {
                        x.CVMatchResultId,
                        x.JobApplicationId,
                        x.OverallScore,
                        x.SkillsScore,
                        x.ExperienceScore,
                        x.EducationScore,
                        x.PreferredSkillsScore,
                        x.RelevanceScore,
                        x.LocationScore,

                        Job = new
                        {
                            x.JobApplication.Job.JobId,
                            x.JobApplication.Job.Title
                        },

                        Candidate = new
                        {
                            x.JobApplication
                                .JobSeekerProfile
                                .JobSeekerProfileId,

                            Name =
                                x.JobApplication
                                    .JobSeekerProfile
                                    .User
                                    .FullName
                        }
                    })
                    .ToListAsync();

            return Ok(new
            {
                totalMatches,

                averageOverallScore =
                    Math.Round(
                        averageOverallScore,
                        2),

                matchDistribution = new
                {
                    highMatches,
                    mediumMatches,
                    lowMatches
                },

                topMatches
            });
        }

        // =========================================
        // SERVICE REPORT
        // =========================================

        [HttpGet("services")]
        public async Task<IActionResult> GetServiceReport()
        {
            var totalProviders =
                await _context.ServiceProviderProfiles
                    .CountAsync();

            var approvedProviders =
                await _context.ServiceProviderProfiles
                    .CountAsync(x =>
                        x.VerificationStatus ==
                        "Approved");

            var pendingProviders =
                await _context.ServiceProviderProfiles
                    .CountAsync(x =>
                        x.VerificationStatus ==
                        "Pending");

            var totalOfferings =
                await _context.ServiceOfferings
                    .CountAsync();

            var totalRequests =
                await _context.ServiceRequests
                    .CountAsync();

            var requestsByStatus =
                await _context.ServiceRequests
                    .GroupBy(x => x.Status)
                    .Select(g => new
                    {
                        status = g.Key,
                        count = g.Count()
                    })
                    .OrderByDescending(x =>
                        x.count)
                    .ToListAsync();

            var totalOffers =
                await _context.ServiceOffers
                    .CountAsync();

            var totalReviews =
                await _context.ServiceReviews
                    .CountAsync();

            double averageRating = 0;

            if (totalReviews > 0)
            {
                averageRating =
                    await _context.ServiceReviews
                        .AverageAsync(x =>
                            (double)x.Rating);
            }

            var categories =
                await _context.ServiceCategories
                    .Select(x => new
                    {
                        x.ServiceCategoryId,
                        x.CategoryName,

                        OfferingCount =
                            _context.ServiceOfferings
                                .Count(o =>
                                    o.ServiceCategoryId ==
                                    x.ServiceCategoryId),

                        RequestCount =
                            _context.ServiceRequests
                                .Count(r =>
                                    r.ServiceCategoryId ==
                                    x.ServiceCategoryId)
                    })
                    .OrderByDescending(x =>
                        x.OfferingCount +
                        x.RequestCount)
                    .ToListAsync();

            var topProviders =
                await _context.ServiceProviderProfiles
                    .Select(x => new
                    {
                        x.ServiceProviderProfileId,
                        x.DisplayName,
                        x.VerificationStatus,

                        ReviewCount =
                            _context.ServiceReviews
                                .Count(r =>
                                    r.ServiceProviderProfileId ==
                                    x.ServiceProviderProfileId),

                        AverageRating =
                            _context.ServiceReviews
                                .Where(r =>
                                    r.ServiceProviderProfileId ==
                                    x.ServiceProviderProfileId)
                                .Select(r =>
                                    (double?)r.Rating)
                                .Average() ?? 0
                    })
                    .OrderByDescending(x =>
                        x.AverageRating)
                    .ThenByDescending(x =>
                        x.ReviewCount)
                    .Take(10)
                    .ToListAsync();

            return Ok(new
            {
                providers = new
                {
                    totalProviders,
                    approvedProviders,
                    pendingProviders
                },

                totalOfferings,
                totalRequests,
                totalOffers,

                reviews = new
                {
                    totalReviews,

                    averageRating =
                        Math.Round(
                            averageRating,
                            2)
                },

                requestsByStatus,
                categories,
                topProviders
            });
        }

        // =========================================
        // MARKETPLACE REPORT
        // =========================================

        [HttpGet("marketplace")]
        public async Task<IActionResult>
            GetMarketplaceReport()
        {
            var totalSellers =
                await _context.MarketplaceSellerProfiles
                    .CountAsync();

            var approvedSellers =
                await _context.MarketplaceSellerProfiles
                    .CountAsync(x =>
                        x.VerificationStatus ==
                        "Approved");

            var pendingSellers =
                await _context.MarketplaceSellerProfiles
                    .CountAsync(x =>
                        x.VerificationStatus ==
                        "Pending");

            var totalListings =
                await _context.MarketplaceListings
                    .CountAsync();

            var activeListings =
                await _context.MarketplaceListings
                    .CountAsync(x =>
                        x.Status == "Active");

            var soldListings =
                await _context.MarketplaceListings
                    .CountAsync(x =>
                        x.Status == "Sold");

            var removedListings =
                await _context.MarketplaceListings
                    .CountAsync(x =>
                        x.Status == "Removed");

            var saleListings =
                await _context.MarketplaceListings
                    .CountAsync(x =>
                        x.ListingType == "Sale");

            var rentListings =
                await _context.MarketplaceListings
                    .CountAsync(x =>
                        x.ListingType == "Rent");

            var categories =
                await _context.MarketplaceCategories
                    .Select(x => new
                    {
                        x.MarketplaceCategoryId,
                        x.CategoryName,

                        ListingCount =
                            _context.MarketplaceListings
                                .Count(l =>
                                    l.MarketplaceCategoryId ==
                                    x.MarketplaceCategoryId)
                    })
                    .OrderByDescending(x =>
                        x.ListingCount)
                    .ToListAsync();

            var recentListings =
                await _context.MarketplaceListings
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Take(10)
                    .Select(x => new
                    {
                        x.MarketplaceListingId,
                        x.Title,
                        x.Price,
                        x.ListingType,
                        x.Condition,
                        x.Status,
                        x.CreatedAt,

                        Seller =
                            x.MarketplaceSellerProfile
                                .BusinessName,

                        Category =
                            x.MarketplaceCategory
                                .CategoryName
                    })
                    .ToListAsync();

            return Ok(new
            {
                sellers = new
                {
                    totalSellers,
                    approvedSellers,
                    pendingSellers
                },

                listings = new
                {
                    totalListings,
                    activeListings,
                    soldListings,
                    removedListings,
                    saleListings,
                    rentListings
                },

                categories,
                recentListings
            });
        }

        // =========================================
        // INQUIRY REPORT
        // =========================================

        [HttpGet("inquiries")]
        public async Task<IActionResult> GetInquiryReport()
        {
            var totalInquiries =
                await _context.Inquiries.CountAsync();

            var newInquiries =
                await _context.Inquiries
                    .CountAsync(x =>
                        x.Status == "New");

            var inProgress =
                await _context.Inquiries
                    .CountAsync(x =>
                        x.Status == "InProgress");

            var resolved =
                await _context.Inquiries
                    .CountAsync(x =>
                        x.Status == "Resolved");

            var closed =
                await _context.Inquiries
                    .CountAsync(x =>
                        x.Status == "Closed");

            var inquiriesByType =
                await _context.Inquiries
                    .GroupBy(x => x.InquiryType)
                    .Select(g => new
                    {
                        inquiryType = g.Key,
                        count = g.Count()
                    })
                    .OrderByDescending(x =>
                        x.count)
                    .ToListAsync();

            var recentInquiries =
                await _context.Inquiries
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Take(10)
                    .Select(x => new
                    {
                        x.InquiryId,
                        x.Name,
                        x.Email,
                        x.InquiryType,
                        x.Subject,
                        x.Status,
                        x.CreatedAt
                    })
                    .ToListAsync();

            return Ok(new
            {
                totalInquiries,

                statusSummary = new
                {
                    newInquiries,
                    inProgress,
                    resolved,
                    closed
                },

                inquiriesByType,
                recentInquiries
            });
        }

        // =========================================
        // RECENT SYSTEM ACTIVITY
        // =========================================

        [HttpGet("recent-activity")]
        public async Task<IActionResult>
            GetRecentActivity()
        {
            var recentUsers =
                await _context.Users
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Take(5)
                    .Select(x => new
                    {
                        type = "User",
                        title = x.FullName,
                        description =
                            "New account registered",
                        date = x.CreatedAt
                    })
                    .ToListAsync();

            var recentJobs =
                await _context.Jobs
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Take(5)
                    .Select(x => new
                    {
                        type = "Job",
                        title = x.Title,
                        description =
                            "New job posted",
                        date = x.CreatedAt
                    })
                    .ToListAsync();

            var recentApplications =
                await _context.JobApplications
                    .OrderByDescending(x =>
                        x.AppliedAt)
                    .Take(5)
                    .Select(x => new
                    {
                        type = "Application",

                        title =
                            x.Job.Title,

                        description =
                            "New job application",

                        date =
                            x.AppliedAt
                    })
                    .ToListAsync();

            var recentServices =
                await _context.ServiceRequests
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Take(5)
                    .Select(x => new
                    {
                        type = "Service",
                        title = x.Title,
                        description =
                            "New service request",
                        date = x.CreatedAt
                    })
                    .ToListAsync();

            var recentMarketplace =
                await _context.MarketplaceListings
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Take(5)
                    .Select(x => new
                    {
                        type = "Marketplace",
                        title = x.Title,
                        description =
                            "New marketplace listing",
                        date = x.CreatedAt
                    })
                    .ToListAsync();

            var recentInquiries =
                await _context.Inquiries
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Take(5)
                    .Select(x => new
                    {
                        type = "Inquiry",
                        title = x.Subject,
                        description =
                            "New inquiry received",
                        date = x.CreatedAt
                    })
                    .ToListAsync();

            var activity =
                recentUsers
                    .Concat(recentJobs)
                    .Concat(recentApplications)
                    .Concat(recentServices)
                    .Concat(recentMarketplace)
                    .Concat(recentInquiries)
                    .OrderByDescending(x =>
                        x.date)
                    .Take(20)
                    .ToList();

            return Ok(activity);
        }
    }
}