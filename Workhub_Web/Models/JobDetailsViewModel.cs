using System.ComponentModel.DataAnnotations;

namespace Workhub_Web.Models
{
    public class JobDetailsViewModel
    {
        public int JobId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int? CareerRoleId { get; set; }
        public string? CareerRoleName { get; set; }
        public string? CareerCategoryName { get; set; }
        public string? Location { get; set; }
        public string? JobType { get; set; }
        public string? ImagePath { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public decimal? MinimumExperienceYears { get; set; }
        public string? EducationRequirement { get; set; }
        public DateTime? ClosingDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public JobCompanyViewModel Company { get; set; } = new();
        public List<JobSkillViewModel> Skills { get; set; } = new();
        public List<JobScreeningQuestionViewModel> ScreeningQuestions { get; set; } = new();

        // Application-specific information.
        [MaxLength(2000)]
        public string? CoverLetter { get; set; }

        public List<JobScreeningAnswerInputViewModel> ScreeningAnswers { get; set; } = new();

        // Personal details shown on the application form. These are pre-filled
        // from the signed-in user's WorkHub profile and can be reviewed before
        // the application is submitted.
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(60)]
        public string ApplicantFirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(100)]
        public string ApplicantLastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [MaxLength(150)]
        public string ApplicantEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [MaxLength(50)]
        public string ApplicantPhone { get; set; } = string.Empty;

        [MaxLength(180)]
        public string? ApplicantLocation { get; set; }

        [MaxLength(50)]
        public string? ApplicantNationalIdNumber { get; set; }

        public bool HasJobSeekerProfile { get; set; }
        public int? PrimaryCvDocumentId { get; set; }
        public string? PrimaryCvFileName { get; set; }
        public bool HasPrimaryCv => PrimaryCvDocumentId.HasValue;

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }

    public class JobCompanyViewModel
    {
        public int CompanyProfileId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? Industry { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
    }

    public class JobSkillViewModel
    {
        public int SkillId { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string Importance { get; set; } = string.Empty;
        public decimal? RequiredYearsOfExperience { get; set; }
    }

    public class JobScreeningQuestionViewModel
    {
        public int JobScreeningQuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "Text";
        public List<string> Options { get; set; } = new();
        public bool IsRequired { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class JobScreeningAnswerInputViewModel
    {
        public int QuestionId { get; set; }
        public string? Answer { get; set; }
    }

    public class ApplyJobApiResponseViewModel
    {
        public int JobApplicationId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
