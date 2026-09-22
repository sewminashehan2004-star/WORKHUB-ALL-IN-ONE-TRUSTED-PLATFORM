using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class JobSkillRequirementDto
    {
        [Required]
        public int SkillId { get; set; }

        [Required]
        [MaxLength(30)]
        public string Importance { get; set; } = "Required";

        [Range(0, 50)]
        public decimal? RequiredYearsOfExperience { get; set; }
    }
}