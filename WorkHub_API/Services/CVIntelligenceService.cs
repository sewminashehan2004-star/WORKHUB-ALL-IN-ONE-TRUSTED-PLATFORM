using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using WorkHub.API.Data;
using WorkHub.API.Interfaces;
using WorkHub.API.Models;

namespace WorkHub.API.Services
{
    public class CVIntelligenceService
        : ICVIntelligenceService
    {
        private readonly ApplicationDbContext _context;

        private readonly ICareerBenchmarkService
            _careerBenchmarkService;

        private readonly ICVTextExtractionService
            _textExtractionService;

        private readonly IWebHostEnvironment
            _environment;


        // =========================================
        // SCORE WEIGHTS
        // =========================================

        private const decimal SkillsMaximum = 35m;

        private const decimal ExperienceMaximum = 25m;

        private const decimal EducationMaximum = 15m;

        private const decimal CVQualityMaximum = 15m;

        private const decimal CareerRelevanceMaximum = 10m;


        public CVIntelligenceService(
            ApplicationDbContext context,
            ICareerBenchmarkService careerBenchmarkService,
            ICVTextExtractionService textExtractionService,
            IWebHostEnvironment environment)
        {
            _context = context;

            _careerBenchmarkService =
                careerBenchmarkService;

            _textExtractionService =
                textExtractionService;

            _environment =
                environment;
        }


        // =========================================
        // MAIN ANALYSIS
        // =========================================

        public async Task<CVIntelligenceResult?>
            AnalyseAsync(
                int userId,
                int careerRoleId)
        {
            // =====================================
            // JOB SEEKER PROFILE
            // =====================================

            var profile =
                await _context.JobSeekerProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);

            if (profile == null)
            {
                return null;
            }


            // =====================================
            // CAREER BENCHMARK
            // =====================================

            var benchmark =
                await _careerBenchmarkService
                    .BuildBenchmarkAsync(
                        careerRoleId);

            if (benchmark == null)
            {
                return null;
            }


            // =====================================
            // PRIMARY CV
            // =====================================

            var cvDocument =
                await _context.CVDocuments
                    .Where(x =>
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId)
                    .OrderByDescending(x =>
                        x.IsPrimary)
                    .ThenByDescending(x =>
                        x.UploadedAt)
                    .FirstOrDefaultAsync();

            if (cvDocument == null)
            {
                return null;
            }


            // =====================================
            // READ ACTUAL CV TEXT
            // =====================================

            var physicalPath =
                GetPhysicalCVPath(
                    cvDocument.FilePath);

            var cvText =
                await _textExtractionService
                    .ExtractTextAsync(
                        physicalPath);

            var normalizedCVText =
                cvText
                    .Trim()
                    .ToLowerInvariant();


            // =====================================
            // CANDIDATE SKILLS
            // =====================================

            var candidateSkills =
                await _context.JobSeekerSkills
                    .Include(x =>
                        x.Skill)
                    .Where(x =>
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId)
                    .ToListAsync();


            // =====================================
            // EXPERIENCE
            // =====================================

            var experiences =
                await _context
                    .JobSeekerExperiences
                    .Where(x =>
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId)
                    .ToListAsync();


            // =====================================
            // EDUCATION
            // =====================================

            var education =
                await _context
                    .JobSeekerEducations
                    .Where(x =>
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId)
                    .ToListAsync();


            // =====================================
            // RESULT
            // =====================================

            var result =
                new CVIntelligenceResult
                {
                    CareerRoleId =
                        benchmark.CareerRoleId,

                    RoleName =
                        benchmark.RoleName,

                    CategoryName =
                        benchmark.CategoryName,

                    CVDocumentId =
                        cvDocument.CVDocumentId,

                    CVFileName =
                        cvDocument.FileName,

                    GeneratedAt =
                        DateTime.UtcNow
                };


            // =====================================
            // 1. SKILLS = 35
            // =====================================

            var skillAnalysis =
                CalculateSkillScore(
                    benchmark,
                    candidateSkills,
                    normalizedCVText);

            result.SkillsScore =
                skillAnalysis.Score;

            result.Skills =
                skillAnalysis.Results;


            // =====================================
            // 2. EXPERIENCE = 25
            // =====================================

            var experienceAnalysis =
                CalculateExperienceScore(
                    benchmark,
                    experiences);

            result.ExperienceScore =
                experienceAnalysis.Score;

            result.CandidateExperienceYears =
                experienceAnalysis
                    .RelevantYears;

            result.RecommendedExperienceYears =
                benchmark
                    .AverageMinimumExperienceYears;

            result.ExperienceStatus =
                experienceAnalysis.Status;


            // =====================================
            // 3. EDUCATION = 15
            // =====================================

            var educationAnalysis =
                CalculateEducationScore(
                    benchmark,
                    education,
                    normalizedCVText);

            result.EducationScore =
                educationAnalysis.Score;

            result.EducationStatus =
                educationAnalysis.Status;

            result.EducationFindings =
                educationAnalysis.Findings;


            // =====================================
            // 4. CV QUALITY = 15
            // =====================================

            var cvQualityAnalysis =
                CalculateCVQuality(
                    cvText);

            result.CvQualityScore =
                cvQualityAnalysis.Score;

            result.CVStrengths =
                cvQualityAnalysis.Strengths;

            result.CVIssues =
                cvQualityAnalysis.Issues;


            // =====================================
            // 5. CAREER RELEVANCE = 10
            // =====================================

            result.CareerRelevanceScore =
                CalculateCareerRelevanceScore(
                    profile,
                    benchmark,
                    experiences,
                    normalizedCVText,
                    result.Skills);


            // =====================================
            // ROUND SCORES
            // =====================================

            result.SkillsScore =
                RoundScore(
                    result.SkillsScore,
                    SkillsMaximum);

            result.ExperienceScore =
                RoundScore(
                    result.ExperienceScore,
                    ExperienceMaximum);

            result.EducationScore =
                RoundScore(
                    result.EducationScore,
                    EducationMaximum);

            result.CvQualityScore =
                RoundScore(
                    result.CvQualityScore,
                    CVQualityMaximum);

            result.CareerRelevanceScore =
                RoundScore(
                    result.CareerRelevanceScore,
                    CareerRelevanceMaximum);


            // =====================================
            // TOTAL = 100
            // =====================================

            result.OverallScore =
                result.SkillsScore
                +
                result.ExperienceScore
                +
                result.EducationScore
                +
                result.CvQualityScore
                +
                result.CareerRelevanceScore;

            result.OverallScore =
                Math.Clamp(
                    result.OverallScore,
                    0m,
                    100m);

            result.OverallScore =
                Math.Round(
                    result.OverallScore,
                    2);


            // =====================================
            // READINESS LEVEL
            // =====================================

            result.ReadinessLevel =
                DetermineReadinessLevel(
                    result.OverallScore);


            // =====================================
            // RECOMMENDATIONS
            // =====================================

            result.Recommendations =
                BuildRecommendations(
                    result,
                    string.IsNullOrWhiteSpace(
                        cvText));


            return result;
        }


        // =========================================
        // SKILL ANALYSIS
        // =========================================

        private static SkillAnalysisOutput
            CalculateSkillScore(
                CareerBenchmarkResult benchmark,
                List<JobSeekerSkill> candidateSkills,
                string cvText)
        {
            var output =
                new SkillAnalysisOutput();


            var requiredSkills =
                benchmark.Skills
                    .Where(x =>
                        x.Importance.Equals(
                            "Required",
                            StringComparison
                                .OrdinalIgnoreCase))
                    .ToList();


            var preferredSkills =
                benchmark.Skills
                    .Where(x =>
                        x.Importance.Equals(
                            "Preferred",
                            StringComparison
                                .OrdinalIgnoreCase))
                    .ToList();


            // =====================================
            // DISTRIBUTE 35 POINTS
            // =====================================

            decimal requiredMaximum =
                28m;

            decimal preferredMaximum =
                7m;


            if (requiredSkills.Count == 0
                &&
                preferredSkills.Count > 0)
            {
                requiredMaximum =
                    0;

                preferredMaximum =
                    SkillsMaximum;
            }


            if (preferredSkills.Count == 0
                &&
                requiredSkills.Count > 0)
            {
                preferredMaximum =
                    0;

                requiredMaximum =
                    SkillsMaximum;
            }


            // =====================================
            // REQUIRED
            // =====================================

            var requiredResult =
                ScoreSkillGroup(
                    requiredSkills,
                    candidateSkills,
                    cvText,
                    requiredMaximum);


            output.Score +=
                requiredResult.Score;

            output.Results.AddRange(
                requiredResult.Results);


            // =====================================
            // PREFERRED
            // =====================================

            var preferredResult =
                ScoreSkillGroup(
                    preferredSkills,
                    candidateSkills,
                    cvText,
                    preferredMaximum);


            output.Score +=
                preferredResult.Score;

            output.Results.AddRange(
                preferredResult.Results);


            output.Score =
                Math.Clamp(
                    output.Score,
                    0m,
                    SkillsMaximum);


            output.Results =
                output.Results
                    .OrderBy(x =>
                        x.Importance ==
                        "Required"
                            ? 0
                            : 1)
                    .ThenBy(x =>
                        SkillStatusOrder(
                            x.MatchStatus))
                    .ThenBy(x =>
                        x.SkillName)
                    .ToList();


            return output;
        }


        // =========================================
        // SCORE ONE SKILL GROUP
        // =========================================

        private static SkillAnalysisOutput
            ScoreSkillGroup(
                List<CareerBenchmarkSkillResult>
                    benchmarkSkills,

                List<JobSeekerSkill>
                    candidateSkills,

                string cvText,

                decimal maximumPoints)
        {
            var output =
                new SkillAnalysisOutput();


            if (benchmarkSkills.Count == 0
                ||
                maximumPoints <= 0)
            {
                return output;
            }


            var pointsPerSkill =
                maximumPoints
                /
                benchmarkSkills.Count;


            foreach (var benchmarkSkill
                     in benchmarkSkills)
            {
                var candidateSkill =
                    candidateSkills
                        .FirstOrDefault(x =>
                            x.SkillId ==
                            benchmarkSkill.SkillId);


                var foundInProfile =
                    candidateSkill != null;


                var foundInCV =
                    ContainsPhrase(
                        cvText,
                        benchmarkSkill.SkillName);


                var candidateYears =
                    candidateSkill?
                        .YearsOfExperience;


                var recommendedYears =
                    benchmarkSkill
                        .AverageRequiredYears;


                string foundFrom;

                if (foundInProfile
                    &&
                    foundInCV)
                {
                    foundFrom =
                        "Profile + CV";
                }
                else if (foundInProfile)
                {
                    foundFrom =
                        "Profile";
                }
                else if (foundInCV)
                {
                    foundFrom =
                        "CV";
                }
                else
                {
                    foundFrom =
                        "Not Found";
                }


                string matchStatus;

                decimal creditRatio;


                // =================================
                // NOT FOUND
                // =================================

                if (!foundInProfile
                    &&
                    !foundInCV)
                {
                    matchStatus =
                        "Missing";

                    creditRatio =
                        0m;
                }

                // =================================
                // NO YEARS REQUIREMENT
                // =================================

                else if (
                    !recommendedYears.HasValue
                    ||
                    recommendedYears.Value <= 0)
                {
                    matchStatus =
                        "Matched";

                    creditRatio =
                        1m;
                }

                // =================================
                // PROFILE HAS YEARS
                // =================================

                else if (
                    foundInProfile
                    &&
                    candidateYears.HasValue)
                {
                    var required =
                        recommendedYears.Value;

                    var candidate =
                        Math.Max(
                            candidateYears.Value,
                            0m);


                    if (candidate >= required)
                    {
                        matchStatus =
                            "Matched";

                        creditRatio =
                            1m;
                    }
                    else
                    {
                        matchStatus =
                            "Partial";


                        var yearsRatio =
                            required > 0
                                ? candidate /
                                  required
                                : 1m;


                        yearsRatio =
                            Math.Clamp(
                                yearsRatio,
                                0m,
                                1m);


                        // Skill is known,
                        // but experience is below
                        // the benchmark.
                        //
                        // Minimum 50% credit for
                        // proven skill existence.

                        creditRatio =
                            0.50m
                            +
                            (
                                0.50m
                                *
                                yearsRatio
                            );
                    }
                }

                // =================================
                // PROFILE SKILL, YEARS UNKNOWN
                // =================================

                else if (foundInProfile)
                {
                    matchStatus =
                        "Partial";

                    creditRatio =
                        0.70m;
                }

                // =================================
                // CV ONLY
                // =================================

                else
                {
                    matchStatus =
                        "Partial";

                    // CV confirms the skill exists,
                    // but years cannot be verified
                    // from the structured profile.

                    creditRatio =
                        0.65m;
                }


                output.Score +=
                    pointsPerSkill
                    *
                    creditRatio;


                output.Results.Add(
                    new CVIntelligenceSkillResult
                    {
                        SkillId =
                            benchmarkSkill.SkillId,

                        SkillName =
                            benchmarkSkill.SkillName,

                        Importance =
                            benchmarkSkill.Importance,

                        MatchStatus =
                            matchStatus,

                        FoundFrom =
                            foundFrom,

                        CandidateYears =
                            candidateYears,

                        RecommendedYears =
                            recommendedYears,

                        DemandPercentage =
                            benchmarkSkill
                                .DemandPercentage,

                        Source =
                            benchmarkSkill.Source
                    });
            }


            return output;
        }


        // =========================================
        // EXPERIENCE ANALYSIS
        // =========================================

        private static ExperienceAnalysisOutput
            CalculateExperienceScore(
                CareerBenchmarkResult benchmark,
                List<JobSeekerExperience> experiences)
        {
            decimal totalRelevantYears =
                0m;


            foreach (var experience
                     in experiences)
            {
                if (!IsExperienceRelevant(
                        experience,
                        benchmark))
                {
                    continue;
                }


                totalRelevantYears +=
                    CalculateExperienceYears(
                        experience);
            }


            totalRelevantYears =
                Math.Round(
                    totalRelevantYears,
                    1);


            var requiredYears =
                benchmark
                    .AverageMinimumExperienceYears;


            // =====================================
            // NO EXPERIENCE REQUIREMENT
            // =====================================

            if (!requiredYears.HasValue
                ||
                requiredYears.Value <= 0)
            {
                return new ExperienceAnalysisOutput
                {
                    Score =
                        ExperienceMaximum,

                    RelevantYears =
                        totalRelevantYears,

                    Status =
                        "No Minimum Requirement"
                };
            }


            // =====================================
            // CALCULATE MATCH
            // =====================================

            var ratio =
                totalRelevantYears
                /
                requiredYears.Value;


            ratio =
                Math.Clamp(
                    ratio,
                    0m,
                    1m);


            var score =
                ratio
                *
                ExperienceMaximum;


            string status;


            if (totalRelevantYears >=
                requiredYears.Value)
            {
                status =
                    "Matched";
            }
            else if (totalRelevantYears > 0)
            {
                status =
                    "Partial";
            }
            else
            {
                status =
                    "Missing";
            }


            return new ExperienceAnalysisOutput
            {
                Score =
                    score,

                RelevantYears =
                    totalRelevantYears,

                Status =
                    status
            };
        }


        // =========================================
        // EXPERIENCE RELEVANCE
        // =========================================

        private static bool
            IsExperienceRelevant(
                JobSeekerExperience experience,
                CareerBenchmarkResult benchmark)
        {
            var text =
                $"{experience.JobTitle} " +
                $"{experience.Description}"
                    .ToLowerInvariant();


            // =====================================
            // ROLE NAME / ROLE KEYWORD
            // =====================================

            if (ContainsRoleSignal(
                    text,
                    benchmark.RoleName))
            {
                return true;
            }


            // =====================================
            // CATEGORY
            // =====================================

            if (ContainsPhrase(
                    text,
                    benchmark.CategoryName))
            {
                return true;
            }


            // =====================================
            // REQUIRED CAREER SKILLS
            // =====================================

            var requiredSkills =
                benchmark.Skills
                    .Where(x =>
                        x.Importance.Equals(
                            "Required",
                            StringComparison
                                .OrdinalIgnoreCase))
                    .Select(x =>
                        x.SkillName)
                    .ToList();


            var matchedSkills =
                requiredSkills.Count(skill =>
                    ContainsPhrase(
                        text,
                        skill));


            // At least two benchmark skills
            // makes the experience reasonably
            // relevant to the selected career.

            return matchedSkills >= 2;
        }


        // =========================================
        // EXPERIENCE YEARS
        // =========================================

        private static decimal
            CalculateExperienceYears(
                JobSeekerExperience experience)
        {
            var endDate =
                experience.IsCurrentJob
                    ? DateTime.UtcNow
                    : experience.EndDate;


            if (!endDate.HasValue)
            {
                return 0m;
            }


            if (endDate.Value <=
                experience.StartDate)
            {
                return 0m;
            }


            var days =
                (
                    endDate.Value
                    -
                    experience.StartDate
                )
                .TotalDays;


            return
                (decimal)
                (
                    days
                    /
                    365.25
                );
        }


        // =========================================
        // EDUCATION ANALYSIS
        // =========================================

        private static EducationAnalysisOutput
            CalculateEducationScore(
                CareerBenchmarkResult benchmark,

                List<JobSeekerEducation>
                    education,

                string cvText)
        {
            var output =
                new EducationAnalysisOutput();


            // =====================================
            // NO EDUCATION BENCHMARK
            // =====================================

            if (benchmark
                    .CommonEducationRequirements
                    .Count == 0)
            {
                output.Score =
                    EducationMaximum;

                output.Status =
                    "No Specific Requirement";

                output.Findings.Add(
                    "No specific education benchmark is currently defined for this role.");

                return output;
            }


            // =====================================
            // CANDIDATE EDUCATION TEXT
            // =====================================

            var profileEducationText =
                string.Join(
                    " ",
                    education.Select(x =>
                        $"{x.Qualification} " +
                        $"{x.FieldOfStudy} " +
                        $"{x.InstitutionName}"
                    ))
                .ToLowerInvariant();


            var combinedCandidateText =
                $"{profileEducationText} {cvText}"
                    .ToLowerInvariant();


            // =====================================
            // BENCHMARK KEYWORDS
            // =====================================

            var benchmarkText =
                string.Join(
                    " ",
                    benchmark
                        .CommonEducationRequirements);


            var keywords =
                GetEducationKeywords(
                    benchmarkText);


            // =====================================
            // BASE CREDIT FOR HAVING EDUCATION
            // =====================================

            decimal score =
                0m;


            if (education.Count > 0)
            {
                score +=
                    5m;

                output.Findings.Add(
                    $"{education.Count} education record(s) found in your Job Seeker profile.");
            }
            else
            {
                output.Findings.Add(
                    "No structured education records were found in your Job Seeker profile.");
            }


            // =====================================
            // KEYWORD RELEVANCE = UP TO 10
            // =====================================

            if (keywords.Count > 0)
            {
                var matchedKeywords =
                    keywords
                        .Where(x =>
                            combinedCandidateText
                                .Contains(
                                    x,
                                    StringComparison
                                        .OrdinalIgnoreCase))
                        .Distinct(
                            StringComparer
                                .OrdinalIgnoreCase)
                        .ToList();


                var ratio =
                    (decimal)
                    matchedKeywords.Count
                    /
                    keywords.Count;


                score +=
                    Math.Min(
                        ratio * 10m,
                        10m);


                if (matchedKeywords.Count > 0)
                {
                    output.Findings.Add(
                        "Relevant education terms found: "
                        +
                        string.Join(
                            ", ",
                            matchedKeywords
                                .Take(6))
                        +
                        ".");
                }
                else
                {
                    output.Findings.Add(
                        "No strong career-specific education terms were detected.");
                }
            }
            else if (education.Count > 0)
            {
                // If baseline language has no useful
                // keywords but structured education
                // exists, do not unfairly penalise it.

                score =
                    EducationMaximum;
            }


            score =
                Math.Clamp(
                    score,
                    0m,
                    EducationMaximum);


            output.Score =
                score;


            if (score >= 12m)
            {
                output.Status =
                    "Matched";
            }
            else if (score >= 5m)
            {
                output.Status =
                    "Partial";
            }
            else
            {
                output.Status =
                    "Missing";
            }


            return output;
        }


        // =========================================
        // CV QUALITY = 15
        // =========================================

        private static CVQualityAnalysisOutput
            CalculateCVQuality(
                string cvText)
        {
            var output =
                new CVQualityAnalysisOutput();


            if (string.IsNullOrWhiteSpace(
                    cvText))
            {
                output.Issues.Add(
                    "WorkHub could not extract readable text from the uploaded CV.");

                return output;
            }


            var lowerText =
                cvText.ToLowerInvariant();


            // =====================================
            // READABLE TEXT = 2
            // =====================================

            output.Score +=
                2m;

            output.Strengths.Add(
                "The uploaded CV contains readable text.");


            // =====================================
            // USEFUL CONTENT LENGTH = 2
            // =====================================

            if (cvText.Length >= 800)
            {
                output.Score +=
                    2m;

                output.Strengths.Add(
                    "The CV contains a useful amount of written content.");
            }
            else if (cvText.Length >= 300)
            {
                output.Score +=
                    1m;

                output.Issues.Add(
                    "The CV is relatively short and may need more detail.");
            }
            else
            {
                output.Issues.Add(
                    "The CV contains very little readable content.");
            }


            // =====================================
            // EMAIL = 1.5
            // =====================================

            if (ContainsEmail(
                    cvText))
            {
                output.Score +=
                    1.5m;

                output.Strengths.Add(
                    "An email address was detected.");
            }
            else
            {
                output.Issues.Add(
                    "No clear email address was detected in the CV.");
            }


            // =====================================
            // PHONE = 1.5
            // =====================================

            if (ContainsPhoneNumber(
                    cvText))
            {
                output.Score +=
                    1.5m;

                output.Strengths.Add(
                    "A phone number was detected.");
            }
            else
            {
                output.Issues.Add(
                    "No clear phone number was detected in the CV.");
            }


            // =====================================
            // EXPERIENCE SECTION = 2
            // =====================================

            if (ContainsAny(
                    lowerText,
                    "work experience",
                    "professional experience",
                    "employment history",
                    "employment",
                    "experience"))
            {
                output.Score +=
                    2m;

                output.Strengths.Add(
                    "A work experience section was detected.");
            }
            else
            {
                output.Issues.Add(
                    "Consider adding a clear Work Experience section.");
            }


            // =====================================
            // EDUCATION SECTION = 2
            // =====================================

            if (ContainsAny(
                    lowerText,
                    "education",
                    "qualifications",
                    "academic background",
                    "academic qualifications"))
            {
                output.Score +=
                    2m;

                output.Strengths.Add(
                    "An education or qualifications section was detected.");
            }
            else
            {
                output.Issues.Add(
                    "Consider adding a clear Education section.");
            }


            // =====================================
            // SKILLS SECTION = 2
            // =====================================

            if (ContainsAny(
                    lowerText,
                    "skills",
                    "technical skills",
                    "core skills",
                    "key skills",
                    "competencies"))
            {
                output.Score +=
                    2m;

                output.Strengths.Add(
                    "A skills section was detected.");
            }
            else
            {
                output.Issues.Add(
                    "Consider adding a clear Skills section.");
            }


            // =====================================
            // SUMMARY / PROFILE = 2
            // =====================================

            if (ContainsAny(
                    lowerText,
                    "professional summary",
                    "career summary",
                    "profile",
                    "personal profile",
                    "objective",
                    "career objective"))
            {
                output.Score +=
                    2m;

                output.Strengths.Add(
                    "A professional summary or profile section was detected.");
            }
            else
            {
                output.Issues.Add(
                    "Consider adding a short professional summary tailored to your target role.");
            }


            output.Score =
                Math.Clamp(
                    output.Score,
                    0m,
                    CVQualityMaximum);


            return output;
        }


        // =========================================
        // CAREER RELEVANCE = 10
        // =========================================

        private static decimal
            CalculateCareerRelevanceScore(
                JobSeekerProfile profile,
                CareerBenchmarkResult benchmark,

                List<JobSeekerExperience>
                    experiences,

                string cvText,

                List<CVIntelligenceSkillResult>
                    skillResults)
        {
            decimal score =
                0m;


            // =====================================
            // 1. PREFERRED CATEGORY = 2.5
            // =====================================

            if (!string.IsNullOrWhiteSpace(
                    profile.PreferredJobCategory))
            {
                var preferred =
                    profile
                        .PreferredJobCategory
                        .ToLowerInvariant();


                if (ContainsPhrase(
                        preferred,
                        benchmark.CategoryName)
                    ||
                    ContainsRoleSignal(
                        preferred,
                        benchmark.RoleName))
                {
                    score +=
                        2.5m;
                }
            }


            // =====================================
            // 2. PROFESSIONAL SUMMARY = 2.5
            // =====================================

            if (!string.IsNullOrWhiteSpace(
                    profile.ProfessionalSummary))
            {
                var summary =
                    profile
                        .ProfessionalSummary
                        .ToLowerInvariant();


                if (ContainsRoleSignal(
                        summary,
                        benchmark.RoleName)
                    ||
                    ContainsPhrase(
                        summary,
                        benchmark.CategoryName))
                {
                    score +=
                        2.5m;
                }
            }


            // =====================================
            // 3. CV RELEVANCE = 2.5
            // =====================================

            var requiredSkillMatches =
                skillResults.Count(x =>
                    x.Importance.Equals(
                        "Required",
                        StringComparison
                            .OrdinalIgnoreCase)
                    &&
                    x.MatchStatus !=
                    "Missing");


            if (ContainsRoleSignal(
                    cvText,
                    benchmark.RoleName)
                ||
                ContainsPhrase(
                    cvText,
                    benchmark.CategoryName)
                ||
                requiredSkillMatches >= 2)
            {
                score +=
                    2.5m;
            }


            // =====================================
            // 4. RELEVANT EXPERIENCE = 2.5
            // =====================================

            if (experiences.Any(x =>
                    IsExperienceRelevant(
                        x,
                        benchmark)))
            {
                score +=
                    2.5m;
            }


            return Math.Clamp(
                score,
                0m,
                CareerRelevanceMaximum);
        }


        // =========================================
        // RECOMMENDATIONS
        // =========================================

        private static List<string>
            BuildRecommendations(
                CVIntelligenceResult result,
                bool cvUnreadable)
        {
            var recommendations =
                new List<string>();


            // =====================================
            // OVERALL MESSAGE
            // =====================================

            if (result.OverallScore >= 85m)
            {
                recommendations.Add(
                    $"Your CV and profile show strong readiness for the {result.RoleName} career benchmark.");
            }
            else if (result.OverallScore >= 70m)
            {
                recommendations.Add(
                    $"Your CV and profile show good potential for {result.RoleName}, with some areas still worth improving.");
            }
            else if (result.OverallScore >= 50m)
            {
                recommendations.Add(
                    $"Your profile is developing toward {result.RoleName}. Focus on the missing requirements before relying on this CV for stronger applications.");
            }
            else
            {
                recommendations.Add(
                    $"Your current CV and profile have significant gaps against the {result.RoleName} benchmark.");
            }


            // =====================================
            // MISSING REQUIRED SKILLS
            // =====================================

            var missingRequired =
                result.Skills
                    .Where(x =>
                        x.Importance.Equals(
                            "Required",
                            StringComparison
                                .OrdinalIgnoreCase)
                        &&
                        x.MatchStatus ==
                        "Missing")
                    .Select(x =>
                        x.SkillName)
                    .Take(5)
                    .ToList();


            if (missingRequired.Count > 0)
            {
                recommendations.Add(
                    "Priority skills to develop or demonstrate: "
                    +
                    string.Join(
                        ", ",
                        missingRequired)
                    +
                    ".");
            }


            // =====================================
            // PARTIAL SKILLS
            // =====================================

            var partialSkills =
                result.Skills
                    .Where(x =>
                        x.MatchStatus ==
                        "Partial")
                    .Select(x =>
                        x.SkillName)
                    .Take(5)
                    .ToList();


            if (partialSkills.Count > 0)
            {
                recommendations.Add(
                    "These skills were detected but do not yet fully meet the benchmark or have insufficient recorded experience: "
                    +
                    string.Join(
                        ", ",
                        partialSkills)
                    +
                    ".");
            }


            // =====================================
            // EXPERIENCE
            // =====================================

            if (result.ExperienceScore <
                ExperienceMaximum)
            {
                recommendations.Add(
                    $"The current benchmark is approximately {FormatYears(result.RecommendedExperienceYears)} of relevant experience. Your recorded relevant experience is approximately {FormatYears(result.CandidateExperienceYears)}.");
            }


            // =====================================
            // EDUCATION
            // =====================================

            if (result.EducationScore <
                12m)
            {
                recommendations.Add(
                    "Review your education section and make relevant qualifications and fields of study clear.");
            }


            // =====================================
            // CV QUALITY
            // =====================================

            if (result.CvQualityScore <
                12m)
            {
                recommendations.Add(
                    "Improve the CV structure by using clear Summary, Skills, Experience and Education sections.");
            }


            // =====================================
            // CAREER RELEVANCE
            // =====================================

            if (result.CareerRelevanceScore <
                8m)
            {
                recommendations.Add(
                    $"Tailor the professional summary and CV wording more clearly toward {result.RoleName}.");
            }


            // =====================================
            // UNREADABLE CV
            // =====================================

            if (cvUnreadable)
            {
                recommendations.Add(
                    "WorkHub could not read the uploaded CV text. Use a text-based PDF or DOCX file instead of a scanned image-only document.");
            }


            return recommendations
                .Distinct(
                    StringComparer
                        .OrdinalIgnoreCase)
                .ToList();
        }


        // =========================================
        // READINESS LEVEL
        // =========================================

        private static string
            DetermineReadinessLevel(
                decimal score)
        {
            if (score >= 85m)
            {
                return
                    "Strong Readiness";
            }


            if (score >= 70m)
            {
                return
                    "Good Readiness";
            }


            if (score >= 50m)
            {
                return
                    "Developing Readiness";
            }


            return
                "Low Readiness";
        }


        // =========================================
        // CV PHYSICAL PATH
        // =========================================

        private string
            GetPhysicalCVPath(
                string storedPath)
        {
            if (Path.IsPathRooted(
                    storedPath))
            {
                return storedPath;
            }


            return Path.Combine(
                _environment.ContentRootPath,
                storedPath);
        }


        // =========================================
        // CHECK PHRASE
        // =========================================

        private static bool
            ContainsPhrase(
                string text,
                string? phrase)
        {
            if (string.IsNullOrWhiteSpace(
                    text)
                ||
                string.IsNullOrWhiteSpace(
                    phrase))
            {
                return false;
            }


            return text.Contains(
                phrase.Trim(),
                StringComparison
                    .OrdinalIgnoreCase);
        }


        // =========================================
        // ROLE SIGNAL
        // =========================================

        private static bool
            ContainsRoleSignal(
                string text,
                string roleName)
        {
            if (string.IsNullOrWhiteSpace(
                    text)
                ||
                string.IsNullOrWhiteSpace(
                    roleName))
            {
                return false;
            }


            if (ContainsPhrase(
                    text,
                    roleName))
            {
                return true;
            }


            var words =
                roleName
                    .ToLowerInvariant()
                    .Split(
                        new[]
                        {
                            ' ',
                            '-',
                            '/',
                            '&',
                            ','
                        },
                        StringSplitOptions
                            .RemoveEmptyEntries)
                    .Select(x =>
                        x.Trim())
                    .Where(x =>
                        x.Length >= 2)
                    .ToList();


            var specificWords =
                words
                    .Where(x =>
                        !GenericRoleWords
                            .Contains(x))
                    .ToList();


            if (specificWords.Count > 0)
            {
                return specificWords.Any(
                    word =>
                        ContainsPhrase(
                            text,
                            word));
            }


            return words.Any(
                word =>
                    ContainsPhrase(
                        text,
                        word));
        }


        // =========================================
        // EDUCATION KEYWORDS
        // =========================================

        private static List<string>
            GetEducationKeywords(
                string requirement)
        {
            return requirement
                .ToLowerInvariant()
                .Split(
                    new[]
                    {
                        ' ',
                        ',',
                        '.',
                        '/',
                        '-',
                        '(',
                        ')',
                        ':',
                        ';'
                    },
                    StringSplitOptions
                        .RemoveEmptyEntries)
                .Select(x =>
                    x.Trim())
                .Where(x =>
                    x.Length >= 3)
                .Where(x =>
                    !IgnoredEducationWords
                        .Contains(x))
                .Distinct(
                    StringComparer
                        .OrdinalIgnoreCase)
                .ToList();
        }


        // =========================================
        // EMAIL
        // =========================================

        private static bool
            ContainsEmail(
                string text)
        {
            return Regex.IsMatch(
                text,
                @"\b[A-Z0-9._%+\-]+@[A-Z0-9.\-]+\.[A-Z]{2,}\b",
                RegexOptions.IgnoreCase);
        }


        // =========================================
        // PHONE
        // =========================================

        private static bool
            ContainsPhoneNumber(
                string text)
        {
            return Regex.IsMatch(
                text,
                @"(?<!\d)(?:\+?\d[\d\s().\-]{7,}\d)(?!\d)");
        }


        // =========================================
        // CONTAINS ANY
        // =========================================

        private static bool
            ContainsAny(
                string text,
                params string[] phrases)
        {
            return phrases.Any(
                phrase =>
                    ContainsPhrase(
                        text,
                        phrase));
        }


        // =========================================
        // SCORE ROUNDING
        // =========================================

        private static decimal
            RoundScore(
                decimal score,
                decimal maximum)
        {
            score =
                Math.Clamp(
                    score,
                    0m,
                    maximum);


            return Math.Round(
                score,
                2);
        }


        // =========================================
        // SKILL STATUS ORDER
        // =========================================

        private static int
            SkillStatusOrder(
                string status)
        {
            return status switch
            {
                "Missing" => 0,

                "Partial" => 1,

                "Matched" => 2,

                _ => 3
            };
        }


        // =========================================
        // FORMAT YEARS
        // =========================================

        private static string
            FormatYears(
                decimal? years)
        {
            if (!years.HasValue)
            {
                return
                    "not specified";
            }


            return
                $"{Math.Round(years.Value, 1):0.#} year(s)";
        }


        // =========================================
        // GENERIC ROLE WORDS
        // =========================================

        private static readonly HashSet<string>
            GenericRoleWords =
                new(
                    StringComparer
                        .OrdinalIgnoreCase)
                {
                    "engineer",
                    "developer",
                    "specialist",
                    "officer",
                    "manager",
                    "assistant",
                    "coordinator",
                    "analyst",
                    "technician",
                    "administrator",
                    "worker",
                    "consultant",
                    "supervisor",
                    "executive",
                    "representative"
                };


        // =========================================
        // EDUCATION STOP WORDS
        // =========================================

        private static readonly HashSet<string>
            IgnoredEducationWords =
                new(
                    StringComparer
                        .OrdinalIgnoreCase)
                {
                    "the",
                    "and",
                    "for",
                    "with",
                    "related",
                    "field",
                    "relevant",
                    "qualification",
                    "qualifications",
                    "required",
                    "preferred",
                    "from",
                    "have",
                    "must",
                    "should",
                    "minimum",
                    "equivalent",
                    "professional",
                    "requirements",
                    "requirement",
                    "where",
                    "applicable",
                    "practical",
                    "experience"
                };


        // =========================================
        // INTERNAL OUTPUT TYPES
        // =========================================

        private sealed class
            SkillAnalysisOutput
        {
            public decimal Score
            {
                get;
                set;
            }


            public List<
                CVIntelligenceSkillResult>
                Results
            {
                get;
                set;
            } = new();
        }


        private sealed class
            ExperienceAnalysisOutput
        {
            public decimal Score
            {
                get;
                set;
            }


            public decimal RelevantYears
            {
                get;
                set;
            }


            public string Status
            {
                get;
                set;
            } = string.Empty;
        }


        private sealed class
            EducationAnalysisOutput
        {
            public decimal Score
            {
                get;
                set;
            }


            public string Status
            {
                get;
                set;
            } = string.Empty;


            public List<string>
                Findings
            {
                get;
                set;
            } = new();
        }


        private sealed class
            CVQualityAnalysisOutput
        {
            public decimal Score
            {
                get;
                set;
            }


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
}