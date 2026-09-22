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
    [Authorize(Roles = "RegisteredUser")]
    public class CVIntelligenceController
        : ControllerBase
    {
        private readonly ICVIntelligenceService
            _cvIntelligenceService;

        private readonly ApplicationDbContext
            _context;


        public CVIntelligenceController(
            ICVIntelligenceService cvIntelligenceService,
            ApplicationDbContext context)
        {
            _cvIntelligenceService =
                cvIntelligenceService;

            _context =
                context;
        }


        // =========================================
        // ANALYSE MY CV AGAINST CAREER ROLE
        //
        // POST:
        // api/CVIntelligence/analyse/16
        // =========================================

        [HttpPost("analyse/{careerRoleId}")]
        public async Task<IActionResult> Analyse(
            int careerRoleId)
        {
            // =====================================
            // USER ID FROM JWT
            // =====================================

            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return Unauthorized(new
                {
                    message =
                        "Unable to identify the signed-in user."
                });
            }


            // =====================================
            // VALID CAREER ROLE
            // =====================================

            if (careerRoleId <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "Please select a valid career role."
                });
            }


            var careerRoleExists =
                await _context.CareerRoles
                    .AnyAsync(x =>
                        x.CareerRoleId ==
                        careerRoleId
                        &&
                        x.IsActive
                        &&
                        x.CareerCategory.IsActive);


            if (!careerRoleExists)
            {
                return NotFound(new
                {
                    message =
                        "Career role not found."
                });
            }


            // =====================================
            // JOB SEEKER PROFILE
            // =====================================

            var profile =
                await _context.JobSeekerProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);


            if (profile == null)
            {
                return BadRequest(new
                {
                    message =
                        "Please create your Job Seeker profile before using CV Intelligence."
                });
            }


            // =====================================
            // PRIMARY CV CHECK
            // =====================================

            var primaryCV =
                await _context.CVDocuments
                    .Where(x =>
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId)
                    .OrderByDescending(x =>
                        x.IsPrimary)
                    .ThenByDescending(x =>
                        x.UploadedAt)
                    .FirstOrDefaultAsync();


            if (primaryCV == null)
            {
                return BadRequest(new
                {
                    message =
                        "Please upload a CV before using CV Intelligence."
                });
            }


            // =====================================
            // RUN INTELLIGENCE ENGINE
            // =====================================

            var result =
                await _cvIntelligenceService
                    .AnalyseAsync(
                        userId,
                        careerRoleId);


            if (result == null)
            {
                return BadRequest(new
                {
                    message =
                        "WorkHub could not analyse this CV."
                });
            }


            // =====================================
            // RESPONSE
            // =====================================

            return Ok(new
            {
                message =
                    "CV analysis completed successfully.",


                career = new
                {
                    result.CareerRoleId,

                    result.RoleName,

                    result.CategoryName
                },


                cv = new
                {
                    result.CVDocumentId,

                    result.CVFileName
                },


                readiness = new
                {
                    result.OverallScore,

                    result.ReadinessLevel
                },


                scoreBreakdown = new
                {
                    skills = new
                    {
                        score =
                            result.SkillsScore,

                        maximum =
                            35
                    },

                    experience = new
                    {
                        score =
                            result.ExperienceScore,

                        maximum =
                            25
                    },

                    education = new
                    {
                        score =
                            result.EducationScore,

                        maximum =
                            15
                    },

                    cvQuality = new
                    {
                        score =
                            result.CvQualityScore,

                        maximum =
                            15
                    },

                    careerRelevance = new
                    {
                        score =
                            result
                                .CareerRelevanceScore,

                        maximum =
                            10
                    }
                },


                skills = new
                {
                    matched =
                        result.Skills
                            .Where(x =>
                                x.MatchStatus ==
                                "Matched")
                            .ToList(),

                    partial =
                        result.Skills
                            .Where(x =>
                                x.MatchStatus ==
                                "Partial")
                            .ToList(),

                    missing =
                        result.Skills
                            .Where(x =>
                                x.MatchStatus ==
                                "Missing")
                            .ToList()
                },


                experience = new
                {
                    result
                        .CandidateExperienceYears,

                    result
                        .RecommendedExperienceYears,

                    result
                        .ExperienceStatus
                },


                education = new
                {
                    result
                        .EducationStatus,

                    result
                        .EducationFindings
                },


                cvQuality = new
                {
                    strengths =
                        result.CVStrengths,

                    issues =
                        result.CVIssues
                },


                recommendations =
                    result.Recommendations,


                result.GeneratedAt
            });
        }
    }
}