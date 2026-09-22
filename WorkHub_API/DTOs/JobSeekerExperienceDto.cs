using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class JobSeekerExperienceDto
    {
        [Required]
        [MaxLength(150)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Location { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsCurrentJob { get; set; }

        [MaxLength(1500)]
        public string? Description { get; set; }
    }
}