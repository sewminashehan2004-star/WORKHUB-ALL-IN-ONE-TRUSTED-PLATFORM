using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkHub.API.Data;
using WorkHub.API.DTOs;
using WorkHub.API.Models;

namespace WorkHub.API.Controllers
{
    [Route("api/jobseeker/cv")]
    [ApiController]
    [Authorize(Roles = "RegisteredUser")]
    public class CVController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        private const long MaxFileSize = 5 * 1024 * 1024;

        private readonly string[] _allowedExtensions =
        {
            ".pdf",
            ".docx"
        };

        private readonly string[] _allowedContentTypes =
        {
            "application/pdf",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };

        public CVController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // =========================================
        // UPLOAD CV
        // =========================================

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadCV(
            [FromForm] UploadCVDto request)
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
                    message =
                        "Job Seeker profile not found. Activate Job Seeker profile first."
                });
            }

            if (request.File == null ||
                request.File.Length == 0)
            {
                return BadRequest(new
                {
                    message = "Please select a CV file."
                });
            }

            if (request.File.Length > MaxFileSize)
            {
                return BadRequest(new
                {
                    message =
                        "CV file size cannot exceed 5 MB."
                });
            }

            var extension =
                Path.GetExtension(request.File.FileName)
                    .ToLowerInvariant();

            if (!_allowedExtensions.Contains(extension))
            {
                return BadRequest(new
                {
                    message =
                        "Only PDF and DOCX files are allowed."
                });
            }

            if (!_allowedContentTypes.Contains(
                    request.File.ContentType))
            {
                return BadRequest(new
                {
                    message =
                        "Invalid CV file type."
                });
            }

            var existingCVs =
                await _context.CVDocuments
                    .Where(x =>
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId)
                    .ToListAsync();

            var shouldBePrimary =
                request.IsPrimary ||
                !existingCVs.Any();

            if (shouldBePrimary)
            {
                foreach (var cv in existingCVs)
                {
                    cv.IsPrimary = false;
                }
            }

            var storedFileName =
                $"{Guid.NewGuid()}{extension}";

            var uploadFolder =
                Path.Combine(
                    _environment.ContentRootPath,
                    "Uploads",
                    "CVs"
                );

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var physicalFilePath =
                Path.Combine(
                    uploadFolder,
                    storedFileName
                );

            await using (var stream =
                new FileStream(
                    physicalFilePath,
                    FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            var cvDocument = new CVDocument
            {
                JobSeekerProfileId =
                    profile.JobSeekerProfileId,

                FileName =
                    Path.GetFileName(
                        request.File.FileName),

                StoredFileName =
                    storedFileName,

                FilePath =
                    Path.Combine(
                        "Uploads",
                        "CVs",
                        storedFileName),

                FileType =
                    extension,

                FileSize =
                    request.File.Length,

                IsPrimary =
                    shouldBePrimary,

                UploadedAt =
                    DateTime.UtcNow
            };

            _context.CVDocuments.Add(cvDocument);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "CV uploaded successfully.",

                cv = new
                {
                    cvDocument.CVDocumentId,
                    cvDocument.FileName,
                    cvDocument.FileType,
                    cvDocument.FileSize,
                    cvDocument.IsPrimary,
                    cvDocument.UploadedAt
                }
            });
        }

        // =========================================
        // GET MY CVS
        // =========================================

        [HttpGet]
        public async Task<IActionResult> GetMyCVs()
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
                    message =
                        "Job Seeker profile not found."
                });
            }

            var cvs =
                await _context.CVDocuments
                    .Where(x =>
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId)
                    .OrderByDescending(x => x.IsPrimary)
                    .ThenByDescending(x => x.UploadedAt)
                    .Select(x => new
                    {
                        x.CVDocumentId,
                        x.FileName,
                        x.FileType,
                        x.FileSize,
                        x.IsPrimary,
                        x.UploadedAt
                    })
                    .ToListAsync();

            return Ok(cvs);
        }

        // =========================================
        // SET PRIMARY CV
        // =========================================

        [HttpPut("{id}/primary")]
        public async Task<IActionResult> SetPrimaryCV(
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
                    message =
                        "Job Seeker profile not found."
                });
            }

            var cvs =
                await _context.CVDocuments
                    .Where(x =>
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId)
                    .ToListAsync();

            var selectedCV =
                cvs.FirstOrDefault(x =>
                    x.CVDocumentId == id);

            if (selectedCV == null)
            {
                return NotFound(new
                {
                    message = "CV not found."
                });
            }

            foreach (var cv in cvs)
            {
                cv.IsPrimary =
                    cv.CVDocumentId == id;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Primary CV updated successfully."
            });
        }

        // =========================================
        // DOWNLOAD CV
        // =========================================

        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadCV(
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
                    message =
                        "Job Seeker profile not found."
                });
            }

            var cv =
                await _context.CVDocuments
                    .FirstOrDefaultAsync(x =>
                        x.CVDocumentId == id &&
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId);

            if (cv == null)
            {
                return NotFound(new
                {
                    message = "CV not found."
                });
            }

            var physicalFilePath =
                Path.Combine(
                    _environment.ContentRootPath,
                    cv.FilePath
                );

            if (!System.IO.File.Exists(
                    physicalFilePath))
            {
                return NotFound(new
                {
                    message =
                        "CV file is missing from the server."
                });
            }

            var contentType =
                cv.FileType == ".pdf"
                    ? "application/pdf"
                    : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

            var fileBytes =
                await System.IO.File.ReadAllBytesAsync(
                    physicalFilePath);

            return File(
                fileBytes,
                contentType,
                cv.FileName
            );
        }

        // =========================================
        // DELETE CV
        // =========================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCV(
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
                    message =
                        "Job Seeker profile not found."
                });
            }

            var cv =
                await _context.CVDocuments
                    .FirstOrDefaultAsync(x =>
                        x.CVDocumentId == id &&
                        x.JobSeekerProfileId ==
                        profile.JobSeekerProfileId);

            if (cv == null)
            {
                return NotFound(new
                {
                    message = "CV not found."
                });
            }

            var usedInApplication =
                await _context.JobApplications
                    .AnyAsync(x =>
                        x.CVDocumentId == id);

            if (usedInApplication)
            {
                return BadRequest(new
                {
                    message =
                        "This CV has already been used in a job application and cannot be deleted."
                });
            }

            var wasPrimary =
                cv.IsPrimary;

            var physicalFilePath =
                Path.Combine(
                    _environment.ContentRootPath,
                    cv.FilePath
                );

            _context.CVDocuments.Remove(cv);

            await _context.SaveChangesAsync();

            if (System.IO.File.Exists(
                    physicalFilePath))
            {
                System.IO.File.Delete(
                    physicalFilePath);
            }

            if (wasPrimary)
            {
                var nextCV =
                    await _context.CVDocuments
                        .Where(x =>
                            x.JobSeekerProfileId ==
                            profile.JobSeekerProfileId)
                        .OrderByDescending(x =>
                            x.UploadedAt)
                        .FirstOrDefaultAsync();

                if (nextCV != null)
                {
                    nextCV.IsPrimary = true;

                    await _context.SaveChangesAsync();
                }
            }

            return Ok(new
            {
                message =
                    "CV deleted successfully."
            });
        }
    }
}