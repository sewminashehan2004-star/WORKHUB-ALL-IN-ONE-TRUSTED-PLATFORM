using System.ComponentModel.DataAnnotations;

namespace Workhub_Web.Models
{
    public class JobSeekerApplicationViewModel
    {
        // =========================================
        // PERSONAL INFORMATION
        // =========================================

        public int UserId { get; set; }

        public string FirstName { get; set; }
            = string.Empty;

        public string LastName { get; set; }
            = string.Empty;

        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}".Trim();
            }
        }

        public string Email { get; set; }
            = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        public string? Country { get; set; }

        public string? Location { get; set; }

        public string? Phone { get; set; }

        public bool HasProfileImage { get; set; }


        // =========================================
        // JOB SEEKER PROFILE
        // =========================================

        public int? JobSeekerProfileId { get; set; }

        public bool JobSeekerProfileExists { get; set; }


        [Display(Name = "Professional Summary")]
        [MaxLength(1000)]
        public string? ProfessionalSummary { get; set; }


        [Display(Name = "Preferred Job Category")]
        [MaxLength(100)]
        public string? PreferredJobCategory { get; set; }


        [Display(Name = "Expected Salary")]
        [Range(
            0,
            100000000,
            ErrorMessage =
                "Please enter a valid expected salary.")]
        public decimal? ExpectedSalary { get; set; }


        [Display(Name = "Current / Preferred Location")]
        [MaxLength(150)]
        public string? CurrentLocation { get; set; }


        // =========================================
        // EDUCATION
        // =========================================

        public JobSeekerEducationFormViewModel EducationForm
        {
            get;
            set;
        } = new();


        public List<JobSeekerEducationItemViewModel> Educations
        {
            get;
            set;
        } = new();


        // =========================================
        // SKILLS
        // =========================================

        public JobSeekerSkillFormViewModel SkillForm
        {
            get;
            set;
        } = new();


        public List<JobSeekerSkillItemViewModel> Skills
        {
            get;
            set;
        } = new();


        public List<SkillOptionViewModel> AvailableSkills
        {
            get;
            set;
        } = new();


        // =========================================
        // WORK EXPERIENCE
        // =========================================

        public JobSeekerExperienceFormViewModel ExperienceForm
        {
            get;
            set;
        } = new();


        public List<JobSeekerExperienceItemViewModel> Experiences
        {
            get;
            set;
        } = new();


        // =========================================
        // CV DOCUMENTS
        // =========================================

        public List<JobSeekerCVItemViewModel> CVDocuments
        {
            get;
            set;
        } = new();


        public JobSeekerCVItemViewModel? PrimaryCV
        {
            get
            {
                return CVDocuments
                    .FirstOrDefault(x => x.IsPrimary);
            }
        }


        // =========================================
        // HELPERS
        // =========================================

        public string Initial
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FirstName))
                {
                    return "U";
                }

                return FirstName
                    .Substring(0, 1)
                    .ToUpperInvariant();
            }
        }
    }


    // =============================================
    // EDUCATION FORM
    // =============================================

    public class JobSeekerEducationFormViewModel
    {
        public int? JobSeekerEducationId { get; set; }


        [Required(
            ErrorMessage =
                "Institution name is required.")]
        [MaxLength(150)]
        public string InstitutionName { get; set; }
            = string.Empty;


        [Required(
            ErrorMessage =
                "Qualification is required.")]
        [MaxLength(150)]
        public string Qualification { get; set; }
            = string.Empty;


        [MaxLength(150)]
        public string? FieldOfStudy { get; set; }


        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }


        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }


        public bool IsCurrentlyStudying { get; set; }
    }


    // =============================================
    // EDUCATION ITEM
    // =============================================

    public class JobSeekerEducationItemViewModel
    {
        public int JobSeekerEducationId { get; set; }

        public string InstitutionName { get; set; }
            = string.Empty;

        public string Qualification { get; set; }
            = string.Empty;

        public string? FieldOfStudy { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsCurrentlyStudying { get; set; }


        public string DateRange
        {
            get
            {
                var start =
                    StartDate.HasValue
                        ? StartDate.Value.ToString("MMM yyyy")
                        : null;


                var end =
                    IsCurrentlyStudying
                        ? "Present"
                        : EndDate.HasValue
                            ? EndDate.Value.ToString("MMM yyyy")
                            : null;


                if (!string.IsNullOrWhiteSpace(start) &&
                    !string.IsNullOrWhiteSpace(end))
                {
                    return $"{start} - {end}";
                }


                if (!string.IsNullOrWhiteSpace(start))
                {
                    return start;
                }


                if (!string.IsNullOrWhiteSpace(end))
                {
                    return end;
                }


                return "Dates not added";
            }
        }
    }


    // =============================================
    // SKILL FORM
    // =============================================

    public class JobSeekerSkillFormViewModel
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int SkillId { get; set; }


        [Required]
        [MaxLength(50)]
        public string ProficiencyLevel { get; set; }
            = string.Empty;


        [Range(0, 50)]
        public decimal YearsOfExperience { get; set; }
    }


    // =============================================
    // SKILL ITEM
    // =============================================

    public class JobSeekerSkillItemViewModel
    {
        public int JobSeekerSkillId { get; set; }

        public int SkillId { get; set; }

        public string SkillName { get; set; }
            = string.Empty;

        public string? Category { get; set; }

        public string ProficiencyLevel { get; set; }
            = string.Empty;

        public decimal YearsOfExperience { get; set; }


        public string ExperienceText
        {
            get
            {
                if (YearsOfExperience == 1)
                {
                    return "1 year";
                }

                return $"{YearsOfExperience:0.#} years";
            }
        }
    }


    // =============================================
    // SKILL DROPDOWN ITEM
    // =============================================

    public class SkillOptionViewModel
    {
        public int SkillId { get; set; }

        public string SkillName { get; set; }
            = string.Empty;

        public string? Category { get; set; }


        public string DisplayText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Category))
                {
                    return SkillName;
                }

                return $"{SkillName} - {Category}";
            }
        }
    }


    // =============================================
    // EXPERIENCE FORM
    // =============================================

    public class JobSeekerExperienceFormViewModel
    {
        public int? JobSeekerExperienceId { get; set; }


        [Required]
        [MaxLength(150)]
        public string JobTitle { get; set; }
            = string.Empty;


        [Required]
        [MaxLength(150)]
        public string CompanyName { get; set; }
            = string.Empty;


        [MaxLength(150)]
        public string? Location { get; set; }


        [Required]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }


        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }


        public bool IsCurrentJob { get; set; }


        [MaxLength(1500)]
        public string? Description { get; set; }
    }


    // =============================================
    // EXPERIENCE ITEM
    // =============================================

    public class JobSeekerExperienceItemViewModel
    {
        public int JobSeekerExperienceId { get; set; }

        public string JobTitle { get; set; }
            = string.Empty;

        public string CompanyName { get; set; }
            = string.Empty;

        public string? Location { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsCurrentJob { get; set; }

        public string? Description { get; set; }


        public string DateRange
        {
            get
            {
                var start =
                    StartDate.ToString("MMM yyyy");


                var end =
                    IsCurrentJob
                        ? "Present"
                        : EndDate.HasValue
                            ? EndDate.Value.ToString("MMM yyyy")
                            : "Not specified";


                return $"{start} - {end}";
            }
        }
    }


    // =============================================
    // CV ITEM
    // =============================================

    public class JobSeekerCVItemViewModel
    {
        public int CVDocumentId { get; set; }

        public string FileName { get; set; }
            = string.Empty;

        public string FileType { get; set; }
            = string.Empty;

        public long FileSize { get; set; }

        public bool IsPrimary { get; set; }

        public DateTime UploadedAt { get; set; }


        // =========================================
        // FILE SIZE DISPLAY
        // =========================================

        public string FileSizeText
        {
            get
            {
                if (FileSize >= 1024 * 1024)
                {
                    return
                        $"{FileSize / 1024d / 1024d:0.00} MB";
                }


                if (FileSize >= 1024)
                {
                    return
                        $"{FileSize / 1024d:0.00} KB";
                }


                return $"{FileSize} bytes";
            }
        }


        // =========================================
        // FILE TYPE DISPLAY
        // =========================================

        public string FileTypeText
        {
            get
            {
                if (FileType.Equals(
                        ".pdf",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return "PDF";
                }


                if (FileType.Equals(
                        ".docx",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return "DOCX";
                }


                return FileType
                    .TrimStart('.')
                    .ToUpperInvariant();
            }
        }


        // =========================================
        // UPLOAD DATE DISPLAY
        // =========================================

        public string UploadedDateText
        {
            get
            {
                return UploadedAt
                    .ToLocalTime()
                    .ToString("dd MMM yyyy");
            }
        }
    }
}