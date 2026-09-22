using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class ServiceReview
    {
        public int ServiceReviewId { get; set; }

        public int ServiceRequestId { get; set; }

        public int ServiceProviderProfileId { get; set; }

        public int UserId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1500)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } =
            DateTime.UtcNow;

        // =====================================
        // NAVIGATION PROPERTIES
        // =====================================

        public ServiceRequest ServiceRequest { get; set; } = null!;

        public ServiceProviderProfile ServiceProviderProfile { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}