namespace Workhub_Web.Models
{
    public class CareerIntelligenceViewModel
    {
        public int SelectedCareerCategoryId
        {
            get;
            set;
        }

        public int SelectedCareerRoleId
        {
            get;
            set;
        }

        public List<CareerCategoryOptionViewModel>
            Categories
        {
            get;
            set;
        } = new();

        public List<CareerRoleOptionViewModel>
            Roles
        {
            get;
            set;
        } = new();

        public CareerBenchmarkViewModel?
            Benchmark
        {
            get;
            set;
        }

        public CVIntelligenceApiResponse?
            CVAnalysis
        {
            get;
            set;
        }

        public string? ErrorMessage
        {
            get;
            set;
        }

        public string? SuccessMessage
        {
            get;
            set;
        }
    }


    // =========================================
    // CAREER CATEGORY
    // =========================================

    public class CareerCategoryOptionViewModel
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


    // =========================================
    // CAREER ROLE
    // =========================================

    public class CareerRoleOptionViewModel
    {
        public int CareerRoleId
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

        public decimal? BaselineExperienceYears
        {
            get;
            set;
        }

        public string?
            BaselineEducationRequirement
        {
            get;
            set;
        }
    }


    // =========================================
    // ROLES API RESPONSE
    // =========================================

    public class CareerRolesApiResponse
    {
        public CareerCategorySummaryViewModel
            Category
        {
            get;
            set;
        } = new();

        public List<CareerRoleOptionViewModel>
            Roles
        {
            get;
            set;
        } = new();
    }


    public class CareerCategorySummaryViewModel
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


    // =========================================
    // CAREER BENCHMARK
    // =========================================

    public class CareerBenchmarkViewModel
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

        public string CategoryName
        {
            get;
            set;
        } = string.Empty;

        public CareerMarketViewModel
            Market
        {
            get;
            set;
        } = new();

        public CareerRequirementsViewModel
            Requirements
        {
            get;
            set;
        } = new();

        public DateTime GeneratedAt
        {
            get;
            set;
        }
    }


    public class CareerMarketViewModel
    {
        public int ActiveJobCount
        {
            get;
            set;
        }

        public string DataSource
        {
            get;
            set;
        } = string.Empty;

        public string ConfidenceLevel
        {
            get;
            set;
        } = string.Empty;
    }


    public class CareerRequirementsViewModel
    {
        public decimal?
            AverageMinimumExperienceYears
        {
            get;
            set;
        }

        public List<string>
            Education
        {
            get;
            set;
        } = new();

        public List<CareerBenchmarkSkillViewModel>
            Skills
        {
            get;
            set;
        } = new();
    }


    public class CareerBenchmarkSkillViewModel
    {
        public int SkillId
        {
            get;
            set;
        }

        public string SkillName
        {
            get;
            set;
        } = string.Empty;

        public string? SkillCategory
        {
            get;
            set;
        }

        public string Importance
        {
            get;
            set;
        } = string.Empty;

        public decimal DemandPercentage
        {
            get;
            set;
        }

        public int RequiredJobCount
        {
            get;
            set;
        }

        public int PreferredJobCount
        {
            get;
            set;
        }

        public decimal?
            AverageRequiredYears
        {
            get;
            set;
        }

        public string Source
        {
            get;
            set;
        } = string.Empty;
    }


    // =========================================
    // CV INTELLIGENCE API RESPONSE
    // =========================================

    public class CVIntelligenceApiResponse
    {
        public string Message
        {
            get;
            set;
        } = string.Empty;

        public CVIntelligenceCareerViewModel
            Career
        {
            get;
            set;
        } = new();

        public CVIntelligenceDocumentViewModel
            CV
        {
            get;
            set;
        } = new();

        public CVReadinessViewModel
            Readiness
        {
            get;
            set;
        } = new();

        public CVScoreBreakdownViewModel
            ScoreBreakdown
        {
            get;
            set;
        } = new();

        public CVSkillGroupsViewModel
            Skills
        {
            get;
            set;
        } = new();

        public CVExperienceAnalysisViewModel
            Experience
        {
            get;
            set;
        } = new();

        public CVEducationAnalysisViewModel
            Education
        {
            get;
            set;
        } = new();

        public CVQualityAnalysisViewModel
            CVQuality
        {
            get;
            set;
        } = new();

        public List<string>
            Recommendations
        {
            get;
            set;
        } = new();

        public DateTime GeneratedAt
        {
            get;
            set;
        }
    }


    // =========================================
    // CAREER
    // =========================================

    public class CVIntelligenceCareerViewModel
    {
        public int CareerRoleId
        {
            get;
            set;
        }

        public string RoleName
        {
            get;
            set;
        } = string.Empty;

        public string CategoryName
        {
            get;
            set;
        } = string.Empty;
    }


    // =========================================
    // CV DOCUMENT
    // =========================================

    public class CVIntelligenceDocumentViewModel
    {
        public int CVDocumentId
        {
            get;
            set;
        }

        public string CVFileName
        {
            get;
            set;
        } = string.Empty;
    }


    // =========================================
    // READINESS
    // =========================================

    public class CVReadinessViewModel
    {
        public decimal OverallScore
        {
            get;
            set;
        }

        public string ReadinessLevel
        {
            get;
            set;
        } = string.Empty;
    }


    // =========================================
    // SCORE BREAKDOWN
    // =========================================

    public class CVScoreBreakdownViewModel
    {
        public CVScoreItemViewModel
            Skills
        {
            get;
            set;
        } = new();

        public CVScoreItemViewModel
            Experience
        {
            get;
            set;
        } = new();

        public CVScoreItemViewModel
            Education
        {
            get;
            set;
        } = new();

        public CVScoreItemViewModel
            CVQuality
        {
            get;
            set;
        } = new();

        public CVScoreItemViewModel
            CareerRelevance
        {
            get;
            set;
        } = new();
    }


    public class CVScoreItemViewModel
    {
        public decimal Score
        {
            get;
            set;
        }

        public decimal Maximum
        {
            get;
            set;
        }
    }


    // =========================================
    // SKILLS
    // =========================================

    public class CVSkillGroupsViewModel
    {
        public List<CVSkillAnalysisViewModel>
            Matched
        {
            get;
            set;
        } = new();

        public List<CVSkillAnalysisViewModel>
            Partial
        {
            get;
            set;
        } = new();

        public List<CVSkillAnalysisViewModel>
            Missing
        {
            get;
            set;
        } = new();
    }


    public class CVSkillAnalysisViewModel
    {
        public int SkillId
        {
            get;
            set;
        }

        public string SkillName
        {
            get;
            set;
        } = string.Empty;

        public string Importance
        {
            get;
            set;
        } = string.Empty;

        public string MatchStatus
        {
            get;
            set;
        } = string.Empty;

        public string FoundFrom
        {
            get;
            set;
        } = string.Empty;

        public decimal? CandidateYears
        {
            get;
            set;
        }

        public decimal? RecommendedYears
        {
            get;
            set;
        }

        public decimal DemandPercentage
        {
            get;
            set;
        }

        public string Source
        {
            get;
            set;
        } = string.Empty;
    }


    // =========================================
    // EXPERIENCE
    // =========================================

    public class CVExperienceAnalysisViewModel
    {
        public decimal CandidateExperienceYears
        {
            get;
            set;
        }

        public decimal?
            RecommendedExperienceYears
        {
            get;
            set;
        }

        public string ExperienceStatus
        {
            get;
            set;
        } = string.Empty;
    }


    // =========================================
    // EDUCATION
    // =========================================

    public class CVEducationAnalysisViewModel
    {
        public string EducationStatus
        {
            get;
            set;
        } = string.Empty;

        public List<string>
            EducationFindings
        {
            get;
            set;
        } = new();
    }


    // =========================================
    // CV QUALITY
    // =========================================

    public class CVQualityAnalysisViewModel
    {
        public List<string>
            Strengths
        {
            get;
            set;
        } = new();

        public List<string>
            Issues
        {
            get;
            set;
        } = new();
    }
}