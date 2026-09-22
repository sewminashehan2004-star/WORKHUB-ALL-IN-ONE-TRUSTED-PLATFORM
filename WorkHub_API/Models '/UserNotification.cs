using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class UserNotification
    {
        public int UserNotificationId { get; set; }
        public int UserId { get; set; }

        [Required, MaxLength(160)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(1500)]
        public string Message { get; set; } = string.Empty;

        [Required, MaxLength(60)]
        public string NotificationType { get; set; } = "System";

        public int? RelatedInquiryId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
    }
}
