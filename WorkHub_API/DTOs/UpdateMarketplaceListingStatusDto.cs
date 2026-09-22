using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class UpdateMarketplaceListingStatusDto
    {
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = string.Empty;
    }
}