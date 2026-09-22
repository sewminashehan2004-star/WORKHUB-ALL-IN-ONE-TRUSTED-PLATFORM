using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class MarketplaceSellerProfileDto
    {
        [MaxLength(150)]
        public string? BusinessName { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(150)]
        public string? Location { get; set; }

        public bool IsBusinessSeller { get; set; }
    }
}