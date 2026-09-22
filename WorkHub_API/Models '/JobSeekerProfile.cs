using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class JobSeekerProfile
    {
        public int JobSeekerProfileId { get; set; }

        public int UserId { get; set; }

        [MaxLength(1000)]
        public string? ProfessionalSummary { get; set; }

        [MaxLength(100)]
        public string? PreferredJobCategory { get; set; }

        public decimal? ExpectedSalary { get; set; }

        [MaxLength(150)]
        public string? CurrentLocation { get; set; }

        public string? ProfileImagePath { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}