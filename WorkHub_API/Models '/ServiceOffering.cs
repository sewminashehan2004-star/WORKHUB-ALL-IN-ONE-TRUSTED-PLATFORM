using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class ServiceOffering
    {
        public int ServiceOfferingId { get; set; }

        public int ServiceProviderProfileId { get; set; }

        public int ServiceCategoryId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public decimal StartingPrice { get; set; }

        [MaxLength(150)]
        public string? Location { get; set; }

        public bool IsAvailable { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ServiceProviderProfile ServiceProviderProfile { get; set; } = null!;

        public ServiceCategory ServiceCategory { get; set; } = null!;
    }
}