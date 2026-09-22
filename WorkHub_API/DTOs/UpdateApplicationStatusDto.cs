using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class UpdateApplicationStatusDto
    {
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = string.Empty;
    }
}