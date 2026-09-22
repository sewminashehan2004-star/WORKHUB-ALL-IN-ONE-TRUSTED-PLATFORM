using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class JobSkill
    {
        public int JobSkillId { get; set; }

        public int JobId { get; set; }

        public int SkillId { get; set; }

        [MaxLength(30)]
        public string Importance { get; set; } = "Required";

        public decimal? RequiredYearsOfExperience { get; set; }

        public Job Job { get; set; } = null!;

        public Skill Skill { get; set; } = null!;
    }
}