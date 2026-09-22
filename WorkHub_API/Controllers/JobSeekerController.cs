using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkHub.API.Data;
using WorkHub.API.DTOs;
using WorkHub.API.Models;

namespace WorkHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "RegisteredUser")]
    public class JobSeekerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JobSeekerController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // GET MY JOB SEEKER PROFILE
        // =========================================

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var profile =
                await _context.JobSeekerProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.UserId == userId.Value);

            if (profile == null)
            {
                return NotFound(new
                {
                    message =
                        "Job Seeker profile has not been created yet."
                });
            }

            return Ok(new
            {
                profile.JobSeekerProfileId,
                profile.UserId,
                profile.ProfessionalSummary,
                profile.PreferredJobCategory,
                profile.ExpectedSalary,
                profile.CurrentLocation,
                profile.ProfileImagePath,
                profile.IsActive,
                profile.CreatedAt
            });
        }


        // =========================================
        // CREATE OR UPDATE JOB SEEKER PROFILE
        // =========================================

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(
            JobSeekerProfileDto request)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }


            var userExists =
                await _context.Users
                    .AnyAsync(
                        x => x.UserId == userId.Value);

            if (!userExists)
            {
                return NotFound(new
                {
                    message =
                        "User account not found."
                });
            }


            var profile =
                await _context.JobSeekerProfiles
                    .FirstOrDefaultAsync(
                        x => x.UserId == userId.Value);


            // =====================================
            // CREATE PROFILE IF IT DOES NOT EXIST
            // =====================================

            var isNewProfile = false;


            if (profile == null)
            {
                profile =
                    new JobSeekerProfile
                    {
                        UserId =
                            userId.Value,

                        IsActive =
                            true,

                        CreatedAt =
                            DateTime.UtcNow
                    };


                _context.JobSeekerProfiles.Add(
                    profile);


                isNewProfile = true;
            }


            // =====================================
            // UPDATE PROFILE INFORMATION
            // =====================================

            profile.ProfessionalSummary =
                Clean(
                    request.ProfessionalSummary);


            profile.PreferredJobCategory =
                Clean(
                    request.PreferredJobCategory);


            profile.ExpectedSalary =
                request.ExpectedSalary;


            profile.CurrentLocation =
                Clean(
                    request.CurrentLocation);


            profile.IsActive =
                true;


            await _context.SaveChangesAsync();


            return Ok(new
            {
                message =
                    isNewProfile
                        ? "Job Seeker profile created successfully."
                        : "Job Seeker profile updated successfully.",

                profile = new
                {
                    profile.JobSeekerProfileId,
                    profile.UserId,
                    profile.ProfessionalSummary,
                    profile.PreferredJobCategory,
                    profile.ExpectedSalary,
                    profile.CurrentLocation,
                    profile.IsActive,
                    profile.CreatedAt
                }
            });
        }


        // =========================================
        // ADD SKILL
        // =========================================

        [HttpPost("skills")]
        public async Task<IActionResult> AddSkill(
            AddJobSeekerSkillDto request)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }


            var profile =
                await _context.JobSeekerProfiles
                    .FirstOrDefaultAsync(
                        x => x.UserId == userId.Value);


            if (profile == null)
            {
                return NotFound(new
                {
                    message =
                        "Create your Job Seeker profile first."
                });
            }


            var skill =
                await _context.Skills
                    .FirstOrDefaultAsync(
                        x =>
                            x.SkillId == request.SkillId &&
                            x.IsActive);


            if (skill == null)
            {
                return NotFound(new
                {
                    message =
                        "Skill not found."
                });
            }


            var exists =
                await _context.JobSeekerSkills
                    .AnyAsync(
                        x =>
                            x.JobSeekerProfileId ==
                            profile.JobSeekerProfileId &&

                            x.SkillId ==
                            request.SkillId);


            if (exists)
            {
                return BadRequest(new
                {
                    message =
                        "This skill is already added to your profile."
                });
            }


            var proficiencyLevel =
                Clean(
                    request.ProficiencyLevel);


            if (string.IsNullOrWhiteSpace(
                    proficiencyLevel))
            {
                return BadRequest(new
                {
                    message =
                        "Proficiency level is required."
                });
            }


            var jobSeekerSkill =
                new JobSeekerSkill
                {
                    JobSeekerProfileId =
                        profile.JobSeekerProfileId,

                    SkillId =
                        request.SkillId,

                    ProficiencyLevel =
                        proficiencyLevel,

                    YearsOfExperience =
                        request.YearsOfExperience
                };


            _context.JobSeekerSkills.Add(
                jobSeekerSkill);


            await _context.SaveChangesAsync();


            return Ok(new
            {
                message =
                    "Skill added successfully.",

                skill = new
                {
                    jobSeekerSkill.JobSeekerSkillId,

                    skill.SkillId,

                    skill.SkillName,

                    jobSeekerSkill.ProficiencyLevel,

                    jobSeekerSkill.YearsOfExperience
                }
            });
        }


        // =========================================
        // GET MY SKILLS
        // =========================================

        [HttpGet("skills")]
        public async Task<IActionResult> GetMySkills()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }


            var profile =
                await _context.JobSeekerProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.UserId == userId.Value);


            if (profile == null)
            {
                return NotFound(new
                {
                    message =
                        "Create your Job Seeker profile first."
                });
            }


            var skills =
                await _context.JobSeekerSkills
                    .AsNoTracking()
                    .Where(
                        x =>
                            x.JobSeekerProfileId ==
                            profile.JobSeekerProfileId)
                    .Include(
                        x => x.Skill)
                    .Select(
                        x => new
                        {
                            x.JobSeekerSkillId,

                            x.SkillId,

                            x.Skill.SkillName,

                            x.Skill.Category,

                            x.ProficiencyLevel,

                            x.YearsOfExperience
                        })
                    .ToListAsync();


            return Ok(skills);
        }


        // =========================================
        // DELETE MY SKILL
        // =========================================

        [HttpDelete("skills/{jobSeekerSkillId:int}")]
        public async Task<IActionResult> DeleteSkill(
            int jobSeekerSkillId)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }


            var profile =
                await _context.JobSeekerProfiles
                    .FirstOrDefaultAsync(
                        x => x.UserId == userId.Value);


            if (profile == null)
            {
                return NotFound(new
                {
                    message =
                        "Job Seeker profile not found."
                });
            }


            var skill =
                await _context.JobSeekerSkills
                    .FirstOrDefaultAsync(
                        x =>
                            x.JobSeekerSkillId ==
                            jobSeekerSkillId &&

                            x.JobSeekerProfileId ==
                            profile.JobSeekerProfileId);


            if (skill == null)
            {
                return NotFound(new
                {
                    message =
                        "Skill not found."
                });
            }


            _context.JobSeekerSkills.Remove(
                skill);


            await _context.SaveChangesAsync();


            return Ok(new
            {
                message =
                    "Skill removed successfully."
            });
        }


        // =========================================
        // GET CURRENT USER ID
        // =========================================

        private int? GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return null;
            }


            return userId;
        }


        // =========================================
        // CLEAN STRING
        // =========================================

        private static string? Clean(
            string? value)
        {
            return string.IsNullOrWhiteSpace(
                    value)
                ? null
                : value.Trim();
        }
    }
}