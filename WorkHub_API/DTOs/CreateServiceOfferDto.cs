using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class CreateServiceOfferDto
    {
        [Range(0, 100000000)]
        public decimal OfferedPrice { get; set; }

        [MaxLength(1000)]
        public string? Message { get; set; }
    }
}