using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class NewsUpdate
    {
        public int NewsUpdateId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Summary { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = "General";

        public bool IsPublished { get; set; }

        public DateTime? PublishedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [MaxLength(200)]
        public string? CreatedBy { get; set; }
    }
}