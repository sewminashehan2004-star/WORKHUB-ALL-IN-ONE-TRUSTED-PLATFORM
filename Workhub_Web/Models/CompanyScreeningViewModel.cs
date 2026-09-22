using System.ComponentModel.DataAnnotations;

namespace Workhub_Web.Models
{
    public class CompanyScreeningViewModel
    {
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public List<JobScreeningQuestionViewModel> Questions { get; set; } = new();

        [Required, MaxLength(500)]
        public string NewQuestionText { get; set; } = string.Empty;
        public string NewQuestionType { get; set; } = "Text";
        public string? NewOptions { get; set; }
        public bool NewQuestionRequired { get; set; } = true;
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }
}
