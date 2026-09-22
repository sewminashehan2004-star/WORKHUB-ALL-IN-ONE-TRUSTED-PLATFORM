using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class MarketplaceConversation
    {
        public int MarketplaceConversationId { get; set; }
        public int MarketplaceListingId { get; set; }
        public int BuyerUserId { get; set; }
        public int SellerUserId { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Open";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public MarketplaceListing MarketplaceListing { get; set; } = null!;
        public User BuyerUser { get; set; } = null!;
        public User SellerUser { get; set; } = null!;
        public ICollection<MarketplaceMessage> Messages { get; set; } = new List<MarketplaceMessage>();
    }
}
