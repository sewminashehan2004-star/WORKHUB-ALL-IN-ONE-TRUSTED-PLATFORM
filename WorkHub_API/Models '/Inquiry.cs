using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class Inquiry
    {
        public int InquiryId { get; set; }

        public int? UserId { get; set; }

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

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "New";

        [MaxLength(2000)]
        public string? AdminNote { get; set; }

        public DateTime CreatedAt { get; set; } =
            DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public string? ReplyMessage { get; set; }

        public DateTime? RepliedAt { get; set; }

        public string? RepliedBy { get; set; }
        public User? User { get; set; 
        }

    }
}