using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class MarketplaceMessage
    {
        public int MarketplaceMessageId { get; set; }
        public int MarketplaceConversationId { get; set; }
        public int SenderUserId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string MessageText { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        public MarketplaceConversation MarketplaceConversation { get; set; } = null!;
        public User SenderUser { get; set; } = null!;
    }
}
