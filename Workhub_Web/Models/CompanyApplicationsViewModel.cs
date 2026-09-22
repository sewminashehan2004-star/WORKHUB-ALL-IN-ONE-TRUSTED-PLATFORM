namespace Workhub_Web.Models
{
    // =============================================
    // COMPANY APPLICATIONS PAGE
    // =============================================

    public class CompanyApplicationsViewModel
    {
        public List<CompanyApplicationJobViewModel>
            Jobs
        { get; set; }
            = new();


        public int? SelectedJobId
        {
            get;
            set;
        }


        public string? SelectedJobTitle
        {
            get;
            set;
        }


        public int TotalApplicants
        {
            get;
            set;
        }


        public List<CompanyApplicantItemViewModel>
            Applicants
        { get; set; }
            = new();


        public string? SuccessMessage
        {
            get;
            set;
        }


        public string? ErrorMessage
        {
            get;
            set;
        }


        // =========================================
        // STATISTICS
        // =========================================

        public int ReceivedCount =>
            Applicants.Count(x =>
                string.Equals(
                    x.Status,
                    "Received",
                    StringComparison.OrdinalIgnoreCase));


        public int UnderReviewCount =>
            Applicants.Count(x =>
                string.Equals(
                    x.Status,
                    "Under Review",
                    StringComparison.OrdinalIgnoreCase));


        public int ShortlistedCount =>
            Applicants.Count(x =>
                string.Equals(
                    x.Status,
                    "Shortlisted",
                    StringComparison.OrdinalIgnoreCase));


        public int InterviewCount =>
            Applicants.Count(x =>
                string.Equals(
                    x.Status,
                    "Interview",
                    StringComparison.OrdinalIgnoreCase));


        public int HiredCount =>
            Applicants.Count(x =>
                string.Equals(
                    x.Status,
                    "Hired",
                    StringComparison.OrdinalIgnoreCase));


        public int RejectedCount =>
            Applicants.Count(x =>
                string.Equals(
                    x.Status,
                    "Rejected",
                    StringComparison.OrdinalIgnoreCase));
    }


    // =============================================
    // COMPANY JOB OPTION
    // =============================================

    public class CompanyApplicationJobViewModel
    {
        public int JobId
        {
            get;
            set;
        }


        public string Title
        {
            get;
            set;
        } = string.Empty;


        public string Status
        {
            get;
            set;
        } = string.Empty;


        public DateTime CreatedAt
        {
            get;
            set;
        }


        public DateTime? ClosingDate
        {
            get;
            set;
        }


        public string? Location
        {
            get;
            set;
        }


        public string? JobType
        {
            get;
            set;
        }
    }


    // =============================================
    // APPLICANT LIST ITEM
    // =============================================

    public class CompanyApplicantItemViewModel
    {
        public int JobApplicationId
        {
            get;
            set;
        }


        public string Status
        {
            get;
            set;
        } = "Received";


        public DateTime AppliedAt
        {
            get;
            set;
        }


        public CompanyApplicantCandidateViewModel
            Candidate
        { get; set; }
            = new();


        public decimal? MatchScore
        {
            get;
            set;
        }


        // =========================================
        // DISPLAY HELPERS
        // =========================================

        public string AppliedDateText =>
            AppliedAt
                .ToLocalTime()
                .ToString(
                    "dd MMM yyyy");


        public string MatchScoreText =>
            MatchScore.HasValue
                ? $"{MatchScore.Value:0.#}%"
                : "Not calculated";


        public string ApplicantInitial =>
            string.IsNullOrWhiteSpace(
                Candidate.FullName)
                ? "A"
                : Candidate.FullName
                    .Substring(
                        0,
                        1)
                    .ToUpperInvariant();
    }


    // =============================================
    // CANDIDATE SUMMARY
    // =============================================

    public class CompanyApplicantCandidateViewModel
    {
        public int JobSeekerProfileId
        {
            get;
            set;
        }


        public string FullName
        {
            get;
            set;
        } = string.Empty;


        public string? ProfessionalSummary
        {
            get;
            set;
        }


        public string? CurrentLocation
        {
            get;
            set;
        }
    }


    // =============================================
    // API RESPONSE:
    // GET api/JobApplications/company/job/{jobId}
    // =============================================

    public class CompanyJobApplicantsApiResponse
    {
        public CompanyApplicationJobSummaryViewModel
            Job
        { get; set; }
            = new();


        public int TotalApplicants
        {
            get;
            set;
        }


        public List<CompanyApplicantItemViewModel>
            Applicants
        { get; set; }
            = new();
    }


    public class CompanyApplicationJobSummaryViewModel
    {
        public int JobId
        {
            get;
            set;
        }


        public string Title
        {
            get;
            set;
        } = string.Empty;
    }


    // =============================================
    // APPLICATION DETAILS PAGE
    // =============================================

    public class CompanyApplicationDetailsViewModel
    {
        public int JobApplicationId
        {
            get;
            set;
        }


        public string Status
        {
            get;
            set;
        } = string.Empty;


        public string? CoverLetter
        {
            get;
            set;
        }


        public DateTime AppliedAt
        {
            get;
            set;
        }


        public DateTime? UpdatedAt
        {
            get;
            set;
        }


        public CompanyApplicationDetailsJobViewModel
            Job
        { get; set; }
            = new();


        public CompanyApplicationCandidateDetailsViewModel
            Candidate
        { get; set; }
            = new();


        public List<CompanyApplicationSkillViewModel>
            Skills
        { get; set; }
            = new();


        public List<CompanyApplicationExperienceViewModel>
            Experience
        { get; set; }
            = new();


        public List<CompanyApplicationEducationViewModel>
            Education
        { get; set; }
            = new();


        public CompanyApplicationMatchViewModel?
            MatchResult
        { get; set; }


        public List<CompanyScreeningAnswerViewModel>
            ScreeningAnswers
        { get; set; }
            = new();


        public string? SuccessMessage
        {
            get;
            set;
        }


        public string? ErrorMessage
        {
            get;
            set;
        }


        public string AppliedDateText =>
            AppliedAt
                .ToLocalTime()
                .ToString(
                    "dd MMM yyyy, h:mm tt");


        public string UpdatedDateText =>
            UpdatedAt.HasValue
                ? UpdatedAt.Value
                    .ToLocalTime()
                    .ToString(
                        "dd MMM yyyy, h:mm tt")
                : "Not updated";
    }


    public class CompanyScreeningAnswerViewModel
    {
        public int JobScreeningQuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public string AnswerText { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }


    // =============================================
    // DETAILS - JOB
    // =============================================

    public class CompanyApplicationDetailsJobViewModel
    {
        public int JobId
        {
            get;
            set;
        }


        public string Title
        {
            get;
            set;
        } = string.Empty;
    }


    // =============================================
    // DETAILS - CANDIDATE
    // =============================================

    public class CompanyApplicationCandidateDetailsViewModel
    {
        public int JobSeekerProfileId
        {
            get;
            set;
        }


        public string FullName
        {
            get;
            set;
        } = string.Empty;


        public string Email
        {
            get;
            set;
        } = string.Empty;


        public string? ProfessionalSummary
        {
            get;
            set;
        }


        public string? PreferredJobCategory
        {
            get;
            set;
        }


        public string? CurrentLocation
        {
            get;
            set;
        }


        public decimal? ExpectedSalary
        {
            get;
            set;
        }
    }


    // =============================================
    // CANDIDATE SKILLS
    // =============================================

    public class CompanyApplicationSkillViewModel
    {
        public string SkillName
        {
            get;
            set;
        } = string.Empty;


        public string? ProficiencyLevel
        {
            get;
            set;
        }


        public decimal? YearsOfExperience
        {
            get;
            set;
        }
    }


    // =============================================
    // CANDIDATE EXPERIENCE
    // =============================================

    public class CompanyApplicationExperienceViewModel
    {
        public string JobTitle
        {
            get;
            set;
        } = string.Empty;


        public string CompanyName
        {
            get;
            set;
        } = string.Empty;


        public DateTime StartDate
        {
            get;
            set;
        }


        public DateTime? EndDate
        {
            get;
            set;
        }


        public bool IsCurrentJob
        {
            get;
            set;
        }


        public string PeriodText
        {
            get
            {
                var start =
                    StartDate.ToString(
                        "MMM yyyy");


                var end =
                    IsCurrentJob
                        ? "Present"
                        : EndDate.HasValue
                            ? EndDate.Value
                                .ToString(
                                    "MMM yyyy")
                            : "Unknown";


                return
                    $"{start} - {end}";
            }
        }
    }


    // =============================================
    // CANDIDATE EDUCATION
    // =============================================

    public class CompanyApplicationEducationViewModel
    {
        public string InstitutionName
        {
            get;
            set;
        } = string.Empty;


        public string Qualification
        {
            get;
            set;
        } = string.Empty;


        public string? FieldOfStudy
        {
            get;
            set;
        }
    }


    // =============================================
    // CV MATCH RESULT
    // =============================================

    public class CompanyApplicationMatchViewModel
    {
        public decimal OverallScore
        {
            get;
            set;
        }


        public decimal SkillsScore
        {
            get;
            set;
        }


        public decimal ExperienceScore
        {
            get;
            set;
        }


        public decimal EducationScore
        {
            get;
            set;
        }


        public decimal PreferredSkillsScore
        {
            get;
            set;
        }


        public decimal RelevanceScore
        {
            get;
            set;
        }


        public decimal LocationScore
        {
            get;
            set;
        }


        public string? MatchedSkills
        {
            get;
            set;
        }


        public string? MissingSkills
        {
            get;
            set;
        }


        public string? Recommendations
        {
            get;
            set;
        }


        public DateTime? CalculatedAt
        {
            get;
            set;
        }
    }


    // =============================================
    // UPDATE STATUS REQUEST
    // =============================================

    public class CompanyApplicationStatusRequest
    {
        public string Status
        {
            get;
            set;
        } = string.Empty;
    }
}