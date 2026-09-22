using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using WorkHub.API.Data;
using WorkHub.API.DTOs;
using WorkHub.API.Interfaces;
using WorkHub.API.Models;

namespace WorkHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobApplicationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICVMatchingService _matchingService;

        public JobApplicationsController(
            ApplicationDbContext context,
            ICVMatchingService matchingService)
        {
            _context = context;
            _matchingService = matchingService;
        }

        // ============================================
        // JOB SEEKER - APPLY FOR JOB
        // ============================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpPost("job/{jobId}/apply")]
        public async Task<IActionResult> ApplyForJob(
            int jobId,
            ApplyJobDto request)
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var profile =
                await _context.JobSeekerProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId &&
                        x.IsActive);

            if (profile == null)
            {
                return BadRequest(new
                {
                    message =
                        "Activate your Job Seeker profile before applying for jobs."
                });
            }

            var job =
                await _context.Jobs
                    .FirstOrDefaultAsync(x =>
                        x.JobId == jobId);

            if (job == null)
            {
                return NotFound(new
                {
                    message = "Job not found."
                });
            }

            if (!string.Equals(
                    job.Status,
                    "Active",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message =
                        "This job is no longer accepting applications."
                });
            }

            if (job.ClosingDate.HasValue &&
                job.ClosingDate.Value < DateTime.UtcNow)
            {
                return BadRequest(new
                {
                    message =
                        "The closing date for this job has passed."
                });
            }

            var alreadyApplied =
                await _context.JobApplications
                    .AnyAsync(x =>
                        x.JobId == jobId &&
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId);

            if (alreadyApplied)
            {
                return BadRequest(new
                {
                    message =
                        "You have already applied for this job."
                });
            }

            CVDocument? selectedCV;

            if (request.CVDocumentId.HasValue)
            {
                selectedCV =
                    await _context.CVDocuments
                        .FirstOrDefaultAsync(x =>
                            x.CVDocumentId ==
                            request.CVDocumentId.Value &&
                            x.JobSeekerProfileId ==
                            profile.JobSeekerProfileId);

                if (selectedCV == null)
                {
                    return BadRequest(new
                    {
                        message =
                            "The selected CV does not belong to your account."
                    });
                }
            }
            else
            {
                selectedCV =
                    await _context.CVDocuments
                        .Where(x =>
                            x.JobSeekerProfileId ==
                            profile.JobSeekerProfileId)
                        .OrderByDescending(x => x.IsPrimary)
                        .ThenByDescending(x => x.UploadedAt)
                        .FirstOrDefaultAsync();
            }

            if (selectedCV == null)
            {
                return BadRequest(new
                {
                    message =
                        "Please upload a CV before applying for a job."
                });
            }

            // Validate screening answers before creating the application so a
            // candidate never ends up with a half-completed application.
            var questions =
                await _context.JobScreeningQuestions
                    .AsNoTracking()
                    .Where(x => x.JobId == jobId)
                    .OrderBy(x => x.DisplayOrder)
                    .ToListAsync();

            var submittedAnswers =
                request.ScreeningAnswers
                ?? new List<ApplyJobScreeningAnswerDto>();

            var answerMap =
                submittedAnswers
                    .Where(x => x.QuestionId > 0)
                    .GroupBy(x => x.QuestionId)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Last().Answer?.Trim() ?? string.Empty);

            foreach (var question in questions)
            {
                answerMap.TryGetValue(
                    question.JobScreeningQuestionId,
                    out var answerText);

                if (question.IsRequired &&
                    string.IsNullOrWhiteSpace(answerText))
                {
                    return BadRequest(new
                    {
                        message =
                            $"Please answer required question: {question.QuestionText}"
                    });
                }

                if (string.IsNullOrWhiteSpace(answerText))
                {
                    continue;
                }

                if (string.Equals(
                        question.QuestionType,
                        "YesNo",
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(answerText, "Yes", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(answerText, "No", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        message =
                            $"Please select Yes or No for: {question.QuestionText}"
                    });
                }

                if (string.Equals(
                        question.QuestionType,
                        "MultipleChoice",
                        StringComparison.OrdinalIgnoreCase))
                {
                    var options =
                        string.IsNullOrWhiteSpace(question.OptionsJson)
                            ? new List<string>()
                            : JsonSerializer.Deserialize<List<string>>(
                                  question.OptionsJson)
                              ?? new List<string>();

                    if (!options.Any(option =>
                            string.Equals(
                                option,
                                answerText,
                                StringComparison.OrdinalIgnoreCase)))
                    {
                        return BadRequest(new
                        {
                            message =
                                $"Please select a valid answer for: {question.QuestionText}"
                        });
                    }
                }
            }

            var application =
                new JobApplication
                {
                    JobId = job.JobId,
                    JobSeekerProfileId =
                        profile.JobSeekerProfileId,
                    CVDocumentId =
                        selectedCV.CVDocumentId,
                    CoverLetter =
                        string.IsNullOrWhiteSpace(request.CoverLetter)
                            ? null
                            : request.CoverLetter.Trim(),
                    Status = "Received",
                    AppliedAt = DateTime.UtcNow
                };

            await using (
                var transaction =
                    await _context.Database
                        .BeginTransactionAsync())
            {
                _context.JobApplications.Add(application);
                await _context.SaveChangesAsync();

                foreach (var question in questions)
                {
                    if (!answerMap.TryGetValue(
                            question.JobScreeningQuestionId,
                            out var answerText) ||
                        string.IsNullOrWhiteSpace(answerText))
                    {
                        continue;
                    }

                    _context.JobScreeningAnswers.Add(
                        new JobScreeningAnswer
                        {
                            JobApplicationId =
                                application.JobApplicationId,
                            JobScreeningQuestionId =
                                question.JobScreeningQuestionId,
                            AnswerText = answerText,
                            SubmittedAt = DateTime.UtcNow
                        });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }

            // Matching is useful for the employer, but an analysis problem must
            // not undo a valid candidate application.
            CVMatchResult? matchResult = null;

            try
            {
                matchResult =
                    await _matchingService
                        .CalculateAndSaveAsync(
                            application.JobApplicationId);
            }
            catch
            {
                // The application itself has already been saved successfully.
            }

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    message =
                        "Job application submitted successfully.",

                    application.JobApplicationId,
                    application.JobId,
                    application.Status,
                    application.CVDocumentId,
                    application.AppliedAt,

                    match = matchResult == null
                        ? null
                        : new
                        {
                            matchResult.OverallScore,
                            matchResult.SkillsScore,
                            matchResult.ExperienceScore,
                            matchResult.EducationScore,
                            matchResult.PreferredSkillsScore,
                            matchResult.RelevanceScore,
                            matchResult.LocationScore,
                            matchResult.MatchedSkills,
                            matchResult.MissingSkills,
                            matchResult.Recommendations,
                            matchResult.CalculatedAt
                        }
                });
        }

        // ============================================
        // JOB SEEKER - MY APPLICATIONS
        // ============================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyApplications()
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

            var profile =
                await _context.JobSeekerProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);

            if (profile == null)
            {
                return NotFound(new
                {
                    message =
                        "Job Seeker profile not found."
                });
            }

            var applications =
                await _context.JobApplications
                    .Where(x =>
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId)
                    .OrderByDescending(x =>
                        x.AppliedAt)
                    .Select(x => new
                    {
                        x.JobApplicationId,

                        x.JobId,

                        JobTitle =
                            x.Job.Title,

                        CompanyName =
                            x.Job.CompanyProfile.CompanyName,

                        x.Status,

                        x.AppliedAt,

                        x.UpdatedAt,

                        CVFileName =
                            x.CVDocument != null
                                ? x.CVDocument.FileName
                                : null,

                        MatchScore =
                            _context.CVMatchResults
                                .Where(m =>
                                    m.JobApplicationId ==
                                    x.JobApplicationId)
                                .Select(m =>
                                    (decimal?)m.OverallScore)
                                .FirstOrDefault()
                    })
                    .ToListAsync();

            return Ok(applications);
        }

        // ============================================
        // JOB SEEKER - APPLICATION DETAILS
        // ============================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpGet("me/{applicationId}")]
        public async Task<IActionResult> GetMyApplication(
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

            var profile =
                await _context.JobSeekerProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);

            if (profile == null)
            {
                return NotFound(new
                {
                    message =
                        "Job Seeker profile not found."
                });
            }

            var application =
                await _context.JobApplications
                    .Where(x =>
                        x.JobApplicationId ==
                        applicationId &&
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId)
                    .Select(x => new
                    {
                        x.JobApplicationId,

                        x.Status,

                        x.CoverLetter,

                        x.AppliedAt,

                        x.UpdatedAt,

                        Job = new
                        {
                            x.Job.JobId,
                            x.Job.Title,
                            x.Job.Location,
                            x.Job.JobType,
                            x.Job.Category
                        },

                        Company = new
                        {
                            x.Job.CompanyProfile
                                .CompanyProfileId,

                            x.Job.CompanyProfile
                                .CompanyName
                        },

                        CV = x.CVDocument == null
                            ? null
                            : new
                            {
                                x.CVDocument
                                    .CVDocumentId,

                                x.CVDocument
                                    .FileName
                            },

                        MatchResult =
                            _context.CVMatchResults
                                .Where(m =>
                                    m.JobApplicationId ==
                                    x.JobApplicationId)
                                .Select(m => new
                                {
                                    m.OverallScore,
                                    m.SkillsScore,
                                    m.ExperienceScore,
                                    m.EducationScore,
                                    m.PreferredSkillsScore,
                                    m.RelevanceScore,
                                    m.LocationScore,
                                    m.MatchedSkills,
                                    m.MissingSkills,
                                    m.Recommendations,
                                    m.CalculatedAt
                                })
                                .FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();

            if (application == null)
            {
                return NotFound(new
                {
                    message =
                        "Application not found."
                });
            }

            return Ok(application);
        }

        // ============================================
        // JOB SEEKER - WITHDRAW APPLICATION
        // ============================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpPatch("me/{applicationId}/withdraw")]
        public async Task<IActionResult> WithdrawApplication(
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

            var profile =
                await _context.JobSeekerProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId == userId);

            if (profile == null)
            {
                return NotFound(new
                {
                    message =
                        "Job Seeker profile not found."
                });
            }

            var application =
                await _context.JobApplications
                    .FirstOrDefaultAsync(x =>
                        x.JobApplicationId ==
                        applicationId &&
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId);

            if (application == null)
            {
                return NotFound(new
                {
                    message =
                        "Application not found."
                });
            }

            if (application.Status == "Hired")
            {
                return BadRequest(new
                {
                    message =
                        "A hired application cannot be withdrawn."
                });
            }

            application.Status =
                "Withdrawn";

            application.UpdatedAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Application withdrawn successfully."
            });
        }

        // ============================================
        // COMPANY - GET APPLICANTS FOR MY JOB
        // ============================================

        [Authorize(Roles = "Company")]
        [HttpGet("company/job/{jobId}")]
        public async Task<IActionResult> GetJobApplicants(
            int jobId)
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
                        x.JobId == jobId &&
                        x.CompanyProfileId ==
                        company.CompanyProfileId);

            if (job == null)
            {
                return NotFound(new
                {
                    message =
                        "Job not found or does not belong to your company."
                });
            }

            var applicants =
                await _context.JobApplications
                    .Where(x =>
                        x.JobId == jobId)
                    .OrderByDescending(x =>
                        _context.CVMatchResults
                            .Where(m =>
                                m.JobApplicationId ==
                                x.JobApplicationId)
                            .Select(m =>
                                (decimal?)m.OverallScore)
                            .FirstOrDefault())
                    .ThenByDescending(x =>
                        x.AppliedAt)
                    .Select(x => new
                    {
                        x.JobApplicationId,

                        x.Status,

                        x.AppliedAt,

                        Candidate = new
                        {
                            x.JobSeekerProfile
                                .JobSeekerProfileId,

                            x.JobSeekerProfile
                                .User.FullName,

                            x.JobSeekerProfile
                                .ProfessionalSummary,

                            x.JobSeekerProfile
                                .CurrentLocation
                        },

                        MatchScore =
                            _context.CVMatchResults
                                .Where(m =>
                                    m.JobApplicationId ==
                                    x.JobApplicationId)
                                .Select(m =>
                                    (decimal?)m.OverallScore)
                                .FirstOrDefault()
                    })
                    .ToListAsync();

            return Ok(new
            {
                job = new
                {
                    job.JobId,
                    job.Title
                },

                totalApplicants =
                    applicants.Count,

                applicants
            });
        }

        // ============================================
        // COMPANY - GET ONE APPLICANT
        // ============================================

        [Authorize(Roles = "Company")]
        [HttpGet("company/application/{applicationId}")]
        public async Task<IActionResult> GetApplicantDetails(
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

            var application =
                await _context.JobApplications
                    .Where(x =>
                        x.JobApplicationId ==
                        applicationId &&
                        x.Job.CompanyProfileId ==
                        company.CompanyProfileId)
                    .Select(x => new
                    {
                        x.JobApplicationId,

                        x.Status,

                        x.CoverLetter,

                        x.AppliedAt,

                        x.UpdatedAt,

                        Job = new
                        {
                            x.Job.JobId,
                            x.Job.Title
                        },

                        Candidate = new
                        {
                            x.JobSeekerProfile
                                .JobSeekerProfileId,

                            x.JobSeekerProfile
                                .User.FullName,

                            x.JobSeekerProfile
                                .User.Email,

                            x.JobSeekerProfile
                                .ProfessionalSummary,

                            x.JobSeekerProfile
                                .PreferredJobCategory,

                            x.JobSeekerProfile
                                .CurrentLocation,

                            x.JobSeekerProfile
                                .ExpectedSalary
                        },

                        Skills =
                            _context.JobSeekerSkills
                                .Where(s =>
                                    s.JobSeekerProfileId ==
                                    x.JobSeekerProfileId)
                                .Select(s => new
                                {
                                    s.Skill.SkillName,
                                    s.ProficiencyLevel,
                                    s.YearsOfExperience
                                })
                                .ToList(),

                        Experience =
                            _context.JobSeekerExperiences
                                .Where(e =>
                                    e.JobSeekerProfileId ==
                                    x.JobSeekerProfileId)
                                .Select(e => new
                                {
                                    e.JobTitle,
                                    e.CompanyName,
                                    e.StartDate,
                                    e.EndDate,
                                    e.IsCurrentJob
                                })
                                .ToList(),

                        Education =
                            _context.JobSeekerEducations
                                .Where(e =>
                                    e.JobSeekerProfileId ==
                                    x.JobSeekerProfileId)
                                .Select(e => new
                                {
                                    e.InstitutionName,
                                    e.Qualification,
                                    e.FieldOfStudy
                                })
                                .ToList(),

                        MatchResult =
                            _context.CVMatchResults
                                .Where(m =>
                                    m.JobApplicationId ==
                                    x.JobApplicationId)
                                .Select(m => new
                                {
                                    m.OverallScore,
                                    m.SkillsScore,
                                    m.ExperienceScore,
                                    m.EducationScore,
                                    m.PreferredSkillsScore,
                                    m.RelevanceScore,
                                    m.LocationScore,
                                    m.MatchedSkills,
                                    m.MissingSkills,
                                    m.Recommendations,
                                    m.CalculatedAt
                                })
                                .FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();

            if (application == null)
            {
                return NotFound(new
                {
                    message =
                        "Application not found."
                });
            }

            return Ok(application);
        }

        // ============================================
        // COMPANY - UPDATE APPLICATION STATUS
        // ============================================

        [Authorize(Roles = "Company")]
        [HttpPatch("company/application/{applicationId}/status")]
        public async Task<IActionResult> UpdateApplicationStatus(
            int applicationId,
            UpdateApplicationStatusDto request)
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

            var application =
                await _context.JobApplications
                    .Include(x => x.Job)
                    .Include(x => x.JobSeekerProfile)
                    .FirstOrDefaultAsync(x =>
                        x.JobApplicationId ==
                        applicationId &&
                        x.Job.CompanyProfileId ==
                        company.CompanyProfileId);

            if (application == null)
            {
                return NotFound(new
                {
                    message =
                        "Application not found."
                });
            }

            if (application.Status == "Withdrawn")
            {
                return BadRequest(new
                {
                    message =
                        "A withdrawn application cannot be updated."
                });
            }

            var allowedStatuses =
                new[]
                {
                    "Received",
                    "Under Review",
                    "Shortlisted",
                    "Interview",
                    "Offered",
                    "Hired",
                    "Rejected"
                };

            var newStatus =
                allowedStatuses
                    .FirstOrDefault(x =>
                        x.Equals(
                            request.Status.Trim(),
                            StringComparison.OrdinalIgnoreCase));

            if (newStatus == null)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid application status."
                });
            }

            application.Status =
                newStatus;

            application.UpdatedAt =
                DateTime.UtcNow;

            _context.UserNotifications.Add(
                new UserNotification
                {
                    UserId = application.JobSeekerProfile.UserId,
                    Title = "Job application update",
                    Message = $"Your application for {application.Job.Title} is now {newStatus}.",
                    NotificationType = "JobApplication",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Application status updated successfully.",

                application.JobApplicationId,

                application.Status,

                application.UpdatedAt
            });
        }
    }
}