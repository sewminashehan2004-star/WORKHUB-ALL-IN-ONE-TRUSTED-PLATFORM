using Microsoft.EntityFrameworkCore;
using WorkHub.API.Data;
using WorkHub.API.Interfaces;
using WorkHub.API.Models;

namespace WorkHub.API.Services
{
    public class CareerBenchmarkService
        : ICareerBenchmarkService
    {
        private readonly ApplicationDbContext
            _context;


        // =========================================
        // MARKET SAMPLE SETTINGS
        // =========================================

        private const int MinimumLiveMarketJobs = 3;

        private const decimal MinimumSkillDemandPercentage =
            20m;


        public CareerBenchmarkService(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // BUILD CAREER BENCHMARK
        // =========================================

        public async Task<CareerBenchmarkResult?>
            BuildBenchmarkAsync(
                int careerRoleId)
        {
            var now =
                DateTime.UtcNow;


            // =====================================
            // CAREER ROLE
            // =====================================

            var careerRole =
                await _context.CareerRoles
                    .Include(x =>
                        x.CareerCategory)
                    .Include(x =>
                        x.CareerRoleSkills)
                        .ThenInclude(x =>
                            x.Skill)
                    .FirstOrDefaultAsync(x =>
                        x.CareerRoleId ==
                        careerRoleId
                        &&
                        x.IsActive
                        &&
                        x.CareerCategory
                            .IsActive);


            if (careerRole == null)
            {
                return null;
            }


            // =====================================
            // ACTIVE JOBS FOR THIS ROLE
            // =====================================

            var jobs =
                await _context.Jobs
                    .Where(x =>
                        x.CareerRoleId ==
                        careerRoleId
                        &&
                        x.Status == "Active"
                        &&
                        (
                            !x.ClosingDate.HasValue
                            ||
                            x.ClosingDate.Value >= now
                        ))
                    .Select(x => new
                    {
                        x.JobId,

                        x.MinimumExperienceYears,

                        x.EducationRequirement
                    })
                    .ToListAsync();


            var activeJobCount =
                jobs.Count;


            var activeJobIds =
                jobs
                    .Select(x =>
                        x.JobId)
                    .ToList();


            // =====================================
            // LIVE JOB SKILLS
            // =====================================

            var liveJobSkills =
                new List<JobSkill>();


            if (activeJobIds.Count > 0)
            {
                liveJobSkills =
                    await _context.JobSkills
                        .Where(x =>
                            activeJobIds.Contains(
                                x.JobId))
                        .Include(x =>
                            x.Skill)
                        .ToListAsync();
            }


            // =====================================
            // RESULT
            // =====================================

            var result =
                new CareerBenchmarkResult
                {
                    CareerRoleId =
                        careerRole.CareerRoleId,

                    CareerCategoryId =
                        careerRole.CareerCategoryId,

                    RoleName =
                        careerRole.RoleName,

                    CategoryName =
                        careerRole
                            .CareerCategory
                            .CategoryName,

                    ActiveJobCount =
                        activeJobCount,

                    DataSource =
                        DetermineDataSource(
                            activeJobCount),

                    ConfidenceLevel =
                        DetermineConfidence(
                            activeJobCount),

                    GeneratedAt =
                        DateTime.UtcNow
                };


            // =====================================
            // EXPERIENCE BENCHMARK
            // =====================================

            result.AverageMinimumExperienceYears =
                CalculateExperienceBenchmark(
                    careerRole
                        .BaselineExperienceYears,
                    jobs
                        .Where(x =>
                            x.MinimumExperienceYears
                                .HasValue)
                        .Select(x =>
                            x.MinimumExperienceYears!
                                .Value)
                        .ToList(),
                    activeJobCount);


            // =====================================
            // EDUCATION BENCHMARK
            // =====================================

            result.CommonEducationRequirements =
                BuildEducationBenchmark(
                    careerRole
                        .BaselineEducationRequirement,
                    jobs
                        .Select(x =>
                            x.EducationRequirement)
                        .Where(x =>
                            !string.IsNullOrWhiteSpace(
                                x))
                        .Select(x =>
                            x!)
                        .ToList(),
                    activeJobCount);


            // =====================================
            // SKILL BENCHMARK
            // =====================================

            result.Skills =
                BuildSkillBenchmark(
                    careerRole.CareerRoleSkills
                        .ToList(),
                    liveJobSkills,
                    activeJobCount);


            return result;
        }


        // =========================================
        // EXPERIENCE BENCHMARK
        // =========================================

        private static decimal?
            CalculateExperienceBenchmark(
                decimal? baselineYears,
                List<decimal> liveYears,
                int activeJobCount)
        {
            var validLiveYears =
                liveYears
                    .Where(x =>
                        x >= 0)
                    .ToList();


            // =====================================
            // ENOUGH LIVE MARKET DATA
            // =====================================

            if (activeJobCount >=
                    MinimumLiveMarketJobs
                &&
                validLiveYears.Count > 0)
            {
                return Math.Round(
                    validLiveYears.Average(),
                    1);
            }


            // =====================================
            // SMALL MARKET SAMPLE
            // BLEND LIVE + BASELINE
            // =====================================

            if (activeJobCount > 0
                &&
                validLiveYears.Count > 0
                &&
                baselineYears.HasValue)
            {
                var liveAverage =
                    validLiveYears.Average();


                return Math.Round(
                    (
                        liveAverage
                        +
                        baselineYears.Value
                    )
                    / 2m,
                    1);
            }


            // =====================================
            // LIVE ONLY
            // =====================================

            if (validLiveYears.Count > 0)
            {
                return Math.Round(
                    validLiveYears.Average(),
                    1);
            }


            // =====================================
            // BASELINE FALLBACK
            // =====================================

            return baselineYears;
        }


        // =========================================
        // EDUCATION BENCHMARK
        // =========================================

        private static List<string>
            BuildEducationBenchmark(
                string? baselineEducation,
                List<string> liveRequirements,
                int activeJobCount)
        {
            var result =
                new List<string>();


            // =====================================
            // CLEAN LIVE REQUIREMENTS
            // =====================================

            var cleaned =
                liveRequirements
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(
                            x))
                    .Select(x =>
                        x.Trim())
                    .ToList();


            // =====================================
            // ENOUGH LIVE JOBS
            // =====================================

            if (activeJobCount >=
                    MinimumLiveMarketJobs
                &&
                cleaned.Count > 0)
            {
                var common =
                    cleaned
                        .GroupBy(
                            x => x,
                            StringComparer
                                .OrdinalIgnoreCase)
                        .OrderByDescending(
                            x => x.Count())
                        .ThenBy(
                            x => x.Key)
                        .Take(5)
                        .Select(
                            x => x.Key)
                        .ToList();


                result.AddRange(
                    common);


                return result;
            }


            // =====================================
            // SMALL SAMPLE
            // INCLUDE LIVE DATA
            // =====================================

            foreach (var requirement
                     in cleaned
                         .Distinct(
                             StringComparer
                                 .OrdinalIgnoreCase)
                         .Take(5))
            {
                result.Add(
                    requirement);
            }


            // =====================================
            // BASELINE FALLBACK
            // =====================================

            if (!string.IsNullOrWhiteSpace(
                    baselineEducation)
                &&
                !result.Contains(
                    baselineEducation.Trim(),
                    StringComparer
                        .OrdinalIgnoreCase))
            {
                result.Add(
                    baselineEducation.Trim());
            }


            return result;
        }


        // =========================================
        // SKILL BENCHMARK
        // =========================================

        private static List<CareerBenchmarkSkillResult>
            BuildSkillBenchmark(
                List<CareerRoleSkill> baselineSkills,
                List<JobSkill> liveJobSkills,
                int activeJobCount)
        {
            var results =
                new List<
                    CareerBenchmarkSkillResult>();


            // =====================================
            // GROUP LIVE SKILLS
            // =====================================

            var liveGroups =
                liveJobSkills
                    .GroupBy(x =>
                        x.SkillId)
                    .ToDictionary(
                        x => x.Key,
                        x => x.ToList());


            var baselineDictionary =
                baselineSkills
                    .GroupBy(x =>
                        x.SkillId)
                    .ToDictionary(
                        x => x.Key,
                        x => x.First());


            // =====================================
            // DECIDE WHICH SKILLS TO USE
            // =====================================

            var selectedSkillIds =
                new HashSet<int>();


            // =====================================
            // STRONG LIVE MARKET SAMPLE
            // =====================================

            if (activeJobCount >=
                MinimumLiveMarketJobs)
            {
                foreach (var group
                         in liveGroups)
                {
                    var mentionedJobs =
                        group.Value
                            .Select(x =>
                                x.JobId)
                            .Distinct()
                            .Count();


                    var demand =
                        activeJobCount == 0
                            ? 0
                            : (
                                (decimal)
                                mentionedJobs
                                /
                                activeJobCount
                              )
                              * 100m;


                    if (demand >=
                        MinimumSkillDemandPercentage)
                    {
                        selectedSkillIds.Add(
                            group.Key);
                    }
                }


                // If live jobs exist but employers
                // added no useful skill information,
                // use baseline instead.

                if (selectedSkillIds.Count == 0)
                {
                    foreach (var skill
                             in baselineSkills)
                    {
                        selectedSkillIds.Add(
                            skill.SkillId);
                    }
                }
            }
            else
            {
                // =================================
                // SMALL / NO LIVE MARKET SAMPLE
                // USE BOTH BASELINE + AVAILABLE JOBS
                // =================================

                foreach (var skill
                         in baselineSkills)
                {
                    selectedSkillIds.Add(
                        skill.SkillId);
                }


                foreach (var skillId
                         in liveGroups.Keys)
                {
                    selectedSkillIds.Add(
                        skillId);
                }
            }


            // =====================================
            // CREATE BENCHMARK SKILLS
            // =====================================

            foreach (var skillId
                     in selectedSkillIds)
            {
                baselineDictionary
                    .TryGetValue(
                        skillId,
                        out var baseline);


                liveGroups
                    .TryGetValue(
                        skillId,
                        out var liveRows);


                liveRows ??=
                    new List<JobSkill>();


                // =================================
                // SKILL DETAILS
                // =================================

                var skill =
                    liveRows
                        .Select(x =>
                            x.Skill)
                        .FirstOrDefault()
                    ??
                    baseline?.Skill;


                if (skill == null)
                {
                    continue;
                }


                // =================================
                // LIVE COUNTS
                // =================================

                var mentionedJobCount =
                    liveRows
                        .Select(x =>
                            x.JobId)
                        .Distinct()
                        .Count();


                var requiredJobCount =
                    liveRows
                        .Where(x =>
                            x.Importance.Equals(
                                "Required",
                                StringComparison
                                    .OrdinalIgnoreCase))
                        .Select(x =>
                            x.JobId)
                        .Distinct()
                        .Count();


                var preferredJobCount =
                    liveRows
                        .Where(x =>
                            x.Importance.Equals(
                                "Preferred",
                                StringComparison
                                    .OrdinalIgnoreCase))
                        .Select(x =>
                            x.JobId)
                        .Distinct()
                        .Count();


                // =================================
                // DEMAND %
                // =================================

                decimal demandPercentage =
                    0;


                if (activeJobCount > 0)
                {
                    demandPercentage =
                        Math.Round(
                            (
                                (decimal)
                                mentionedJobCount
                                /
                                activeJobCount
                            )
                            * 100m,
                            1);
                }


                // =================================
                // IMPORTANCE
                // =================================

                var importance =
                    DetermineSkillImportance(
                        baseline,
                        requiredJobCount,
                        mentionedJobCount,
                        activeJobCount);


                // =================================
                // YEARS
                // =================================

                var liveRequiredYears =
                    liveRows
                        .Where(x =>
                            x.RequiredYearsOfExperience
                                .HasValue)
                        .Select(x =>
                            x.RequiredYearsOfExperience!
                                .Value)
                        .ToList();


                decimal? averageRequiredYears =
                    null;


                if (liveRequiredYears.Count > 0)
                {
                    averageRequiredYears =
                        Math.Round(
                            liveRequiredYears.Average(),
                            1);
                }
                else if (
                    baseline?
                        .RecommendedYearsOfExperience
                        .HasValue == true)
                {
                    averageRequiredYears =
                        baseline
                            .RecommendedYearsOfExperience;
                }


                // =================================
                // SOURCE
                // =================================

                var hasLive =
                    liveRows.Count > 0;


                var hasBaseline =
                    baseline != null;


                var source =
                    hasLive && hasBaseline
                        ? "Baseline + Live Market"
                        : hasLive
                            ? "Live Market"
                            : "Baseline";


                results.Add(
                    new CareerBenchmarkSkillResult
                    {
                        SkillId =
                            skill.SkillId,

                        SkillName =
                            skill.SkillName,

                        SkillCategory =
                            skill.Category,

                        Importance =
                            importance,

                        DemandPercentage =
                            demandPercentage,

                        RequiredJobCount =
                            requiredJobCount,

                        PreferredJobCount =
                            preferredJobCount,

                        AverageRequiredYears =
                            averageRequiredYears,

                        Source =
                            source
                    });
            }


            // =====================================
            // SORT
            // =====================================

            return results
                .OrderBy(x =>
                    x.Importance == "Required"
                        ? 0
                        : 1)
                .ThenByDescending(x =>
                    x.DemandPercentage)
                .ThenBy(x =>
                    x.SkillName)
                .ToList();
        }


        // =========================================
        // DETERMINE SKILL IMPORTANCE
        // =========================================

        private static string
            DetermineSkillImportance(
                CareerRoleSkill? baseline,
                int requiredJobCount,
                int mentionedJobCount,
                int activeJobCount)
        {
            // =====================================
            // LIVE MARKET HAS ENOUGH DATA
            // =====================================

            if (activeJobCount >=
                    MinimumLiveMarketJobs
                &&
                mentionedJobCount > 0)
            {
                var requiredPercentage =
                    (
                        (decimal)
                        requiredJobCount
                        /
                        activeJobCount
                    )
                    * 100m;


                if (requiredPercentage >= 50m)
                {
                    return "Required";
                }


                return "Preferred";
            }


            // =====================================
            // BASELINE
            // =====================================

            if (baseline != null)
            {
                return baseline.Importance.Equals(
                        "Preferred",
                        StringComparison
                            .OrdinalIgnoreCase)
                    ? "Preferred"
                    : "Required";
            }


            // =====================================
            // SMALL LIVE SAMPLE
            // =====================================

            if (requiredJobCount > 0)
            {
                return "Required";
            }


            return "Preferred";
        }


        // =========================================
        // DATA SOURCE
        // =========================================

        private static string
            DetermineDataSource(
                int activeJobCount)
        {
            if (activeJobCount >=
                MinimumLiveMarketJobs)
            {
                return "Live WorkHub Market";
            }


            if (activeJobCount > 0)
            {
                return "Baseline + Limited Live Market";
            }


            return "Career Baseline";
        }


        // =========================================
        // CONFIDENCE
        // =========================================

        private static string
            DetermineConfidence(
                int activeJobCount)
        {
            if (activeJobCount >= 10)
            {
                return "High";
            }


            if (activeJobCount >= 3)
            {
                return "Medium";
            }


            if (activeJobCount >= 1)
            {
                return "Low";
            }


            return "Baseline";
        }
    }
}