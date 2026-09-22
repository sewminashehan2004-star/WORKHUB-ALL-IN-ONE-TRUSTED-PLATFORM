using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class CreateInquiryDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required]
        [MaxLength(50)]
        public string InquiryType { get; set; } = "General";

        [Required]
        [MaxLength(150)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(3000)]
        public string Message { get; set; } = string.Empty;
    }
}