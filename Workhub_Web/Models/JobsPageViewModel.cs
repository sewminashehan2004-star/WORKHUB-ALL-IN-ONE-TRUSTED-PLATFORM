namespace Workhub_Web.Models
{
    public class JobsPageViewModel
    {
        // =========================================
        // FILTER
        // =========================================

        public int? SelectedCareerCategoryId
        {
            get;
            set;
        }


        public int? SelectedCareerRoleId
        {
            get;
            set;
        }


        public string? Search
        {
            get;
            set;
        }


        // =========================================
        // MASTER DATA
        // =========================================

        public List<JobsCareerCategoryViewModel>
            Categories
        {
            get;
            set;
        } = new();


        public List<JobsCareerRoleViewModel>
            Roles
        {
            get;
            set;
        } = new();


        // =========================================
        // JOBS
        // =========================================

        public List<JobsListItemViewModel>
            Jobs
        {
            get;
            set;
        } = new();


        // =========================================
        // MESSAGE
        // =========================================

        public string? ErrorMessage
        {
            get;
            set;
        }
    }


    // =============================================
    // CAREER CATEGORY
    // =============================================

    public class JobsCareerCategoryViewModel
    {
        public int CareerCategoryId
        {
            get;
            set;
        }


        public string CategoryName
        {
            get;
            set;
        } = string.Empty;


        public string? Description
        {
            get;
            set;
        }
    }


    // =============================================
    // CAREER ROLE
    // =============================================

    public class JobsCareerRoleViewModel
    {
        public int CareerRoleId
        {
            get;
            set;
        }


        public int CareerCategoryId
        {
            get;
            set;
        }


        public string RoleName
        {
            get;
            set;
        } = string.Empty;


        public string? Description
        {
            get;
            set;
        }
    }


    // =============================================
    // ROLES API RESPONSE
    // =============================================

    public class JobsCareerRolesApiResponse
    {
        public JobsCareerCategorySummaryViewModel
            Category
        {
            get;
            set;
        } = new();


        public List<JobsCareerRoleViewModel>
            Roles
        {
            get;
            set;
        } = new();
    }


    public class JobsCareerCategorySummaryViewModel
    {
        public int CareerCategoryId
        {
            get;
            set;
        }


        public string CategoryName
        {
            get;
            set;
        } = string.Empty;
    }


    // =============================================
    // JOB
    // =============================================

    public class JobsListItemViewModel
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


        // =========================================
        // JOB COVER IMAGE
        // =========================================

        public string? ImagePath
        {
            get;
            set;
        }


        // =========================================
        // OLD CATEGORY FIELD
        // =========================================

        public string? Category
        {
            get;
            set;
        }


        // =========================================
        // CAREER CLASSIFICATION
        // =========================================

        public int? CareerRoleId
        {
            get;
            set;
        }


        public string? CareerRoleName
        {
            get;
            set;
        }


        public int? CareerCategoryId
        {
            get;
            set;
        }


        public string? CareerCategoryName
        {
            get;
            set;
        }


        // =========================================
        // JOB INFORMATION
        // =========================================

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


        public decimal? SalaryMin
        {
            get;
            set;
        }


        public decimal? SalaryMax
        {
            get;
            set;
        }


        public decimal? MinimumExperienceYears
        {
            get;
            set;
        }


        public DateTime? ClosingDate
        {
            get;
            set;
        }


        public DateTime CreatedAt
        {
            get;
            set;
        }


        public JobsCompanySummaryViewModel
            Company
        {
            get;
            set;
        } = new();


        // =========================================
        // DISPLAY HELPERS
        // =========================================

        public string DisplayCategory
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(
                        CareerCategoryName))
                {
                    return CareerCategoryName;
                }


                if (!string.IsNullOrWhiteSpace(
                        Category))
                {
                    return Category;
                }


                return "General";
            }
        }


        public string DisplayRole
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(
                        CareerRoleName))
                {
                    return CareerRoleName;
                }


                return Title;
            }
        }


        public string LocationText
        {
            get
            {
                return string.IsNullOrWhiteSpace(
                        Location)
                    ? "Location not specified"
                    : Location;
            }
        }


        public string JobTypeText
        {
            get
            {
                return string.IsNullOrWhiteSpace(
                        JobType)
                    ? "Job type not specified"
                    : JobType;
            }
        }


        public string SalaryText
        {
            get
            {
                if (SalaryMin.HasValue &&
                    SalaryMax.HasValue)
                {
                    return
                        $"${SalaryMin.Value:N0} - ${SalaryMax.Value:N0}";
                }


                if (SalaryMin.HasValue)
                {
                    return
                        $"From ${SalaryMin.Value:N0}";
                }


                if (SalaryMax.HasValue)
                {
                    return
                        $"Up to ${SalaryMax.Value:N0}";
                }


                return
                    "Salary not listed";
            }
        }


        public string ExperienceText
        {
            get
            {
                if (!MinimumExperienceYears.HasValue)
                {
                    return
                        "Experience not specified";
                }


                return
                    $"{MinimumExperienceYears.Value:0.#}+ years experience";
            }
        }


        public string PostedText
        {
            get
            {
                if (CreatedAt == default)
                {
                    return "Recently";
                }


                var local =
                    CreatedAt.ToLocalTime();


                return local.ToString(
                    "dd MMM yyyy");
            }
        }


        public string ClosingText
        {
            get
            {
                if (!ClosingDate.HasValue)
                {
                    return
                        "Open until filled";
                }


                return
                    ClosingDate.Value
                        .ToLocalTime()
                        .ToString(
                            "dd MMM yyyy");
            }
        }


        // =========================================
        // NEW JOB
        //
        // Jobs created in the last 7 days
        // can display a NEW badge.
        // =========================================

        public bool IsNew
        {
            get
            {
                if (CreatedAt == default)
                {
                    return false;
                }


                return
                    CreatedAt >=
                    DateTime.UtcNow.AddDays(-7);
            }
        }
    }


    // =============================================
    // COMPANY
    // =============================================

    public class JobsCompanySummaryViewModel
    {
        public int CompanyProfileId
        {
            get;
            set;
        }


        public string CompanyName
        {
            get;
            set;
        } = string.Empty;


        public string? VerificationStatus
        {
            get;
            set;
        }
    }
}