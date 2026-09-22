using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class UpdateInquiryDto
    {
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? AdminNote { get; set; }
    }
}