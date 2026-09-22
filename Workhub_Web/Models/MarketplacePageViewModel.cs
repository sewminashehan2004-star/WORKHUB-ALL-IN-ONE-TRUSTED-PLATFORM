namespace Workhub_Web.Models
{
    public class MarketplacePageViewModel
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public string? ListingType { get; set; }
        public string? Location { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<MarketplaceCategoryItemViewModel> Categories { get; set; } = new();
        public List<MarketplaceListingCardViewModel> Listings { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class MarketplaceCategoryItemViewModel
    {
        public int MarketplaceCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class MarketplaceListingCardViewModel
    {
        public int MarketplaceListingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ListingType { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string? Location { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Category { get; set; } = string.Empty;
        public MarketplaceSellerMiniViewModel Seller { get; set; } = new();
        public int? PrimaryImageId { get; set; }
        public string? PrimaryImage { get; set; }
    }

    public class MarketplaceSellerMiniViewModel
    {
        public int MarketplaceSellerProfileId { get; set; }
        public string? BusinessName { get; set; }
        public string? Location { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public bool IsBusinessSeller { get; set; }
    }

    public class MarketplaceDetailsViewModel
    {
        public int MarketplaceListingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ListingType { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public MarketplaceCategoryDetailsViewModel Category { get; set; } = new();
        public MarketplaceSellerDetailsViewModel Seller { get; set; } = new();
        public List<MarketplaceImageViewModel> Images { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class MarketplaceCategoryDetailsViewModel
    {
        public int MarketplaceCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }

    public class MarketplaceSellerDetailsViewModel
    {
        public int MarketplaceSellerProfileId { get; set; }
        public string? BusinessName { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
        public bool IsBusinessSeller { get; set; }
    }

    public class MarketplaceImageViewModel
    {
        public int MarketplaceImageId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}
