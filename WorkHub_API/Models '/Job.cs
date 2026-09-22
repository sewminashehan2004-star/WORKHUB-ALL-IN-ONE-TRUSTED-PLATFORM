using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class Job
    {
        public int JobId { get; set; }

        public int CompanyProfileId { get; set; }


        // =========================================
        // CAREER CLASSIFICATION
        // =========================================

        public int? CareerRoleId { get; set; }


        // =========================================
        // JOB DETAILS
        // =========================================

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }
            = string.Empty;


        [Required]
        [MaxLength(3000)]
        public string Description { get; set; }
            = string.Empty;


        [MaxLength(100)]
        public string? Category { get; set; }


        [MaxLength(150)]
        public string? Location { get; set; }


        [MaxLength(50)]
        public string? JobType { get; set; }


        // =========================================
        // JOB COVER IMAGE
        // =========================================

        [MaxLength(500)]
        public string? ImagePath { get; set; }


        // =========================================
        // SALARY / REQUIREMENTS
        // =========================================

        public decimal? SalaryMin { get; set; }

        public decimal? SalaryMax { get; set; }


        public decimal? MinimumExperienceYears
        {
            get;
            set;
        }


        [MaxLength(200)]
        public string? EducationRequirement
        {
            get;
            set;
        }


        public DateTime? ClosingDate { get; set; }


        [MaxLength(30)]
        public string Status { get; set; }
            = "Active";


        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;


        // =========================================
        // NAVIGATION
        // =========================================

        public CompanyProfile CompanyProfile
        {
            get;
            set;
        } = null!;


        public CareerRole? CareerRole
        {
            get;
            set;
        }
    }
}