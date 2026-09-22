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
    public class CVTextController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICVTextExtractionService _textExtractionService;
        private readonly IWebHostEnvironment _environment;

        public CVTextController(
            ApplicationDbContext context,
            ICVTextExtractionService textExtractionService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _textExtractionService = textExtractionService;
            _environment = environment;
        }

        [HttpGet("{cvId}/preview")]
        public async Task<IActionResult> PreviewCVText(int cvId)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
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
                    message = "Job Seeker profile not found."
                });
            }

            var cv =
                await _context.CVDocuments
                    .FirstOrDefaultAsync(x =>
                        x.CVDocumentId == cvId &&
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId);

            if (cv == null)
            {
                return NotFound(new
                {
                    message = "CV not found."
                });
            }

            var physicalPath =
                Path.Combine(
                    _environment.ContentRootPath,
                    cv.FilePath
                );

            var extractedText =
                await _textExtractionService
                    .ExtractTextAsync(physicalPath);

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                return BadRequest(new
                {
                    message =
                        "No readable text could be extracted from this CV."
                });
            }

            var preview =
                extractedText.Length > 3000
                    ? extractedText.Substring(0, 3000)
                    : extractedText;

            return Ok(new
            {
                cv.CVDocumentId,
                cv.FileName,
                characterCount = extractedText.Length,
                preview
            });
        }
    }
}