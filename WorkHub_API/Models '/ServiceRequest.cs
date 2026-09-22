using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class ServiceRequest
    {
        public int ServiceRequestId { get; set; }

        public int UserId { get; set; }

        public int ServiceCategoryId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Location { get; set; }

        public DateTime? PreferredDate { get; set; }

        public decimal? Budget { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Open";

        public DateTime CreatedAt { get; set; } =
            DateTime.UtcNow;

        // =====================================
        // NAVIGATION PROPERTIES
        // =====================================

        public User User { get; set; } = null!;

        public ServiceCategory ServiceCategory { get; set; } = null!;

        public ICollection<ServiceOffer> ServiceOffers { get; set; } =
            new List<ServiceOffer>();

        public ServiceReview? Review { get; set; }
    }
}