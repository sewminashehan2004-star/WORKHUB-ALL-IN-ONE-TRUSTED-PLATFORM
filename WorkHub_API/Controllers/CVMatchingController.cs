using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkHub.API.Data;
using WorkHub.API.Interfaces;

namespace WorkHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CVMatchingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICVMatchingService
            _matchingService;

        public CVMatchingController(
            ApplicationDbContext context,
            ICVMatchingService matchingService)
        {
            _context = context;
            _matchingService = matchingService;
        }

        // =========================================
        // CALCULATE MATCH
        // =========================================

        [HttpPost(
            "application/{applicationId}/calculate")]
        public async Task<IActionResult> CalculateMatch(
            int applicationId)
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return Unauthorized();
            }

            var role =
                User.FindFirstValue(
                    ClaimTypes.Role);

            var application =
                await _context.JobApplications
                    .Include(x =>
                        x.JobSeekerProfile)
                    .Include(x =>
                        x.Job)
                    .FirstOrDefaultAsync(x =>
                        x.JobApplicationId ==
                        applicationId);

            if (application == null)
            {
                return NotFound(new
                {
                    message =
                        "Application not found."
                });
            }

            if (role == "RegisteredUser")
            {
                if (application
                        .JobSeekerProfile
                        .UserId != userId)
                {
                    return Forbid();
                }
            }
            else if (role == "Company")
            {
                var company =
                    await _context.CompanyProfiles
                        .FirstOrDefaultAsync(x =>
                            x.UserId == userId);

                if (company == null ||
                    application.Job.CompanyProfileId !=
                    company.CompanyProfileId)
                {
                    return Forbid();
                }
            }
            else if (role != "Admin")
            {
                return Forbid();
            }

            var result =
                await _matchingService
                    .CalculateAndSaveAsync(
                        applicationId);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                message =
                    "CV to Job match calculated successfully.",

                result.CVMatchResultId,

                result.JobApplicationId,

                score = new
                {
                    requiredSkills =
                        result.SkillsScore,

                    experience =
                        result.ExperienceScore,

                    education =
                        result.EducationScore,

                    preferredSkills =
                        result.PreferredSkillsScore,

                    relevance =
                        result.RelevanceScore,

                    location =
                        result.LocationScore,

                    overall =
                        result.OverallScore
                },

                result.MatchedSkills,

                result.MissingSkills,

                result.Recommendations,

                result.CalculatedAt
            });
        }

        // =========================================
        // GET MATCH RESULT
        // =========================================

        [HttpGet(
            "application/{applicationId}")]
        public async Task<IActionResult> GetMatch(
            int applicationId)
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return Unauthorized();
            }

            var role =
                User.FindFirstValue(
                    ClaimTypes.Role);

            var application =
                await _context.JobApplications
                    .Include(x =>
                        x.JobSeekerProfile)
                    .Include(x =>
                        x.Job)
                    .FirstOrDefaultAsync(x =>
                        x.JobApplicationId ==
                        applicationId);

            if (application == null)
            {
                return NotFound(new
                {
                    message =
                        "Application not found."
                });
            }

            if (role == "RegisteredUser" &&
                application
                    .JobSeekerProfile
                    .UserId != userId)
            {
                return Forbid();
            }

            if (role == "Company")
            {
                var company =
                    await _context.CompanyProfiles
                        .FirstOrDefaultAsync(x =>
                            x.UserId == userId);

                if (company == null ||
                    application.Job.CompanyProfileId !=
                    company.CompanyProfileId)
                {
                    return Forbid();
                }
            }

            var result =
                await _context.CVMatchResults
                    .FirstOrDefaultAsync(x =>
                        x.JobApplicationId ==
                        applicationId);

            if (result == null)
            {
                return NotFound(new
                {
                    message =
                        "Match result has not been calculated yet."
                });
            }

            return Ok(new
            {
                result.CVMatchResultId,

                result.JobApplicationId,

                result.SkillsScore,

                result.ExperienceScore,

                result.EducationScore,

                result.PreferredSkillsScore,

                result.RelevanceScore,

                result.LocationScore,

                result.OverallScore,

                result.MatchedSkills,

                result.MissingSkills,

                result.Recommendations,

                result.CalculatedAt
            });
        }
    }
}