namespace WorkHub.API.Models
{
    public class MarketplaceImage
    {
        public int MarketplaceImageId { get; set; }

        public int MarketplaceListingId { get; set; }

        public string ImagePath { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        public MarketplaceListing MarketplaceListing { get; set; } = null!;
    }
}