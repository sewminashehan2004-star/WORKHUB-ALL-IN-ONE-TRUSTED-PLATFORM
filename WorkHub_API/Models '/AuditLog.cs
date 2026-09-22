using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }

        public int? UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? EntityId { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? IpAddress { get; set; }

        public DateTime CreatedAt { get; set; } =
            DateTime.UtcNow;

        public User? User { get; set; }
    }
}