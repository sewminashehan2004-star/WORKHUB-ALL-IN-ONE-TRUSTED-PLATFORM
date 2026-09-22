using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class ContactInquiryCreateDto
    {
        public int? UserId { get; set; }


        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;


        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;


        [Required]
        [MaxLength(100)]
        public string Topic { get; set; } = string.Empty;


        [Required]
        [MaxLength(4000)]
        public string Message { get; set; } = string.Empty;
    }
}