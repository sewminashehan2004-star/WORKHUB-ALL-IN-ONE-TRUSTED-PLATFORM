using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class JobSeekerEducation
    {
        public int JobSeekerEducationId { get; set; }

        public int JobSeekerProfileId { get; set; }

        [Required]
        [MaxLength(150)]
        public string InstitutionName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Qualification { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? FieldOfStudy { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsCurrentlyStudying { get; set; }

        public JobSeekerProfile JobSeekerProfile { get; set; } = null!;
    }
}