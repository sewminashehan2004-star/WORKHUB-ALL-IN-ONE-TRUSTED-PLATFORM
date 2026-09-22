namespace WorkHub.API.Models
{
    public class CareerBenchmarkResult
    {
        public int CareerRoleId { get; set; }

        public int CareerCategoryId { get; set; }

        public string RoleName { get; set; }
            = string.Empty;

        public string CategoryName { get; set; }
            = string.Empty;


        // =========================================
        // MARKET INFORMATION
        // =========================================

        public int ActiveJobCount { get; set; }

        public string DataSource { get; set; }
            = string.Empty;

        public string ConfidenceLevel { get; set; }
            = string.Empty;


        // =========================================
        // EXPERIENCE
        // =========================================

        public decimal? AverageMinimumExperienceYears
        {
            get;
            set;
        }


        // =========================================
        // EDUCATION
        // =========================================

        public List<string> CommonEducationRequirements
        {
            get;
            set;
        } = new();


        // =========================================
        // SKILLS
        // =========================================

        public List<CareerBenchmarkSkillResult> Skills
        {
            get;
            set;
        } = new();


        public DateTime GeneratedAt { get; set; }
            = DateTime.UtcNow;
    }


    public class CareerBenchmarkSkillResult
    {
        public int SkillId { get; set; }

        public string SkillName { get; set; }
            = string.Empty;

        public string? SkillCategory { get; set; }


        // Required / Preferred

        public string Importance { get; set; }
            = string.Empty;


        // Example:
        // 75 = requested by 75% of active jobs

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


        public decimal? AverageRequiredYears
        {
            get;
            set;
        }


        // Live Market
        // Baseline
        // Baseline + Live Market

        public string Source { get; set; }
            = string.Empty;
    }
}