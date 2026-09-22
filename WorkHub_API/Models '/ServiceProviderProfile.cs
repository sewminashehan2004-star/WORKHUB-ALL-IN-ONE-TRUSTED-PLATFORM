using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class ServiceProviderProfile
    {
        public int ServiceProviderProfileId { get; set; }

        public int UserId { get; set; }

        [MaxLength(100)]
        public string? DisplayName { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }

        [MaxLength(150)]
        public string? Location { get; set; }

        public int YearsOfExperience { get; set; }

        public decimal? StartingPrice { get; set; }

        [MaxLength(30)]
        public string VerificationStatus { get; set; } = "Pending";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}