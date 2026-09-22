using System.ComponentModel.DataAnnotations;

namespace Workhub_Web.Models
{
    // =============================================
    // COMPANY JOB FORM
    // =============================================

    public class CompanyJobFormViewModel
    {
        public int? JobId { get; set; }


        // =========================================
        // JOB DETAILS
        // =========================================

        [Required]
        [MaxLength(200)]
        [Display(Name = "Job Title")]
        public string Title { get; set; }
            = string.Empty;


        [Required]
        [Display(Name = "Job Description")]
        public string Description { get; set; }
            = string.Empty;


        [Required]
        [Display(Name = "Career Category")]
        public int CareerCategoryId { get; set; }


        [Required]
        [Display(Name = "Career Role")]
        public int CareerRoleId { get; set; }


        [MaxLength(150)]
        public string? Location { get; set; }


        [MaxLength(100)]
        [Display(Name = "Job Type")]
        public string? JobType { get; set; }


        [Display(Name = "Minimum Salary")]
        public decimal? SalaryMin { get; set; }


        [Display(Name = "Maximum Salary")]
        public decimal? SalaryMax { get; set; }


        [Range(
            0,
            100,
            ErrorMessage =
                "Minimum experience cannot be negative.")]
        [Display(Name = "Minimum Experience")]
        public decimal? MinimumExperienceYears { get; set; }


        [MaxLength(500)]
        [Display(Name = "Education Requirement")]
        public string? EducationRequirement { get; set; }


        [Display(Name = "Closing Date")]
        [DataType(DataType.Date)]
        public DateTime? ClosingDate { get; set; }


        // =========================================
        // MASTER DATA
        // =========================================

        public List<CompanyCareerCategoryViewModel>
            Categories
        { get; set; }
            = new();


        public List<CompanyCareerRoleViewModel>
            Roles
        { get; set; }
            = new();


        public List<CompanyJobSkillOptionViewModel>
            AvailableSkills
        { get; set; }
            = new();


        // =========================================
        // SELECTED JOB SKILLS
        // =========================================

        public List<CompanyJobSkillFormViewModel>
            Skills
        { get; set; }
            = new();


        // =========================================
        // PAGE MESSAGE
        // =========================================

        public string? SuccessMessage { get; set; }

        public string? ErrorMessage { get; set; }
    }


    // =============================================
    // CAREER CATEGORY
    // =============================================

    public class CompanyCareerCategoryViewModel
    {
        public int CareerCategoryId { get; set; }

        public string CategoryName { get; set; }
            = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }


    // =============================================
    // CAREER ROLE
    // =============================================

    public class CompanyCareerRoleViewModel
    {
        public int CareerRoleId { get; set; }

        public int CareerCategoryId { get; set; }

        public string RoleName { get; set; }
            = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }


    // =============================================
    // SKILL OPTION RETURNED FROM CAREER ROLE
    // =============================================

    public class CompanyJobSkillOptionViewModel
    {
        public int SkillId { get; set; }

        public string SkillName { get; set; }
            = string.Empty;

        public string? Category { get; set; }

        public string Importance { get; set; }
            = "Required";

        public decimal?
            RecommendedYearsOfExperience
        {
            get;
            set;
        }
    }


    // =============================================
    // SKILL SELECTED FOR JOB
    //
    // Matches API JobSkillRequirementDto
    // =============================================

    public class CompanyJobSkillFormViewModel
    {
        public int SkillId { get; set; }

        public string SkillName { get; set; }
            = string.Empty;

        public bool Selected { get; set; }

        public string Importance { get; set; }
            = "Required";

        [Range(
            0,
            100,
            ErrorMessage =
                "Skill experience cannot be negative.")]
        public decimal?
            RequiredYearsOfExperience
        {
            get;
            set;
        }
    }


    // =============================================
    // REQUEST SENT TO WORKHUB API
    // =============================================

    public class CompanyJobApiRequest
    {
        public string Title { get; set; }
            = string.Empty;

        public string Description { get; set; }
            = string.Empty;

        public int CareerRoleId { get; set; }

        public string? Category { get; set; }

        public string? Location { get; set; }

        public string? JobType { get; set; }

        public decimal? SalaryMin { get; set; }

        public decimal? SalaryMax { get; set; }

        public decimal?
            MinimumExperienceYears
        {
            get;
            set;
        }

        public string?
            EducationRequirement
        {
            get;
            set;
        }

        public DateTime? ClosingDate { get; set; }

        public List<CompanyJobSkillApiRequest>
            Skills
        { get; set; }
            = new();
    }


    // =============================================
    // JOB SKILL API REQUEST
    // =============================================

    public class CompanyJobSkillApiRequest
    {
        public int SkillId { get; set; }

        public string Importance { get; set; }
            = "Required";

        public decimal?
            RequiredYearsOfExperience
        {
            get;
            set;
        }
    }


    // =============================================
    // CREATE JOB API RESPONSE
    // =============================================

    public class CompanyJobCreateApiResponse
    {
        public string Message { get; set; }
            = string.Empty;

        public int JobId { get; set; }

        public string Title { get; set; }
            = string.Empty;

        public string? Category { get; set; }

        public int CareerRoleId { get; set; }

        public string? CareerRoleName { get; set; }

        public string? CareerCategoryName { get; set; }

        public string? Status { get; set; }
    }
}