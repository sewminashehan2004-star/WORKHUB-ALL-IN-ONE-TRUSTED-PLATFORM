using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Workhub_Web.Models
{
    public class MarketplaceSellerPortalViewModel
    {
        public MarketplaceSellerProfileEditViewModel Profile { get; set; } = new();

        public MarketplaceListingCreateViewModel NewListing { get; set; } = new();

        public List<MarketplaceCategoryItemViewModel> Categories { get; set; } = new();

        public List<MyMarketplaceListingViewModel> Listings { get; set; } = new();

        public string? ErrorMessage { get; set; }
    }


    public class MarketplaceSellerProfileEditViewModel
    {
        [MaxLength(150)]
        public string? BusinessName { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(150)]
        public string? Location { get; set; }

        public bool IsBusinessSeller { get; set; }

        public string VerificationStatus { get; set; } = string.Empty;
    }


    public class MarketplaceListingCreateViewModel
    {
        [Required]
        public int MarketplaceCategoryId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(3000)]
        public string Description { get; set; } = string.Empty;

        [Range(0, 1000000000)]
        public decimal Price { get; set; }

        [Required]
        [MaxLength(20)]
        public string ListingType { get; set; } = "Sale";

        [Required]
        [MaxLength(50)]
        public string Condition { get; set; } = "Used";

        [MaxLength(150)]
        public string? Location { get; set; }

        public List<IFormFile> Images { get; set; } = new();
    }


    public class MyMarketplaceListingViewModel
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

        public int ImageCount { get; set; }

        public MarketplaceCategoryItemViewModel Category { get; set; } = new();
    }
}