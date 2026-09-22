using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using WorkHub.API.Data;
using WorkHub.API.Models;

namespace WorkHub.API.Controllers
{
    [ApiController]
    [Route("api/JobScreening")]
    public class JobScreeningController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JobScreeningController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Company")]
        [HttpGet("company/job/{jobId:int}")]
        public async Task<IActionResult> CompanyQuestions(int jobId)
        {
            if (!await CompanyOwnsJob(jobId))
                return NotFound(new { message = "Job not found for this company." });

            return Ok(await GetQuestionList(jobId));
        }

        [Authorize(Roles = "Company")]
        [HttpPost("company/job/{jobId:int}")]
        public async Task<IActionResult> AddQuestion(int jobId, CreateScreeningQuestionRequest request)
        {
            if (!await CompanyOwnsJob(jobId))
                return NotFound(new { message = "Job not found for this company." });

            if (request == null || string.IsNullOrWhiteSpace(request.QuestionText))
                return BadRequest(new { message = "Question text is required." });

            var normalizedType = NormalizeType(request.QuestionType);
            var options = request.Options?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(10)
                .ToList() ?? new List<string>();

            if ((normalizedType == "YesNo" || normalizedType == "Text"))
                options.Clear();

            if (normalizedType == "MultipleChoice" && options.Count < 2)
                return BadRequest(new { message = "Multiple choice questions need at least two options." });

            var order = await _context.JobScreeningQuestions
                .Where(x => x.JobId == jobId)
                .Select(x => (int?)x.DisplayOrder)
                .MaxAsync() ?? 0;

            var question = new JobScreeningQuestion
            {
                JobId = jobId,
                QuestionText = request.QuestionText.Trim(),
                QuestionType = normalizedType,
                OptionsJson = options.Count == 0 ? null : JsonSerializer.Serialize(options),
                IsRequired = request.IsRequired,
                DisplayOrder = order + 1,
                CreatedAt = DateTime.UtcNow
            };

            _context.JobScreeningQuestions.Add(question);
            await _context.SaveChangesAsync();

            return StatusCode(StatusCodes.Status201Created, new
            {
                message = "Screening question added.",
                question.JobScreeningQuestionId
            });
        }

        [Authorize(Roles = "Company")]
        [HttpDelete("company/question/{questionId:int}")]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            var question = await _context.JobScreeningQuestions
                .FirstOrDefaultAsync(x => x.JobScreeningQuestionId == questionId);

            if (question == null || !await CompanyOwnsJob(question.JobId))
                return NotFound(new { message = "Question not found." });

            var hasAnswers = await _context.JobScreeningAnswers
                .AnyAsync(x => x.JobScreeningQuestionId == questionId);

            if (hasAnswers)
                return BadRequest(new { message = "This question already has candidate answers and cannot be deleted." });

            _context.JobScreeningQuestions.Remove(question);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Question deleted." });
        }

        [Authorize(Roles = "RegisteredUser")]
        [HttpGet("job/{jobId:int}")]
        public async Task<IActionResult> CandidateQuestions(int jobId)
        {
            var exists = await _context.Jobs.AnyAsync(x => x.JobId == jobId && x.Status == "Active");
            if (!exists) return NotFound(new { message = "Job not found." });
            return Ok(await GetQuestionList(jobId));
        }

        [Authorize(Roles = "RegisteredUser")]
        [HttpPost("application/{applicationId:int}/answers")]
        public async Task<IActionResult> SubmitAnswers(int applicationId, SubmitScreeningAnswersRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var application = await _context.JobApplications
                .Where(x => x.JobApplicationId == applicationId && x.JobSeekerProfile.UserId == userId.Value)
                .Select(x => new { x.JobApplicationId, x.JobId })
                .FirstOrDefaultAsync();

            if (application == null)
                return NotFound(new { message = "Application not found." });

            var questions = await _context.JobScreeningQuestions
                .Where(x => x.JobId == application.JobId)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            var answers = request?.Answers ?? new List<ScreeningAnswerRequest>();
            var answerMap = answers
                .GroupBy(x => x.QuestionId)
                .ToDictionary(x => x.Key, x => x.Last().Answer?.Trim() ?? string.Empty);

            foreach (var question in questions)
            {
                answerMap.TryGetValue(question.JobScreeningQuestionId, out var answerText);
                if (question.IsRequired && string.IsNullOrWhiteSpace(answerText))
                    return BadRequest(new { message = $"Please answer required question: {question.QuestionText}" });

                if (string.IsNullOrWhiteSpace(answerText)) continue;

                var existing = await _context.JobScreeningAnswers
                    .FirstOrDefaultAsync(x => x.JobApplicationId == applicationId &&
                                              x.JobScreeningQuestionId == question.JobScreeningQuestionId);

                if (existing == null)
                {
                    _context.JobScreeningAnswers.Add(new JobScreeningAnswer
                    {
                        JobApplicationId = applicationId,
                        JobScreeningQuestionId = question.JobScreeningQuestionId,
                        AnswerText = answerText,
                        SubmittedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    existing.AnswerText = answerText;
                    existing.SubmittedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Screening answers submitted successfully." });
        }

        [Authorize(Roles = "Company")]
        [HttpGet("company/application/{applicationId:int}/answers")]
        public async Task<IActionResult> CompanyCandidateAnswers(int applicationId)
        {
            var application = await _context.JobApplications
                .Where(x => x.JobApplicationId == applicationId)
                .Select(x => new { x.JobId })
                .FirstOrDefaultAsync();

            if (application == null || !await CompanyOwnsJob(application.JobId))
                return NotFound(new { message = "Application not found." });

            var answers = await _context.JobScreeningAnswers
                .Where(x => x.JobApplicationId == applicationId)
                .OrderBy(x => x.JobScreeningQuestion.DisplayOrder)
                .Select(x => new
                {
                    x.JobScreeningQuestionId,
                    x.JobScreeningQuestion.QuestionText,
                    x.JobScreeningQuestion.QuestionType,
                    x.AnswerText,
                    x.SubmittedAt
                })
                .ToListAsync();

            return Ok(answers);
        }

        private async Task<bool> CompanyOwnsJob(int jobId)
        {
            var userId = GetUserId();
            if (userId == null) return false;

            return await _context.Jobs.AnyAsync(x =>
                x.JobId == jobId &&
                x.CompanyProfile.UserId == userId.Value);
        }

        private async Task<List<object>> GetQuestionList(int jobId)
        {
            var questions = await _context.JobScreeningQuestions
                .Where(x => x.JobId == jobId)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();

            return questions.Select(x => (object)new
            {
                x.JobScreeningQuestionId,
                x.QuestionText,
                x.QuestionType,
                Options = string.IsNullOrWhiteSpace(x.OptionsJson)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(x.OptionsJson) ?? new List<string>(),
                x.IsRequired,
                x.DisplayOrder
            }).ToList();
        }

        private int? GetUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }

        private static string NormalizeType(string? value)
        {
            if (string.Equals(value, "YesNo", StringComparison.OrdinalIgnoreCase)) return "YesNo";
            if (string.Equals(value, "MultipleChoice", StringComparison.OrdinalIgnoreCase)) return "MultipleChoice";
            return "Text";
        }

        public class CreateScreeningQuestionRequest
        {
            [Required, MaxLength(500)]
            public string QuestionText { get; set; } = string.Empty;
            public string QuestionType { get; set; } = "Text";
            public List<string> Options { get; set; } = new();
            public bool IsRequired { get; set; } = true;
        }

        public class SubmitScreeningAnswersRequest
        {
            public List<ScreeningAnswerRequest> Answers { get; set; } = new();
        }

        public class ScreeningAnswerRequest
        {
            public int QuestionId { get; set; }
            public string? Answer { get; set; }
        }
    }
}
