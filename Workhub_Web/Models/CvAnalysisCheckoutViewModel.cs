using System.ComponentModel.DataAnnotations;

namespace Workhub_Web.Models
{
    public class CvAnalysisCheckoutViewModel
    {
        [Range(1, int.MaxValue)]
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        public string PlanCode { get; set; } = "MATCH_ONLY";

        [Required, Display(Name = "Cardholder name")]
        public string CardHolderName { get; set; } = string.Empty;

        [Required, Display(Name = "Card number")]
        public string CardNumber { get; set; } = string.Empty;

        [Range(1, 12)]
        public int ExpiryMonth { get; set; }

        [Range(2026, 2100)]
        public int ExpiryYear { get; set; }

        [Required]
        public string Cvv { get; set; } = string.Empty;

        public CvAnalysisReportViewModel? Report { get; set; }
        public PaymentResultViewModel? Payment { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class CvAnalysisPurchaseApiResponse
    {
        public string Message { get; set; } = string.Empty;
        public PaymentResultViewModel Payment { get; set; } = new();
        public CvAnalysisPlanResultViewModel Plan { get; set; } = new();
        public CvAnalysisJobResultViewModel Job { get; set; } = new();
        public CvAnalysisReportViewModel Report { get; set; } = new();
    }

    public class PaymentResultViewModel
    {
        public string Reference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string? Last4 { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CvAnalysisPlanResultViewModel
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IncludesRecommendations { get; set; }
    }

    public class CvAnalysisJobResultViewModel
    {
        public int JobId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class CvAnalysisReportViewModel
    {
        public decimal MatchPercentage { get; set; }
        public string ReadinessLevel { get; set; } = string.Empty;
        public CvScoreBreakdownViewModel ScoreBreakdown { get; set; } = new();
        public List<string>? MissingSkills { get; set; }
        public List<string>? CvIssues { get; set; }
        public List<string>? Recommendations { get; set; }
    }

    public class CvScoreBreakdownViewModel
    {
        public decimal SkillsScore { get; set; }
        public decimal ExperienceScore { get; set; }
        public decimal EducationScore { get; set; }
        public decimal CvQualityScore { get; set; }
        public decimal CareerRelevanceScore { get; set; }
    }
}
