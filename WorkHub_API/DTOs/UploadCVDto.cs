using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class UploadCVDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        public bool IsPrimary { get; set; } = true;
    }
}