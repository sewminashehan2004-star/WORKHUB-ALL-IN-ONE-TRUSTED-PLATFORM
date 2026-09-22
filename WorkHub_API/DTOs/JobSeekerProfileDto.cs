using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class JobSeekerProfileDto
    {
        [MaxLength(1000)]
        public string? ProfessionalSummary { get; set; }

        [MaxLength(100)]
        public string? PreferredJobCategory { get; set; }

        public decimal? ExpectedSalary { get; set; }

        [MaxLength(150)]
        public string? CurrentLocation { get; set; }
    }
}