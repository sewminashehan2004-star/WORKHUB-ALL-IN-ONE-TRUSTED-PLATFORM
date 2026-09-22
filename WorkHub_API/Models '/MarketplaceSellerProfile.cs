using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class MarketplaceSellerProfile
    {
        public int MarketplaceSellerProfileId { get; set; }

        public int UserId { get; set; }

        [MaxLength(150)]
        public string? BusinessName { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(150)]
        public string? Location { get; set; }

        [MaxLength(30)]
        public string VerificationStatus { get; set; } = "Pending";

        public bool IsBusinessSeller { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}