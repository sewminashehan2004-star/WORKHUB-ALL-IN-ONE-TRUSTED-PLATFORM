using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class UpdateVerificationStatusDto
    {
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = string.Empty;
    }
}