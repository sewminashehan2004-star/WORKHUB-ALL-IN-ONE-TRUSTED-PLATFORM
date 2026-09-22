using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class CreateServiceReviewDto
    {
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1500)]
        public string? Comment { get; set; }
    }
}