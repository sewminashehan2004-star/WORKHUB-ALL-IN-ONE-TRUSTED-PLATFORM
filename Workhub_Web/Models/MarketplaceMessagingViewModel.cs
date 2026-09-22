namespace Workhub_Web.Models
{
    public class MarketplaceInboxViewModel
    {
        public List<MarketplaceConversationItemViewModel> Conversations { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }

    public class MarketplaceConversationItemViewModel
    {
        public int MarketplaceConversationId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public ConversationListingViewModel Listing { get; set; } = new();
        public ConversationPartyViewModel OtherParty { get; set; } = new();
        public ConversationLastMessageViewModel? LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }

    public class ConversationListingViewModel
    {
        public int MarketplaceListingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ListingType { get; set; } = string.Empty;
        public string? Location { get; set; }
    }

    public class ConversationPartyViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    public class ConversationLastMessageViewModel
    {
        public string MessageText { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public int SenderUserId { get; set; }
    }

    public class MarketplaceChatViewModel
    {
        public MarketplaceConversationHeaderViewModel Conversation { get; set; } = new();
        public List<MarketplaceChatMessageViewModel> Messages { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class MarketplaceConversationHeaderViewModel
    {
        public int MarketplaceConversationId { get; set; }
        public string Status { get; set; } = string.Empty;
        public ConversationListingViewModel Listing { get; set; } = new();
        public ConversationPartyViewModel Buyer { get; set; } = new();
        public ConversationPartyViewModel Seller { get; set; } = new();
    }

    public class MarketplaceChatMessageViewModel
    {
        public int MarketplaceMessageId { get; set; }
        public int SenderUserId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string MessageText { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsMine { get; set; }
    }

    public class MarketplaceChatApiResponse
    {
        public MarketplaceConversationHeaderViewModel Conversation { get; set; } = new();
        public List<MarketplaceChatMessageViewModel> Messages { get; set; } = new();
    }
}
