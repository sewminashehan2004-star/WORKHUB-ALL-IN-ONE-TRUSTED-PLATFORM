using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using WorkHub.API.Data;
using WorkHub.API.Interfaces;
using WorkHub.API.Models;

namespace WorkHub.API.Controllers
{
    [ApiController]
    [Route("api/Payments")]
    [Authorize(Roles = "RegisteredUser")]
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICVIntelligenceService _cvIntelligence;

        public PaymentsController(ApplicationDbContext context, ICVIntelligenceService cvIntelligence)
        {
            _context = context;
            _cvIntelligence = cvIntelligence;
        }

        [AllowAnonymous]
        [HttpGet("cv-plans")]
        public IActionResult GetPlans()
        {
            return Ok(new[]
            {
                new
                {
                    code = "MATCH_ONLY",
                    name = "CV Match",
                    price = 2.00m,
                    currency = "USD",
                    description = "Job-specific CV match percentage and readiness score.",
                    includesRecommendations = false
                },
                new
                {
                    code = "MATCH_PLUS",
                    name = "CV Match + Insights",
                    price = 5.00m,
                    currency = "USD",
                    description = "Match score plus missing skills, CV issues and improvement recommendations.",
                    includesRecommendations = true
                }
            });
        }

        [HttpPost("cv-analysis")]
        public async Task<IActionResult> PurchaseCvAnalysis(PurchaseCvAnalysisRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var plan = ResolvePlan(request.PlanCode);
            if (plan == null)
                return BadRequest(new { message = "Please select a valid CV analysis plan." });

            var job = await _context.Jobs
                .Where(x => x.JobId == request.JobId && x.Status == "Active")
                .Select(x => new { x.JobId, x.Title, x.CareerRoleId })
                .FirstOrDefaultAsync();

            if (job == null)
                return NotFound(new { message = "Job not found." });

            if (!job.CareerRoleId.HasValue)
                return BadRequest(new { message = "This job is not linked to a career role yet." });

            var profile = await _context.JobSeekerProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId.Value && x.IsActive);

            if (profile == null)
                return BadRequest(new { message = "Activate your Job Seeker profile before analysing a CV." });

            var cv = await _context.CVDocuments
                .Where(x => x.JobSeekerProfileId == profile.JobSeekerProfileId)
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.UploadedAt)
                .FirstOrDefaultAsync();

            if (cv == null)
                return BadRequest(new { message = "Upload a CV before purchasing CV analysis." });

            var cardResult = ValidateDemoPayment(request.Payment);
            if (!cardResult.Success)
                return BadRequest(new { message = cardResult.Message });

            var result = await _cvIntelligence.AnalyseAsync(userId.Value, job.CareerRoleId.Value);
            if (result == null)
                return BadRequest(new { message = "WorkHub could not analyse this CV." });

            var reference = $"WH-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
            var purchase = new CVAnalysisPurchase
            {
                UserId = userId.Value,
                JobId = job.JobId,
                CVDocumentId = cv.CVDocumentId,
                PlanCode = plan.Code,
                Amount = plan.Price,
                Currency = "USD",
                PaymentStatus = "Paid",
                PaymentReference = reference,
                PaymentMethod = "Card",
                CardLast4 = cardResult.Last4,
                MatchPercentage = result.OverallScore,
                MissingSkills = plan.IncludesRecommendations
                    ? JsonSerializer.Serialize(result.Skills.Where(x => x.MatchStatus == "Missing").Select(x => x.SkillName).ToList())
                    : null,
                Recommendations = plan.IncludesRecommendations
                    ? JsonSerializer.Serialize(result.Recommendations)
                    : null,
                PurchasedAt = DateTime.UtcNow
            };

            _context.CVAnalysisPurchases.Add(purchase);
            _context.UserNotifications.Add(new UserNotification
            {
                UserId = userId.Value,
                Title = "CV analysis ready",
                Message = $"Your {plan.Name} report for {job.Title} is ready. Match score: {result.OverallScore:0.#}%.",
                NotificationType = "CVAnalysis",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Payment approved and CV analysis completed.",
                payment = new
                {
                    reference,
                    amount = plan.Price,
                    currency = "USD",
                    method = "Card",
                    last4 = cardResult.Last4,
                    status = "Paid"
                },
                plan = new { plan.Code, plan.Name, plan.IncludesRecommendations },
                job = new { job.JobId, job.Title },
                report = new
                {
                    matchPercentage = result.OverallScore,
                    result.ReadinessLevel,
                    scoreBreakdown = new
                    {
                        result.SkillsScore,
                        result.ExperienceScore,
                        result.EducationScore,
                        result.CvQualityScore,
                        result.CareerRelevanceScore
                    },
                    missingSkills = plan.IncludesRecommendations
                        ? result.Skills.Where(x => x.MatchStatus == "Missing").Select(x => x.SkillName).ToList()
                        : null,
                    cvIssues = plan.IncludesRecommendations ? result.CVIssues : null,
                    recommendations = plan.IncludesRecommendations ? result.Recommendations : null
                }
            });
        }

        [HttpGet("cv-analysis/history")]
        public async Task<IActionResult> History()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var items = await _context.CVAnalysisPurchases
                .Where(x => x.UserId == userId.Value)
                .OrderByDescending(x => x.PurchasedAt)
                .Select(x => new
                {
                    x.CVAnalysisPurchaseId,
                    x.PlanCode,
                    x.Amount,
                    x.Currency,
                    x.PaymentStatus,
                    x.PaymentReference,
                    x.CardLast4,
                    x.MatchPercentage,
                    x.PurchasedAt,
                    Job = new { x.JobId, x.Job.Title }
                })
                .ToListAsync();

            return Ok(items);
        }

        private int? GetUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }

        private static Plan? ResolvePlan(string? code)
        {
            if (string.Equals(code, "MATCH_ONLY", StringComparison.OrdinalIgnoreCase))
                return new Plan("MATCH_ONLY", "CV Match", 2.00m, false);
            if (string.Equals(code, "MATCH_PLUS", StringComparison.OrdinalIgnoreCase))
                return new Plan("MATCH_PLUS", "CV Match + Insights", 5.00m, true);
            return null;
        }

        private static PaymentValidationResult ValidateDemoPayment(CardPaymentRequest payment)
        {
            if (payment == null)
                return new(false, "Payment details are required.", null);

            var digits = new string((payment.CardNumber ?? string.Empty).Where(char.IsDigit).ToArray());
            if (digits.Length < 13 || digits.Length > 19 || !PassesLuhn(digits))
                return new(false, "Please enter a valid card number.", null);

            if (string.IsNullOrWhiteSpace(payment.CardHolderName))
                return new(false, "Cardholder name is required.", null);

            if (payment.ExpiryMonth < 1 || payment.ExpiryMonth > 12)
                return new(false, "Expiry month is invalid.", null);

            var now = DateTime.UtcNow;
            if (payment.ExpiryYear < now.Year || (payment.ExpiryYear == now.Year && payment.ExpiryMonth < now.Month))
                return new(false, "This card is expired.", null);

            var cvv = new string((payment.Cvv ?? string.Empty).Where(char.IsDigit).ToArray());
            if (cvv.Length is < 3 or > 4)
                return new(false, "CVV is invalid.", null);

            return new(true, "Approved", digits[^4..]);
        }

        private static bool PassesLuhn(string digits)
        {
            var sum = 0;
            var alternate = false;
            for (var i = digits.Length - 1; i >= 0; i--)
            {
                var n = digits[i] - '0';
                if (alternate)
                {
                    n *= 2;
                    if (n > 9) n -= 9;
                }
                sum += n;
                alternate = !alternate;
            }
            return sum % 10 == 0;
        }

        private sealed record Plan(string Code, string Name, decimal Price, bool IncludesRecommendations);
        private sealed record PaymentValidationResult(bool Success, string Message, string? Last4);

        public class PurchaseCvAnalysisRequest
        {
            [Range(1, int.MaxValue)]
            public int JobId { get; set; }

            [Required]
            public string PlanCode { get; set; } = "MATCH_ONLY";

            [Required]
            public CardPaymentRequest Payment { get; set; } = new();
        }

        public class CardPaymentRequest
        {
            [Required]
            public string CardHolderName { get; set; } = string.Empty;
            [Required]
            public string CardNumber { get; set; } = string.Empty;
            public int ExpiryMonth { get; set; }
            public int ExpiryYear { get; set; }
            [Required]
            public string Cvv { get; set; } = string.Empty;
        }
    }
}
