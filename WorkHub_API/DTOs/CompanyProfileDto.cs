using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class CompanyProfileDto
    {
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
    }
}