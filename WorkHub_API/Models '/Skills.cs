using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class Skill
    {
        public int SkillId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SkillName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Category { get; set; }

        public bool IsActive { get; set; } = true;
    }
}