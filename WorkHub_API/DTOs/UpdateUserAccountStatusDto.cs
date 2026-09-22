using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class UpdateUserAccountStatusDto
    {
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;
    }
}