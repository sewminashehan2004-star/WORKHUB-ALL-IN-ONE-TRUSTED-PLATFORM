using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class ServiceOfferingDto
    {
        [Required]
        public int ServiceCategoryId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Range(0, 100000000)]
        public decimal StartingPrice { get; set; }

        [MaxLength(150)]
        public string? Location { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}