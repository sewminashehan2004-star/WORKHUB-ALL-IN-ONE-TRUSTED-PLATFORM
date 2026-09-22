using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkHub.API.Data;
using WorkHub.API.DTOs;
using WorkHub.API.Models;

namespace WorkHub.API.Controllers
{
    [Route("api/jobseeker/education")]
    [ApiController]
    [Authorize(Roles = "RegisteredUser")]
    public class JobSeekerEducationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JobSeekerEducationController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // GET ALL EDUCATION
        [HttpGet]
        public async Task<IActionResult> GetEducation()
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

            var education =
                await _context.JobSeekerEducations
                    .Where(x =>
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId)
                    .OrderByDescending(x => x.StartDate)
                    .Select(x => new
                    {
                        x.JobSeekerEducationId,
                        x.InstitutionName,
                        x.Qualification,
                        x.FieldOfStudy,
                        x.StartDate,
                        x.EndDate,
                        x.IsCurrentlyStudying
                    })
                    .ToListAsync();

            return Ok(education);
        }

        // ADD EDUCATION
        [HttpPost]
        public async Task<IActionResult> AddEducation(
            JobSeekerEducationDto request)
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

            if (request.StartDate.HasValue &&
                request.EndDate.HasValue &&
                request.EndDate.Value <
                request.StartDate.Value)
            {
                return BadRequest(new
                {
                    message =
                        "End date cannot be earlier than start date."
                });
            }

            var education = new JobSeekerEducation
            {
                JobSeekerProfileId =
                    profile.JobSeekerProfileId,

                InstitutionName =
                    request.InstitutionName.Trim(),

                Qualification =
                    request.Qualification.Trim(),

                FieldOfStudy =
                    request.FieldOfStudy?.Trim(),

                StartDate =
                    request.StartDate,

                EndDate =
                    request.IsCurrentlyStudying
                        ? null
                        : request.EndDate,

                IsCurrentlyStudying =
                    request.IsCurrentlyStudying
            };

            _context.JobSeekerEducations.Add(education);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Education added successfully.",

                education.JobSeekerEducationId
            });
        }

        // UPDATE EDUCATION
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEducation(
            int id,
            JobSeekerEducationDto request)
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

            var education =
                await _context.JobSeekerEducations
                    .FirstOrDefaultAsync(x =>
                        x.JobSeekerEducationId == id &&
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId);

            if (education == null)
            {
                return NotFound(new
                {
                    message = "Education record not found."
                });
            }

            if (request.StartDate.HasValue &&
                request.EndDate.HasValue &&
                request.EndDate.Value <
                request.StartDate.Value)
            {
                return BadRequest(new
                {
                    message =
                        "End date cannot be earlier than start date."
                });
            }

            education.InstitutionName =
                request.InstitutionName.Trim();

            education.Qualification =
                request.Qualification.Trim();

            education.FieldOfStudy =
                request.FieldOfStudy?.Trim();

            education.StartDate =
                request.StartDate;

            education.EndDate =
                request.IsCurrentlyStudying
                    ? null
                    : request.EndDate;

            education.IsCurrentlyStudying =
                request.IsCurrentlyStudying;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Education updated successfully."
            });
        }

        // DELETE EDUCATION
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEducation(
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

            var education =
                await _context.JobSeekerEducations
                    .FirstOrDefaultAsync(x =>
                        x.JobSeekerEducationId == id &&
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId);

            if (education == null)
            {
                return NotFound(new
                {
                    message = "Education record not found."
                });
            }

            _context.JobSeekerEducations.Remove(education);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Education removed successfully."
            });
        }
    }
}