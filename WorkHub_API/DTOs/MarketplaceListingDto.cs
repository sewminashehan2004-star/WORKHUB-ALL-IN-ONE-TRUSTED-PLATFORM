using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class MarketplaceListingDto
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
    }
}