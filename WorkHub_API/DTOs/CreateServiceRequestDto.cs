using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class CreateServiceRequestDto
    {
        [Required]
        public int ServiceCategoryId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Location { get; set; }

        public DateTime? PreferredDate { get; set; }

        [Range(0, 100000000)]
        public decimal? Budget { get; set; }
    }
}