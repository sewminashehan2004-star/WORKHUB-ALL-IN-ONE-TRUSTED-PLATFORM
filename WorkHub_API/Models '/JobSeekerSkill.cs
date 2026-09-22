using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class JobSeekerSkill
    {
        public int JobSeekerSkillId { get; set; }

        public int JobSeekerProfileId { get; set; }

        public int SkillId { get; set; }

        [MaxLength(50)]
        public string? ProficiencyLevel { get; set; }

        public decimal? YearsOfExperience { get; set; }

        public JobSeekerProfile JobSeekerProfile { get; set; } = null!;

        public Skill Skill { get; set; } = null!;
    }
}