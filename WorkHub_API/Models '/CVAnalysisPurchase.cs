using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class CVAnalysisPurchase
    {
        public int CVAnalysisPurchaseId { get; set; }
        public int UserId { get; set; }
        public int JobId { get; set; }
        public int CVDocumentId { get; set; }

        [Required]
        [MaxLength(30)]
        public string PlanCode { get; set; } = "MATCH_ONLY";

        public decimal Amount { get; set; }

        [Required]
        [MaxLength(20)]
        public string Currency { get; set; } = "USD";

        [Required]
        [MaxLength(30)]
        public string PaymentStatus { get; set; } = "Paid";

        [MaxLength(80)]
        public string? PaymentReference { get; set; }

        [MaxLength(30)]
        public string? PaymentMethod { get; set; }

        [MaxLength(4)]
        public string? CardLast4 { get; set; }

        public decimal MatchPercentage { get; set; }

        [MaxLength(4000)]
        public string? MissingSkills { get; set; }

        [MaxLength(4000)]
        public string? Recommendations { get; set; }

        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public Job Job { get; set; } = null!;
        public CVDocument CVDocument { get; set; } = null!;
    }
}
