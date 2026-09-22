using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class JobApplication
    {
        public int JobApplicationId { get; set; }

        public int JobId { get; set; }

        public int JobSeekerProfileId { get; set; }

        public int? CVDocumentId { get; set; }

        [MaxLength(30)]
        public string Status { get; set; } = "Received";

        [MaxLength(2000)]
        public string? CoverLetter { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public Job Job { get; set; } = null!;

        public JobSeekerProfile JobSeekerProfile { get; set; } = null!;

        public CVDocument? CVDocument { get; set; }
    }
}