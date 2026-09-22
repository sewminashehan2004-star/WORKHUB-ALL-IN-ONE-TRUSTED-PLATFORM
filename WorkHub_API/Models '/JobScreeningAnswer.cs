using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class JobScreeningAnswer
    {
        public int JobScreeningAnswerId { get; set; }
        public int JobApplicationId { get; set; }
        public int JobScreeningQuestionId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string AnswerText { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public JobApplication JobApplication { get; set; } = null!;
        public JobScreeningQuestion JobScreeningQuestion { get; set; } = null!;
    }
}
