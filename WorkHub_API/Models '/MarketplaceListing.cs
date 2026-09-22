using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class MarketplaceListing
    {
        public int MarketplaceListingId { get; set; }

        public int MarketplaceSellerProfileId { get; set; }

        public int MarketplaceCategoryId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(3000)]
        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        [MaxLength(20)]
        public string ListingType { get; set; } = "Sale";

        [MaxLength(50)]
        public string Condition { get; set; } = "Used";

        [MaxLength(150)]
        public string Location { get; set; } = string.Empty;

        [MaxLength(30)]
        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public MarketplaceSellerProfile MarketplaceSellerProfile { get; set; } = null!;

        public MarketplaceCategory MarketplaceCategory { get; set; } = null!;
    }
}