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
    public class JobsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;


        public JobsController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // PUBLIC - GET ACTIVE JOBS
        // =========================================

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetJobs(
            string? search,
            string? category,
            int? careerCategoryId,
            int? careerRoleId,
            string? location,
            string? jobType)
        {
            var query =
                _context.Jobs
                    .Where(x =>
                        x.Status == "Active"
                        &&
                        (
                            !x.ClosingDate.HasValue
                            ||
                            x.ClosingDate.Value >=
                            DateTime.UtcNow
                        ))
                    .AsQueryable();


            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query =
                    query.Where(x =>
                        x.Title.Contains(search)
                        ||
                        x.Description.Contains(search));
            }


            if (!string.IsNullOrWhiteSpace(category))
            {
                category = category.Trim();

                query =
                    query.Where(x =>
                        x.Category == category);
            }


            if (careerCategoryId.HasValue)
            {
                query =
                    query.Where(x =>
                        x.CareerRole != null
                        &&
                        x.CareerRole.CareerCategoryId ==
                        careerCategoryId.Value);
            }


            if (careerRoleId.HasValue)
            {
                query =
                    query.Where(x =>
                        x.CareerRoleId ==
                        careerRoleId.Value);
            }


            if (!string.IsNullOrWhiteSpace(location))
            {
                location = location.Trim();

                query =
                    query.Where(x =>
                        x.Location != null
                        &&
                        x.Location.Contains(location));
            }


            if (!string.IsNullOrWhiteSpace(jobType))
            {
                jobType = jobType.Trim();

                query =
                    query.Where(x =>
                        x.JobType == jobType);
            }


            var jobs =
                await query
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Select(x => new
                    {
                        x.JobId,

                        x.Title,

                        x.Category,

                        x.CareerRoleId,

                        CareerRoleName =
                            x.CareerRole != null
                                ? x.CareerRole.RoleName
                                : null,

                        CareerCategoryId =
                            x.CareerRole != null
                                ? x.CareerRole.CareerCategoryId
                                : (int?)null,

                        CareerCategoryName =
                            x.CareerRole != null
                                ? x.CareerRole
                                    .CareerCategory
                                    .CategoryName
                                : x.Category,

                        x.Location,

                        x.JobType,

                        x.ImagePath,

                        x.SalaryMin,

                        x.SalaryMax,

                        x.MinimumExperienceYears,

                        x.ClosingDate,

                        x.CreatedAt,

                        Company = new
                        {
                            x.CompanyProfile.CompanyProfileId,

                            x.CompanyProfile.CompanyName,

                            x.CompanyProfile.VerificationStatus
                        }
                    })
                    .ToListAsync();


            return Ok(jobs);
        }


        // =========================================
        // PUBLIC - JOB DETAILS
        // =========================================

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJob(
            int id)
        {
            var job =
                await _context.Jobs
                    .Where(x =>
                        x.JobId == id
                        &&
                        x.Status == "Active")
                    .Select(x => new
                    {
                        x.JobId,

                        x.Title,

                        x.Description,

                        x.Category,

                        x.CareerRoleId,

                        CareerRoleName =
                            x.CareerRole != null
                                ? x.CareerRole.RoleName
                                : null,

                        CareerCategoryId =
                            x.CareerRole != null
                                ? x.CareerRole.CareerCategoryId
                                : (int?)null,

                        CareerCategoryName =
                            x.CareerRole != null
                                ? x.CareerRole
                                    .CareerCategory
                                    .CategoryName
                                : x.Category,

                        x.Location,

                        x.JobType,

                        x.ImagePath,

                        x.SalaryMin,

                        x.SalaryMax,

                        x.MinimumExperienceYears,

                        x.EducationRequirement,

                        x.ClosingDate,

                        x.Status,

                        x.CreatedAt,

                        Company = new
                        {
                            x.CompanyProfile.CompanyProfileId,

                            x.CompanyProfile.CompanyName,

                            x.CompanyProfile.Industry,

                            x.CompanyProfile.Website,

                            x.CompanyProfile.Description,

                            x.CompanyProfile.Address,

                            x.CompanyProfile.VerificationStatus
                        },

                        Skills =
                            _context.JobSkills
                                .Where(js =>
                                    js.JobId ==
                                    x.JobId)
                                .OrderBy(js =>
                                    js.Importance ==
                                    "Required"
                                        ? 0
                                        : 1)
                                .ThenBy(js =>
                                    js.Skill.SkillName)
                                .Select(js => new
                                {
                                    js.SkillId,

                                    js.Skill.SkillName,

                                    js.Skill.Category,

                                    js.Importance,

                                    js.RequiredYearsOfExperience
                                })
                                .ToList()
                    })
                    .FirstOrDefaultAsync();


            if (job == null)
            {
                return NotFound(new
                {
                    message =
                        "Job not found."
                });
            }


            return Ok(job);
        }


        // =========================================
        // COMPANY - MY JOBS
        // =========================================

        [Authorize(Roles = "Company")]
        [HttpGet("company/me")]
        public async Task<IActionResult> GetMyJobs()
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


            var company =
                await _context.CompanyProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);


            if (company == null)
            {
                return NotFound(new
                {
                    message =
                        "Company profile not found."
                });
            }


            var jobs =
                await _context.Jobs
                    .Where(x =>
                        x.CompanyProfileId ==
                        company.CompanyProfileId)
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Select(x => new
                    {
                        x.JobId,

                        x.Title,

                        x.Category,

                        x.CareerRoleId,

                        CareerRoleName =
                            x.CareerRole != null
                                ? x.CareerRole.RoleName
                                : null,

                        CareerCategoryId =
                            x.CareerRole != null
                                ? x.CareerRole.CareerCategoryId
                                : (int?)null,

                        CareerCategoryName =
                            x.CareerRole != null
                                ? x.CareerRole
                                    .CareerCategory
                                    .CategoryName
                                : x.Category,

                        x.Location,

                        x.JobType,

                        x.ImagePath,

                        x.Status,

                        x.ClosingDate,

                        x.CreatedAt
                    })
                    .ToListAsync();


            return Ok(jobs);
        }


        // =========================================
        // COMPANY - CREATE JOB
        // =========================================

        [Authorize(Roles = "Company")]
        [HttpPost]
        public async Task<IActionResult> CreateJob(
            JobDto request)
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


            var company =
                await _context.CompanyProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId
                        &&
                        x.IsActive);


            if (company == null)
            {
                return NotFound(new
                {
                    message =
                        "Company profile not found."
                });
            }


            if (!string.Equals(
                    company.VerificationStatus,
                    "Approved",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message =
                        "Company must be approved before posting jobs."
                });
            }


            var careerRole =
                await GetActiveCareerRoleAsync(
                    request.CareerRoleId);


            if (careerRole == null)
            {
                return BadRequest(new
                {
                    message =
                        "Please select a valid active career role."
                });
            }


            var validationResult =
                await ValidateJobRequest(
                    request);


            if (validationResult != null)
            {
                return validationResult;
            }


            var job =
                new Job
                {
                    CompanyProfileId =
                        company.CompanyProfileId,

                    CareerRoleId =
                        careerRole.CareerRoleId,

                    Title =
                        request.Title.Trim(),

                    Description =
                        request.Description.Trim(),

                    Category =
                        careerRole
                            .CareerCategory
                            .CategoryName,

                    Location =
                        Clean(request.Location),

                    JobType =
                        Clean(request.JobType),

                    ImagePath =
                        Clean(request.ImagePath),

                    SalaryMin =
                        request.SalaryMin,

                    SalaryMax =
                        request.SalaryMax,

                    MinimumExperienceYears =
                        request.MinimumExperienceYears,

                    EducationRequirement =
                        Clean(
                            request
                                .EducationRequirement),

                    ClosingDate =
                        request.ClosingDate,

                    Status =
                        "Active",

                    CreatedAt =
                        DateTime.UtcNow
                };


            _context.Jobs.Add(job);

            await _context.SaveChangesAsync();


            foreach (var item in request.Skills)
            {
                _context.JobSkills.Add(
                    new JobSkill
                    {
                        JobId =
                            job.JobId,

                        SkillId =
                            item.SkillId,

                        Importance =
                            NormalizeImportance(
                                item.Importance),

                        RequiredYearsOfExperience =
                            item.RequiredYearsOfExperience
                    });
            }


            await _context.SaveChangesAsync();


            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    message =
                        "Job created successfully.",

                    job.JobId,

                    job.Title,

                    job.Category,

                    job.CareerRoleId,

                    job.ImagePath,

                    CareerRoleName =
                        careerRole.RoleName,

                    CareerCategoryName =
                        careerRole
                            .CareerCategory
                            .CategoryName,

                    job.Status
                });
        }


        // =========================================
        // COMPANY - UPDATE JOB
        // =========================================

        [Authorize(Roles = "Company")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJob(
            int id,
            JobDto request)
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


            var company =
                await _context.CompanyProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);


            if (company == null)
            {
                return NotFound(new
                {
                    message =
                        "Company profile not found."
                });
            }


            var job =
                await _context.Jobs
                    .FirstOrDefaultAsync(x =>
                        x.JobId == id
                        &&
                        x.CompanyProfileId ==
                        company.CompanyProfileId);


            if (job == null)
            {
                return NotFound(new
                {
                    message =
                        "Job not found."
                });
            }


            var careerRole =
                await GetActiveCareerRoleAsync(
                    request.CareerRoleId);


            if (careerRole == null)
            {
                return BadRequest(new
                {
                    message =
                        "Please select a valid active career role."
                });
            }


            var validationResult =
                await ValidateJobRequest(
                    request);


            if (validationResult != null)
            {
                return validationResult;
            }


            job.CareerRoleId =
                careerRole.CareerRoleId;

            job.Title =
                request.Title.Trim();

            job.Description =
                request.Description.Trim();

            job.Category =
                careerRole
                    .CareerCategory
                    .CategoryName;

            job.Location =
                Clean(request.Location);

            job.JobType =
                Clean(request.JobType);


            // Only replace image when
            // a new image path is supplied.

            if (!string.IsNullOrWhiteSpace(
                    request.ImagePath))
            {
                job.ImagePath =
                    request.ImagePath.Trim();
            }


            job.SalaryMin =
                request.SalaryMin;

            job.SalaryMax =
                request.SalaryMax;

            job.MinimumExperienceYears =
                request.MinimumExperienceYears;

            job.EducationRequirement =
                Clean(
                    request
                        .EducationRequirement);

            job.ClosingDate =
                request.ClosingDate;


            var existingSkills =
                await _context.JobSkills
                    .Where(x =>
                        x.JobId ==
                        job.JobId)
                    .ToListAsync();


            _context.JobSkills.RemoveRange(
                existingSkills);


            foreach (var item in request.Skills)
            {
                _context.JobSkills.Add(
                    new JobSkill
                    {
                        JobId =
                            job.JobId,

                        SkillId =
                            item.SkillId,

                        Importance =
                            NormalizeImportance(
                                item.Importance),

                        RequiredYearsOfExperience =
                            item.RequiredYearsOfExperience
                    });
            }


            await _context.SaveChangesAsync();


            return Ok(new
            {
                message =
                    "Job updated successfully.",

                job.JobId,

                job.Title,

                job.Category,

                job.CareerRoleId,

                job.ImagePath,

                CareerRoleName =
                    careerRole.RoleName,

                CareerCategoryName =
                    careerRole
                        .CareerCategory
                        .CategoryName
            });
        }


        // =========================================
        // COMPANY - CLOSE JOB
        // =========================================

        [Authorize(Roles = "Company")]
        [HttpPatch("{id}/close")]
        public async Task<IActionResult> CloseJob(
            int id)
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


            var company =
                await _context.CompanyProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);


            if (company == null)
            {
                return NotFound(new
                {
                    message =
                        "Company profile not found."
                });
            }


            var job =
                await _context.Jobs
                    .FirstOrDefaultAsync(x =>
                        x.JobId == id
                        &&
                        x.CompanyProfileId ==
                        company.CompanyProfileId);


            if (job == null)
            {
                return NotFound(new
                {
                    message =
                        "Job not found."
                });
            }


            if (job.Status == "Closed")
            {
                return BadRequest(new
                {
                    message =
                        "Job is already closed."
                });
            }


            job.Status =
                "Closed";


            await _context.SaveChangesAsync();


            return Ok(new
            {
                message =
                    "Job closed successfully."
            });
        }


        // =========================================
        // COMPANY - DELETE JOB
        // =========================================

        [Authorize(Roles = "Company")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJob(
            int id)
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


            var company =
                await _context.CompanyProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);


            if (company == null)
            {
                return NotFound(new
                {
                    message =
                        "Company profile not found."
                });
            }


            var job =
                await _context.Jobs
                    .FirstOrDefaultAsync(x =>
                        x.JobId == id
                        &&
                        x.CompanyProfileId ==
                        company.CompanyProfileId);


            if (job == null)
            {
                return NotFound(new
                {
                    message =
                        "Job not found."
                });
            }


            var hasApplications =
                await _context.JobApplications
                    .AnyAsync(x =>
                        x.JobId == id);


            if (hasApplications)
            {
                return BadRequest(new
                {
                    message =
                        "This job already has applications. Close the job instead of deleting it."
                });
            }


            var jobSkills =
                await _context.JobSkills
                    .Where(x =>
                        x.JobId == id)
                    .ToListAsync();


            _context.JobSkills.RemoveRange(
                jobSkills);


            _context.Jobs.Remove(job);


            await _context.SaveChangesAsync();


            return Ok(new
            {
                message =
                    "Job deleted successfully."
            });
        }


        // =========================================
        // ACTIVE CAREER ROLE
        // =========================================

        private async Task<CareerRole?>
            GetActiveCareerRoleAsync(
                int careerRoleId)
        {
            return await _context.CareerRoles
                .Include(x =>
                    x.CareerCategory)
                .FirstOrDefaultAsync(x =>
                    x.CareerRoleId ==
                    careerRoleId
                    &&
                    x.IsActive
                    &&
                    x.CareerCategory.IsActive);
        }


        // =========================================
        // VALIDATE JOB
        // =========================================

        private async Task<IActionResult?>
            ValidateJobRequest(
                JobDto request)
        {
            if (request.SalaryMin.HasValue
                &&
                request.SalaryMax.HasValue
                &&
                request.SalaryMin.Value >
                request.SalaryMax.Value)
            {
                return BadRequest(new
                {
                    message =
                        "Minimum salary cannot be greater than maximum salary."
                });
            }


            if (request.MinimumExperienceYears.HasValue
                &&
                request.MinimumExperienceYears.Value < 0)
            {
                return BadRequest(new
                {
                    message =
                        "Minimum experience cannot be negative."
                });
            }


            if (request.ClosingDate.HasValue
                &&
                request.ClosingDate.Value <=
                DateTime.UtcNow)
            {
                return BadRequest(new
                {
                    message =
                        "Closing date must be in the future."
                });
            }


            // =====================================
            // IMAGE PATH VALIDATION
            // =====================================

            if (!string.IsNullOrWhiteSpace(
                    request.ImagePath)
                &&
                !request.ImagePath.StartsWith(
                    "/uploads/jobs/",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message =
                        "Invalid job image path."
                });
            }


            request.Skills ??=
                new List<
                    JobSkillRequirementDto>();


            var duplicateSkills =
                request.Skills
                    .GroupBy(x =>
                        x.SkillId)
                    .Any(x =>
                        x.Count() > 1);


            if (duplicateSkills)
            {
                return BadRequest(new
                {
                    message =
                        "A skill cannot be added more than once to the same job."
                });
            }


            foreach (var item in request.Skills)
            {
                if (string.IsNullOrWhiteSpace(
                        item.Importance))
                {
                    return BadRequest(new
                    {
                        message =
                            "Skill importance is required."
                    });
                }


                if (!item.Importance.Equals(
                        "Required",
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    !item.Importance.Equals(
                        "Preferred",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        message =
                            "Skill importance must be Required or Preferred."
                    });
                }


                if (item.RequiredYearsOfExperience.HasValue
                    &&
                    item.RequiredYearsOfExperience.Value < 0)
                {
                    return BadRequest(new
                    {
                        message =
                            "Required skill experience cannot be negative."
                    });
                }
            }


            if (request.Skills.Count == 0)
            {
                return null;
            }


            var requestedSkillIds =
                request.Skills
                    .Select(x =>
                        x.SkillId)
                    .Distinct()
                    .ToList();


            var existingSkillIds =
                await _context.Skills
                    .Where(x =>
                        requestedSkillIds.Contains(
                            x.SkillId)
                        &&
                        x.IsActive)
                    .Select(x =>
                        x.SkillId)
                    .ToListAsync();


            if (existingSkillIds.Count !=
                requestedSkillIds.Count)
            {
                return BadRequest(new
                {
                    message =
                        "One or more selected skills are invalid."
                });
            }


            return null;
        }


        // =========================================
        // NORMALIZE IMPORTANCE
        // =========================================

        private static string NormalizeImportance(
            string importance)
        {
            return importance.Equals(
                    "Preferred",
                    StringComparison.OrdinalIgnoreCase)
                ? "Preferred"
                : "Required";
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