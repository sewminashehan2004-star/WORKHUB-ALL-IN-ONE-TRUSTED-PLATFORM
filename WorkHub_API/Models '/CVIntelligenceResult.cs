namespace WorkHub.API.Models
{
    public class CVIntelligenceResult
    {
        public int CareerRoleId { get; set; }

        public string RoleName { get; set; }
            = string.Empty;

        public string CategoryName { get; set; }
            = string.Empty;


        // =========================================
        // TOTAL SCORE
        // =========================================

        public decimal OverallScore { get; set; }

        public string ReadinessLevel { get; set; }
            = string.Empty;


        // =========================================
        // SCORE BREAKDOWN
        // =========================================

        public decimal SkillsScore { get; set; }

        public decimal ExperienceScore { get; set; }

        public decimal EducationScore { get; set; }

        public decimal CvQualityScore { get; set; }

        public decimal CareerRelevanceScore { get; set; }


        // =========================================
        // CV INFORMATION
        // =========================================

        public int CVDocumentId { get; set; }

        public string CVFileName { get; set; }
            = string.Empty;


        // =========================================
        // SKILL ANALYSIS
        // =========================================

        public List<CVIntelligenceSkillResult>
            Skills
        {
            get;
            set;
        } = new();


        // =========================================
        // EXPERIENCE
        // =========================================

        public decimal CandidateExperienceYears
        {
            get;
            set;
        }

        public decimal? RecommendedExperienceYears
        {
            get;
            set;
        }

        public string ExperienceStatus
        {
            get;
            set;
        } = string.Empty;


        // =========================================
        // EDUCATION
        // =========================================

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


        // =========================================
        // CV QUALITY
        // =========================================

        public List<string>
            CVStrengths
        {
            get;
            set;
        } = new();

        public List<string>
            CVIssues
        {
            get;
            set;
        } = new();


        // =========================================
        // RECOMMENDATIONS
        // =========================================

        public List<string>
            Recommendations
        {
            get;
            set;
        } = new();


        public DateTime GeneratedAt { get; set; }
            = DateTime.UtcNow;
    }


    public class CVIntelligenceSkillResult
    {
        public int SkillId { get; set; }

        public string SkillName { get; set; }
            = string.Empty;

        public string Importance { get; set; }
            = string.Empty;


        // Matched / Partial / Missing

        public string MatchStatus { get; set; }
            = string.Empty;


        // Profile / CV / Profile + CV / Not Found

        public string FoundFrom { get; set; }
            = string.Empty;


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
}