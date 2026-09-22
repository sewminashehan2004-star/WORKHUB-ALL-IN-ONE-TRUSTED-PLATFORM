using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class ServiceOffer
    {
        public int ServiceOfferId { get; set; }

        public int ServiceRequestId { get; set; }

        public int ServiceProviderProfileId { get; set; }

        public decimal OfferedPrice { get; set; }

        [MaxLength(1000)]
        public string? Message { get; set; }

        [MaxLength(30)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ServiceRequest ServiceRequest { get; set; } = null!;

        public ServiceProviderProfile ServiceProviderProfile { get; set; } = null!;
    }
}