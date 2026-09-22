using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class AddJobSeekerSkillDto
    {
        [Required]
        public int SkillId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProficiencyLevel { get; set; } = string.Empty;

        [Range(0, 50)]
        public decimal YearsOfExperience { get; set; }
    }
}