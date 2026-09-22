using Microsoft.EntityFrameworkCore;
using WorkHub.API.Data;
using WorkHub.API.Interfaces;
using WorkHub.API.Models;

namespace WorkHub.API.Services
{
    public class CVMatchingService : ICVMatchingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICVTextExtractionService _textExtractionService;
        private readonly IWebHostEnvironment _environment;

        public CVMatchingService(
            ApplicationDbContext context,
            ICVTextExtractionService textExtractionService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _textExtractionService = textExtractionService;
            _environment = environment;
        }

        public async Task<CVMatchResult?> CalculateAndSaveAsync(
            int applicationId)
        {
            var application =
                await _context.JobApplications
                    .Include(x => x.Job)
                    .Include(x => x.JobSeekerProfile)
                    .FirstOrDefaultAsync(x =>
                        x.JobApplicationId == applicationId);

            if (application == null)
            {
                return null;
            }

            var job = application.Job;
            var profile = application.JobSeekerProfile;

            // =====================================
            // READ ACTUAL UPLOADED CV TEXT
            // =====================================

            string cvText = string.Empty;

            if (application.CVDocumentId.HasValue)
            {
                var cvDocument =
                    await _context.CVDocuments
                        .FirstOrDefaultAsync(x =>
                            x.CVDocumentId ==
                            application.CVDocumentId.Value);

                if (cvDocument != null)
                {
                    var physicalPath =
                        Path.Combine(
                            _environment.ContentRootPath,
                            cvDocument.FilePath);

                    cvText =
                        await _textExtractionService
                            .ExtractTextAsync(physicalPath);
                }
            }

            var normalizedCVText =
                cvText.ToLowerInvariant();

            // =====================================
            // GET JOB SKILLS
            // =====================================

            var jobSkills =
                await _context.JobSkills
                    .Include(x => x.Skill)
                    .Where(x =>
                        x.JobId == job.JobId)
                    .ToListAsync();

            // =====================================
            // GET CANDIDATE SKILLS
            // =====================================

            var candidateSkills =
                await _context.JobSeekerSkills
                    .Include(x => x.Skill)
                    .Where(x =>
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId)
                    .ToListAsync();

            // =====================================
            // GET EXPERIENCE
            // =====================================

            var experiences =
                await _context.JobSeekerExperiences
                    .Where(x =>
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId)
                    .ToListAsync();

            // =====================================
            // GET EDUCATION
            // =====================================

            var education =
                await _context.JobSeekerEducations
                    .Where(x =>
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId)
                    .ToListAsync();

            // =====================================
            // 1. REQUIRED SKILLS = 45
            // =====================================

            var requiredSkills =
                jobSkills
                    .Where(x =>
                        x.Importance.Equals(
                            "Required",
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

            decimal skillsScore = 0;

            var matchedSkillNames =
                new List<string>();

            var missingSkillNames =
                new List<string>();

            if (requiredSkills.Count == 0)
            {
                skillsScore = 45;
            }
            else
            {
                decimal pointsPerSkill =
                    45m / requiredSkills.Count;

                foreach (var required in requiredSkills)
                {
                    var candidate =
                        candidateSkills
                            .FirstOrDefault(x =>
                                x.SkillId ==
                                required.SkillId);

                    // Skill is not in profile
                    // Check actual uploaded CV text
                    if (candidate == null)
                    {
                        var skillInCV =
                            ContainsPhrase(
                                normalizedCVText,
                                required.Skill.SkillName);

                        if (skillInCV)
                        {
                            matchedSkillNames.Add(
                                required.Skill.SkillName);

                            // CV proves skill exists,
                            // but years are unknown
                            skillsScore +=
                                pointsPerSkill * 0.70m;

                            continue;
                        }

                        missingSkillNames.Add(
                            required.Skill.SkillName);

                        continue;
                    }

                    matchedSkillNames.Add(
                        required.Skill.SkillName);

                    if (required.RequiredYearsOfExperience.HasValue &&
                        required.RequiredYearsOfExperience.Value > 0)
                    {
                        var candidateYears =
                            candidate.YearsOfExperience ?? 0;

                        var requiredYears =
                            required.RequiredYearsOfExperience.Value;

                        var ratio =
                            candidateYears / requiredYears;

                        ratio =
                            Math.Min(ratio, 1m);

                        skillsScore +=
                            pointsPerSkill * ratio;

                        if (candidateYears < requiredYears)
                        {
                            missingSkillNames.Add(
                                $"{required.Skill.SkillName} " +
                                $"(need {requiredYears} years, " +
                                $"have {candidateYears} years)");
                        }
                    }
                    else
                    {
                        skillsScore +=
                            pointsPerSkill;
                    }
                }
            }

            // =====================================
            // 2. EXPERIENCE = 20
            // =====================================

            decimal totalExperienceYears = 0;

            foreach (var experience in experiences)
            {
                var endDate =
                    experience.IsCurrentJob
                        ? DateTime.UtcNow
                        : experience.EndDate;

                if (endDate.HasValue &&
                    endDate.Value > experience.StartDate)
                {
                    var days =
                        (endDate.Value -
                         experience.StartDate)
                        .TotalDays;

                    totalExperienceYears +=
                        (decimal)(days / 365.25);
                }
            }

            decimal experienceScore;

            if (!job.MinimumExperienceYears.HasValue ||
                job.MinimumExperienceYears.Value <= 0)
            {
                experienceScore = 20;
            }
            else
            {
                var ratio =
                    totalExperienceYears /
                    job.MinimumExperienceYears.Value;

                experienceScore =
                    Math.Min(ratio, 1m) * 20m;
            }

            // =====================================
            // 3. EDUCATION = 10
            // =====================================

            decimal educationScore = 0;

            if (string.IsNullOrWhiteSpace(
                    job.EducationRequirement))
            {
                educationScore = 10;
            }
            else
            {
                var importantWords =
                    GetEducationKeywords(
                        job.EducationRequirement);

                var educationText =
                    string.Join(
                        " ",
                        education.Select(x =>
                            $"{x.Qualification} " +
                            $"{x.FieldOfStudy} " +
                            $"{x.InstitutionName}"))
                    .ToLowerInvariant();

                if (importantWords.Count == 0)
                {
                    educationScore = 10;
                }
                else
                {
                    var profileMatches =
                        importantWords.Count(word =>
                            educationText.Contains(word));

                    var profileRatio =
                        (decimal)profileMatches /
                        importantWords.Count;

                    var profileEducationScore =
                        Math.Min(
                            profileRatio * 10m,
                            10m);

                    decimal cvEducationScore = 0;

                    if (!string.IsNullOrWhiteSpace(
                            normalizedCVText))
                    {
                        var cvMatches =
                            importantWords.Count(word =>
                                normalizedCVText
                                    .Contains(word));

                        var cvRatio =
                            (decimal)cvMatches /
                            importantWords.Count;

                        cvEducationScore =
                            Math.Min(
                                cvRatio * 10m,
                                10m);
                    }

                    educationScore =
                        Math.Max(
                            profileEducationScore,
                            cvEducationScore);
                }
            }

            // =====================================
            // 4. PREFERRED SKILLS = 10
            // =====================================

            var preferredSkills =
                jobSkills
                    .Where(x =>
                        x.Importance.Equals(
                            "Preferred",
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();

            decimal preferredScore = 0;

            if (preferredSkills.Count == 0)
            {
                preferredScore = 10;
            }
            else
            {
                decimal pointsPerSkill =
                    10m / preferredSkills.Count;

                foreach (var preferred in preferredSkills)
                {
                    var candidate =
                        candidateSkills
                            .FirstOrDefault(x =>
                                x.SkillId ==
                                preferred.SkillId);

                    var skillInCV =
                        ContainsPhrase(
                            normalizedCVText,
                            preferred.Skill.SkillName);

                    if (candidate != null ||
                        skillInCV)
                    {
                        preferredScore +=
                            pointsPerSkill;

                        if (!matchedSkillNames.Contains(
                                preferred.Skill.SkillName,
                                StringComparer.OrdinalIgnoreCase))
                        {
                            matchedSkillNames.Add(
                                preferred.Skill.SkillName);
                        }
                    }
                }
            }

            // =====================================
            // 5. JOB RELEVANCE = 10
            // =====================================

            decimal relevanceScore = 0;

            var preferredCategory =
                profile.PreferredJobCategory?
                    .Trim()
                    .ToLowerInvariant();

            var jobCategory =
                job.Category?
                    .Trim()
                    .ToLowerInvariant();

            var jobTitle =
                job.Title
                    .Trim()
                    .ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(
                    preferredCategory) &&
                !string.IsNullOrWhiteSpace(
                    jobCategory))
            {
                if (preferredCategory ==
                    jobCategory)
                {
                    relevanceScore = 10;
                }
                else if (
                    preferredCategory.Contains(
                        jobCategory) ||
                    jobCategory.Contains(
                        preferredCategory))
                {
                    relevanceScore = 8;
                }
            }

            if (relevanceScore < 10 &&
                !string.IsNullOrWhiteSpace(
                    profile.ProfessionalSummary))
            {
                var summary =
                    profile.ProfessionalSummary
                        .ToLowerInvariant();

                if (summary.Contains(jobTitle))
                {
                    relevanceScore =
                        Math.Max(
                            relevanceScore,
                            8m);
                }
                else if (
                    !string.IsNullOrWhiteSpace(
                        jobCategory) &&
                    summary.Contains(jobCategory))
                {
                    relevanceScore =
                        Math.Max(
                            relevanceScore,
                            6m);
                }
            }

            // Check actual CV
            if (!string.IsNullOrWhiteSpace(
                    normalizedCVText))
            {
                if (ContainsPhrase(
                        normalizedCVText,
                        jobTitle))
                {
                    relevanceScore =
                        Math.Max(
                            relevanceScore,
                            10m);
                }
                else if (
                    !string.IsNullOrWhiteSpace(
                        jobCategory) &&
                    ContainsPhrase(
                        normalizedCVText,
                        jobCategory))
                {
                    relevanceScore =
                        Math.Max(
                            relevanceScore,
                            7m);
                }
            }

            // =====================================
            // 6. LOCATION = 5
            // =====================================

            decimal locationScore = 0;

            var jobLocation =
                job.Location?
                    .Trim()
                    .ToLowerInvariant();

            var candidateLocation =
                profile.CurrentLocation?
                    .Trim()
                    .ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(
                    jobLocation))
            {
                locationScore = 5;
            }
            else if (
                jobLocation.Contains("remote"))
            {
                locationScore = 5;
            }
            else if (
                !string.IsNullOrWhiteSpace(
                    candidateLocation))
            {
                if (jobLocation ==
                    candidateLocation)
                {
                    locationScore = 5;
                }
                else if (
                    jobLocation.Contains(
                        candidateLocation) ||
                    candidateLocation.Contains(
                        jobLocation))
                {
                    locationScore = 4;
                }
            }

            // =====================================
            // TOTAL
            // =====================================

            skillsScore =
                Math.Round(skillsScore, 2);

            experienceScore =
                Math.Round(experienceScore, 2);

            educationScore =
                Math.Round(educationScore, 2);

            preferredScore =
                Math.Round(preferredScore, 2);

            relevanceScore =
                Math.Round(relevanceScore, 2);

            locationScore =
                Math.Round(locationScore, 2);

            var overallScore =
                skillsScore +
                experienceScore +
                educationScore +
                preferredScore +
                relevanceScore +
                locationScore;

            overallScore =
                Math.Clamp(
                    overallScore,
                    0,
                    100);

            overallScore =
                Math.Round(
                    overallScore,
                    2);

            // =====================================
            // RECOMMENDATIONS
            // =====================================

            var recommendations =
                new List<string>();

            if (missingSkillNames.Count > 0)
            {
                recommendations.Add(
                    "Improve or add the missing required skills.");
            }

            if (experienceScore < 20)
            {
                recommendations.Add(
                    $"The job requires approximately " +
                    $"{job.MinimumExperienceYears} years of experience. " +
                    $"Your recorded experience is approximately " +
                    $"{Math.Round(totalExperienceYears, 1)} years.");
            }

            if (educationScore < 10)
            {
                recommendations.Add(
                    "Review the job education requirements and add relevant qualifications or fields of study.");
            }

            if (preferredScore < 10)
            {
                recommendations.Add(
                    "Adding relevant preferred skills could improve your match score.");
            }

            if (relevanceScore < 10)
            {
                recommendations.Add(
                    "Improve your professional summary and CV content to better reflect this type of role.");
            }

            if (locationScore < 5)
            {
                recommendations.Add(
                    "The job location does not fully match your current location.");
            }

            if (string.IsNullOrWhiteSpace(cvText))
            {
                recommendations.Add(
                    "The uploaded CV could not provide readable text.");
            }

            if (overallScore >= 85)
            {
                recommendations.Insert(
                    0,
                    "Strong match. Your profile and CV align well with this job.");
            }
            else if (overallScore >= 70)
            {
                recommendations.Insert(
                    0,
                    "Good match. A few improvements could strengthen your application.");
            }
            else if (overallScore >= 50)
            {
                recommendations.Insert(
                    0,
                    "Moderate match. Review the missing requirements before applying.");
            }
            else
            {
                recommendations.Insert(
                    0,
                    "Low match. Consider improving your profile, CV and relevant skills.");
            }

            // =====================================
            // SAVE MATCH RESULT
            // =====================================

            var existingResult =
                await _context.CVMatchResults
                    .FirstOrDefaultAsync(x =>
                        x.JobApplicationId ==
                        applicationId);

            if (existingResult == null)
            {
                existingResult =
                    new CVMatchResult
                    {
                        JobApplicationId =
                            applicationId
                    };

                _context.CVMatchResults.Add(
                    existingResult);
            }

            existingResult.SkillsScore =
                skillsScore;

            existingResult.ExperienceScore =
                experienceScore;

            existingResult.EducationScore =
                educationScore;

            existingResult.PreferredSkillsScore =
                preferredScore;

            existingResult.RelevanceScore =
                relevanceScore;

            existingResult.LocationScore =
                locationScore;

            existingResult.OverallScore =
                overallScore;

            existingResult.MatchedSkills =
                matchedSkillNames.Count > 0
                    ? string.Join(
                        ", ",
                        matchedSkillNames
                            .Distinct(
                                StringComparer.OrdinalIgnoreCase))
                    : "None";

            existingResult.MissingSkills =
                missingSkillNames.Count > 0
                    ? string.Join(
                        ", ",
                        missingSkillNames
                            .Distinct(
                                StringComparer.OrdinalIgnoreCase))
                    : "None";

            existingResult.Recommendations =
                string.Join(
                    Environment.NewLine,
                    recommendations);

            existingResult.CalculatedAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return existingResult;
        }

        // =====================================
        // CHECK CV TEXT
        // =====================================

        private static bool ContainsPhrase(
            string text,
            string? phrase)
        {
            if (string.IsNullOrWhiteSpace(text) ||
                string.IsNullOrWhiteSpace(phrase))
            {
                return false;
            }

            return text.Contains(
                phrase.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }

        // =====================================
        // EDUCATION KEYWORDS
        // =====================================

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
                .Where(x =>
                    x.Length >= 3)
                .Where(x =>
                    !IgnoredEducationWords
                        .Contains(x))
                .Distinct()
                .ToList();
        }

        private static readonly HashSet<string>
            IgnoredEducationWords =
                new(
                    StringComparer.OrdinalIgnoreCase)
                {
                    "the",
                    "and",
                    "for",
                    "with",
                    "related",
                    "field",
                    "degree",
                    "qualification",
                    "required",
                    "preferred",
                    "from",
                    "have",
                    "must",
                    "should",
                    "minimum",
                    "equivalent"
                };
    }
}