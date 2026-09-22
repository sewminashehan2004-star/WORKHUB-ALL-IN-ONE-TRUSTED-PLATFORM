using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class UploadMarketplaceImageDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        public bool IsPrimary { get; set; } = false;
    }
}