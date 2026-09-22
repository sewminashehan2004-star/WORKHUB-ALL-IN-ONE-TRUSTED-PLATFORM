using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class JobScreeningQuestion
    {
        public int JobScreeningQuestionId { get; set; }
        public int JobId { get; set; }

        [Required]
        [MaxLength(500)]
        public string QuestionText { get; set; } = string.Empty;

        [MaxLength(30)]
        public string QuestionType { get; set; } = "Text";

        [MaxLength(1500)]
        public string? OptionsJson { get; set; }

        public bool IsRequired { get; set; } = true;
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Job Job { get; set; } = null!;
        public ICollection<JobScreeningAnswer> Answers { get; set; } = new List<JobScreeningAnswer>();
    }
}
