namespace WorkHub_Admin.Models
{
    public class AdminOperationsViewModel
    {
        public List<AdminUserItem> Users { get; set; } = new();
        public List<AdminCompanyItem> Companies { get; set; } = new();
        public List<AdminProviderItem> ServiceProviders { get; set; } = new();
        public List<AdminSellerItem> MarketplaceSellers { get; set; } = new();
        public List<AdminJobItem> Jobs { get; set; } = new();
        public List<AdminListingItem> MarketplaceListings { get; set; } = new();
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class AdminUserItem
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public string Role { get; set; } = string.Empty;
        public string AccountStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class AdminCompanyItem
    {
        public int CompanyProfileId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? Industry { get; set; }
        public string? Website { get; set; }
        public string? Address { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public AdminRelatedUser User { get; set; } = new();
    }

    public class AdminProviderItem
    {
        public int ServiceProviderProfileId { get; set; }
        public string? DisplayName { get; set; }
        public string? Location { get; set; }
        public int YearsOfExperience { get; set; }
        public decimal? StartingPrice { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public AdminRelatedUser User { get; set; } = new();
    }

    public class AdminSellerItem
    {
        public int MarketplaceSellerProfileId { get; set; }
        public string? BusinessName { get; set; }
        public string? Location { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public bool IsBusinessSeller { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public AdminRelatedUser User { get; set; } = new();
    }

    public class AdminJobItem
    {
        public int JobId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? Location { get; set; }
        public string? JobType { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosingDate { get; set; }
        public AdminJobCompany Company { get; set; } = new();
    }

    public class AdminListingItem
    {
        public int MarketplaceListingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ListingType { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public AdminListingSeller Seller { get; set; } = new();
    }

    public class AdminRelatedUser
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AccountStatus { get; set; } = string.Empty;
    }

    public class AdminJobCompany
    {
        public int CompanyProfileId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
    }

    public class AdminListingSeller
    {
        public int MarketplaceSellerProfileId { get; set; }
        public string? BusinessName { get; set; }
        public string? FullName { get; set; }
    }
}
