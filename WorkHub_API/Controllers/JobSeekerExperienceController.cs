using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkHub.API.Data;
using WorkHub.API.DTOs;
using WorkHub.API.Models;

namespace WorkHub.API.Controllers
{
    [Route("api/jobseeker/experience")]
    [ApiController]
    [Authorize(Roles = "RegisteredUser")]
    public class JobSeekerExperienceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JobSeekerExperienceController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // GET ALL MY EXPERIENCE
        [HttpGet]
        public async Task<IActionResult> GetExperience()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var profile = await _context.JobSeekerProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (profile == null)
            {
                return NotFound(new
                {
                    message = "Job Seeker profile not found."
                });
            }

            var experience =
                await _context.JobSeekerExperiences
                    .Where(x =>
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId)
                    .OrderByDescending(x => x.StartDate)
                    .Select(x => new
                    {
                        x.JobSeekerExperienceId,
                        x.JobTitle,
                        x.CompanyName,
                        x.Location,
                        x.StartDate,
                        x.EndDate,
                        x.IsCurrentJob,
                        x.Description
                    })
                    .ToListAsync();

            return Ok(experience);
        }

        // ADD EXPERIENCE
        [HttpPost]
        public async Task<IActionResult> AddExperience(
            JobSeekerExperienceDto request)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var profile = await _context.JobSeekerProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (profile == null)
            {
                return NotFound(new
                {
                    message = "Job Seeker profile not found."
                });
            }

            if (!request.IsCurrentJob &&
                request.EndDate == null)
            {
                return BadRequest(new
                {
                    message =
                        "End date is required when this is not your current job."
                });
            }

            if (request.EndDate.HasValue &&
                request.EndDate.Value < request.StartDate)
            {
                return BadRequest(new
                {
                    message =
                        "End date cannot be earlier than start date."
                });
            }

            var experience = new JobSeekerExperience
            {
                JobSeekerProfileId =
                    profile.JobSeekerProfileId,

                JobTitle = request.JobTitle.Trim(),
                CompanyName = request.CompanyName.Trim(),
                Location = request.Location?.Trim(),
                StartDate = request.StartDate,

                EndDate = request.IsCurrentJob
                    ? null
                    : request.EndDate,

                IsCurrentJob = request.IsCurrentJob,
                Description = request.Description?.Trim()
            };

            _context.JobSeekerExperiences.Add(experience);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Work experience added successfully.",

                experience.JobSeekerExperienceId
            });
        }

        // UPDATE EXPERIENCE
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExperience(
            int id,
            JobSeekerExperienceDto request)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var profile = await _context.JobSeekerProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (profile == null)
            {
                return NotFound(new
                {
                    message = "Job Seeker profile not found."
                });
            }

            var experience =
                await _context.JobSeekerExperiences
                    .FirstOrDefaultAsync(x =>
                        x.JobSeekerExperienceId == id &&
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId);

            if (experience == null)
            {
                return NotFound(new
                {
                    message = "Work experience not found."
                });
            }

            if (!request.IsCurrentJob &&
                request.EndDate == null)
            {
                return BadRequest(new
                {
                    message =
                        "End date is required when this is not your current job."
                });
            }

            if (request.EndDate.HasValue &&
                request.EndDate.Value < request.StartDate)
            {
                return BadRequest(new
                {
                    message =
                        "End date cannot be earlier than start date."
                });
            }

            experience.JobTitle =
                request.JobTitle.Trim();

            experience.CompanyName =
                request.CompanyName.Trim();

            experience.Location =
                request.Location?.Trim();

            experience.StartDate =
                request.StartDate;

            experience.EndDate =
                request.IsCurrentJob
                    ? null
                    : request.EndDate;

            experience.IsCurrentJob =
                request.IsCurrentJob;

            experience.Description =
                request.Description?.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Work experience updated successfully."
            });
        }

        // DELETE EXPERIENCE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExperience(
            int id)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var profile = await _context.JobSeekerProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (profile == null)
            {
                return NotFound(new
                {
                    message = "Job Seeker profile not found."
                });
            }

            var experience =
                await _context.JobSeekerExperiences
                    .FirstOrDefaultAsync(x =>
                        x.JobSeekerExperienceId == id &&
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId);

            if (experience == null)
            {
                return NotFound(new
                {
                    message = "Work experience not found."
                });
            }

            _context.JobSeekerExperiences.Remove(experience);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Work experience removed successfully."
            });
        }
    }
}