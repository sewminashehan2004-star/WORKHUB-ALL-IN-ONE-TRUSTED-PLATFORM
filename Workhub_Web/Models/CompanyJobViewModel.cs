using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Workhub_Web.Models
{
    public class CompanyJobFormViewModel
    {
        public int? JobId { get; set; }


        [Required]
        [MaxLength(150)]
        [Display(Name = "Job Title")]
        public string Title { get; set; }
            = string.Empty;


        [Required]
        [MaxLength(3000)]
        [Display(Name = "Job Description")]
        public string Description { get; set; }
            = string.Empty;


        [Range(
            1,
            int.MaxValue,
            ErrorMessage =
                "Please select a career category.")]
        public int CareerCategoryId { get; set; }


        [Range(
            1,
            int.MaxValue,
            ErrorMessage =
                "Please select a career role.")]
        public int CareerRoleId { get; set; }


        [MaxLength(150)]
        public string? Location { get; set; }


        [MaxLength(50)]
        public string? JobType { get; set; }


        // =========================================
        // JOB IMAGE
        // =========================================

        [Display(Name = "Job Cover Image")]
        public IFormFile? JobImage { get; set; }


        public string? ExistingImagePath
        {
            get;
            set;
        }


        public decimal? SalaryMin { get; set; }

        public decimal? SalaryMax { get; set; }


        [Range(0, 100)]
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


        [DataType(DataType.Date)]
        public DateTime? ClosingDate { get; set; }


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


        public List<CompanyJobSkillFormViewModel>
            Skills
        { get; set; }
            = new();


        public string? SuccessMessage { get; set; }

        public string? ErrorMessage { get; set; }
    }


    public class CompanyCareerCategoryViewModel
    {
        public int CareerCategoryId { get; set; }

        public string CategoryName { get; set; }
            = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }
            = true;
    }


    public class CompanyCareerRoleViewModel
    {
        public int CareerRoleId { get; set; }

        public int CareerCategoryId { get; set; }

        public string RoleName { get; set; }
            = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }
            = true;
    }


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


    public class CompanyJobSkillFormViewModel
    {
        public int SkillId { get; set; }

        public string SkillName { get; set; }
            = string.Empty;

        public bool Selected { get; set; }

        public string Importance { get; set; }
            = "Required";

        [Range(0, 100)]
        public decimal?
            RequiredYearsOfExperience
        {
            get;
            set;
        }
    }


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


        // =========================================
        // IMAGE PATH SENT TO API
        // =========================================

        public string? ImagePath { get; set; }


        public decimal? SalaryMin { get; set; }

        public decimal? SalaryMax { get; set; }

        public decimal?
            MinimumExperienceYears
        {
            get;
            set;
        }

        public string? EducationRequirement
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

        public string? ImagePath { get; set; }

        public string? Status { get; set; }
    }
}