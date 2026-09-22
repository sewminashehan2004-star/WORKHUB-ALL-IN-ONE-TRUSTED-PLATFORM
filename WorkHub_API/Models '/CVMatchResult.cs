using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class CVMatchResult
    {
        public int CVMatchResultId { get; set; }

        public int JobApplicationId { get; set; }

        public decimal SkillsScore { get; set; }

        public decimal ExperienceScore { get; set; }

        public decimal EducationScore { get; set; }

        public decimal PreferredSkillsScore { get; set; }

        public decimal RelevanceScore { get; set; }

        public decimal LocationScore { get; set; }

        public decimal OverallScore { get; set; }

        [MaxLength(3000)]
        public string? MatchedSkills { get; set; }

        [MaxLength(3000)]
        public string? MissingSkills { get; set; }

        [MaxLength(4000)]
        public string? Recommendations { get; set; }

        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

        public JobApplication JobApplication { get; set; } = null!;
    }
}