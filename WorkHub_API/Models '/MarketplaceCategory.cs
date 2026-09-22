using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class MarketplaceCategory
    {
        public int MarketplaceCategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}