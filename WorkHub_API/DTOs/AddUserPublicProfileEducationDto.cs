using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class AddUserPublicProfileEducationDto
    {
        [Required]
        [MaxLength(200)]
        public string Institution { get; set; } =
            string.Empty;

        [Required]
        [MaxLength(200)]
        public string Qualification { get; set; } =
            string.Empty;

        [MaxLength(200)]
        public string? FieldOfStudy { get; set; }

        public int? StartYear { get; set; }

        public int? EndYear { get; set; }

        [MaxLength(1500)]
        public string? Description { get; set; }
    }
}