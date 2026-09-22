using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class ServiceCategoryAdminDto
    {
        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}