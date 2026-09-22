using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class CompanyProfile
    {
        public int CompanyProfileId { get; set; }

        public int UserId { get; set; }

        [Required]
        [MaxLength(150)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Industry { get; set; }

        [MaxLength(200)]
        public string? Website { get; set; }

        [MaxLength(1500)]
        public string? Description { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        public string? LogoPath { get; set; }

        [MaxLength(30)]
        public string VerificationStatus { get; set; } = "Pending";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}