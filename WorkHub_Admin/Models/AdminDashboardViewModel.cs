namespace WorkHub_Admin.Models
{
    public class AdminDashboardViewModel
    {
        // =========================================
        // LOGGED-IN ADMIN
        // =========================================

        public string AdminFullName { get; set; }
            = string.Empty;

        public string AdminEmail { get; set; }
            = string.Empty;


        // =========================================
        // DASHBOARD COUNTS
        // =========================================

        public int PendingCompanyRequestCount { get; set; }

        public int TotalUsers { get; set; }

        public int TotalCompanies { get; set; }

        public int TotalJobs { get; set; }

        public int TotalApplications { get; set; }

        public int TotalServiceProviders { get; set; }

        public int TotalMarketplaceSellers { get; set; }

        public int TotalServiceRequests { get; set; }

        public int TotalMarketplaceListings { get; set; }

        public int NewInquiries { get; set; }


        // =========================================
        // PENDING COMPANY REQUESTS
        // =========================================

        public List<AdminPendingCompanyViewModel>
            PendingCompanies
        { get; set; }
            = new();


        // =========================================
        // PAGE MESSAGE
        // =========================================

        public string? SuccessMessage { get; set; }

        public string? ErrorMessage { get; set; }
    }


    // =============================================
    // COMPANY REQUEST
    // =============================================

    public class AdminPendingCompanyViewModel
    {
        public int CompanyProfileId { get; set; }

        public int UserId { get; set; }


        public string CompanyName { get; set; }
            = string.Empty;


        public string ContactPerson { get; set; }
            = string.Empty;


        public string Email { get; set; }
            = string.Empty;


        public string? Phone { get; set; }

        public string? Location { get; set; }

        public string? Industry { get; set; }

        public string? Website { get; set; }

        public string? Description { get; set; }

        public string? Address { get; set; }


        public string AccountStatus { get; set; }
            = string.Empty;


        public string VerificationStatus { get; set; }
            = string.Empty;


        public bool IsActive { get; set; }


        public DateTime SubmittedAt { get; set; }


        // =========================================
        // DISPLAY
        // =========================================

        public string SubmittedDateText
        {
            get
            {
                return SubmittedAt
                    .ToLocalTime()
                    .ToString(
                        "dd MMM yyyy");
            }
        }
    }


    // =============================================
    // API RESPONSE:
    //
    // GET
    // api/Admin/CompanyApprovals/pending
    // =============================================

    public class PendingCompanyApiResponse
    {
        public int Count { get; set; }


        public List<AdminPendingCompanyViewModel>
            Companies
        { get; set; }
            = new();
    }

    public class AdminSystemDashboardApiResponse
    {
        public AdminUsersStats Users { get; set; } = new();
        public AdminCompaniesStats Companies { get; set; } = new();
        public AdminServiceProviderStats ServiceProviders { get; set; } = new();
        public AdminMarketplaceSellerStats MarketplaceSellers { get; set; } = new();
        public AdminJobsStats Jobs { get; set; } = new();
        public AdminServicesStats Services { get; set; } = new();
        public AdminMarketplaceStats Marketplace { get; set; } = new();
        public AdminInquiryStats Inquiries { get; set; } = new();
    }

    public class AdminUsersStats { public int TotalUsers { get; set; } public int TotalJobSeekers { get; set; } }
    public class AdminCompaniesStats { public int TotalCompanies { get; set; } public int PendingCompanies { get; set; } }
    public class AdminServiceProviderStats { public int TotalServiceProviders { get; set; } public int PendingServiceProviders { get; set; } }
    public class AdminMarketplaceSellerStats { public int TotalMarketplaceSellers { get; set; } public int PendingMarketplaceSellers { get; set; } }
    public class AdminJobsStats { public int TotalJobs { get; set; } public int ActiveJobs { get; set; } public int TotalApplications { get; set; } }
    public class AdminServicesStats { public int TotalServiceRequests { get; set; } }
    public class AdminMarketplaceStats { public int TotalMarketplaceListings { get; set; } }
    public class AdminInquiryStats { public int TotalInquiries { get; set; } public int NewInquiries { get; set; } }

}