using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class JobDto
    {
        // =========================================
        // CAREER ROLE
        // =========================================

        [Required]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage =
                "Please select a valid career role.")]
        public int CareerRoleId { get; set; }


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


        // =========================================
        // LEGACY CATEGORY
        // =========================================

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
        // SALARY
        // =========================================

        public decimal? SalaryMin { get; set; }

        public decimal? SalaryMax { get; set; }


        // =========================================
        // REQUIREMENTS
        // =========================================

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


        // =========================================
        // SKILLS
        // =========================================

        public List<JobSkillRequirementDto> Skills
        {
            get;
            set;
        } = new();
    }
}