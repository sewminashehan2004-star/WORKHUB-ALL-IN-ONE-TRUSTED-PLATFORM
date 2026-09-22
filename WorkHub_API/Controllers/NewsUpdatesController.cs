using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkHub.API.Data;
using WorkHub.API.Models;

namespace WorkHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsUpdatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NewsUpdatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // PUBLIC - GET PUBLISHED NEWS
        // GET: api/NewsUpdates/public
        // =====================================================

        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublishedNews()
        {
            var news = await _context.NewsUpdates
                .AsNoTracking()
                .Where(x => x.IsPublished)
                .OrderByDescending(x => x.PublishedAt ?? x.CreatedAt)
                .ToListAsync();

            return Ok(news);
        }

        // =====================================================
        // PUBLIC - GET ONE PUBLISHED NEWS
        // GET: api/NewsUpdates/public/5
        // =====================================================

        [HttpGet("public/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublishedNewsById(int id)
        {
            var news = await _context.NewsUpdates
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.NewsUpdateId == id &&
                    x.IsPublished);

            if (news == null)
            {
                return NotFound(new
                {
                    message = "News item not found."
                });
            }

            return Ok(news);
        }

        // =====================================================
        // ADMIN - GET ALL NEWS
        // GET: api/NewsUpdates/admin
        // =====================================================

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllForAdmin()
        {
            var news = await _context.NewsUpdates
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(news);
        }

        // =====================================================
        // ADMIN - GET ONE NEWS
        // GET: api/NewsUpdates/admin/5
        // =====================================================

        [HttpGet("admin/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetForAdmin(int id)
        {
            var news = await _context.NewsUpdates
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.NewsUpdateId == id);

            if (news == null)
            {
                return NotFound(new
                {
                    message = "News item not found."
                });
            }

            return Ok(news);
        }

        // =====================================================
        // ADMIN - CREATE NEWS
        // POST: api/NewsUpdates/admin
        // =====================================================

        [HttpPost("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [FromBody] NewsUpdateSaveRequest request)
        {
            var validationError = ValidateRequest(request);

            if (validationError != null)
            {
                return BadRequest(new
                {
                    message = validationError
                });
            }

            var adminName =
                User.FindFirstValue(ClaimTypes.Name)
                ??
                User.FindFirstValue("name")
                ??
                "WorkHub Admin";

            var now = DateTime.UtcNow;

            var news = new NewsUpdate
            {
                Title = request.Title,
                Summary = string.IsNullOrWhiteSpace(request.Summary)
                    ? null
                    : request.Summary,

                Content = request.Content,

                Category = string.IsNullOrWhiteSpace(request.Category)
                    ? "General"
                    : request.Category,

                IsPublished = request.IsPublished,

                PublishedAt = request.IsPublished
                    ? now
                    : null,

                CreatedAt = now,

                UpdatedAt = null,

                CreatedBy = adminName
            };

            _context.NewsUpdates.Add(news);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = request.IsPublished
                    ? "News published successfully."
                    : "News saved as draft successfully.",

                newsUpdateId = news.NewsUpdateId
            });
        }

        // =====================================================
        // ADMIN - UPDATE NEWS
        // PUT: api/NewsUpdates/admin/5
        // =====================================================

        [HttpPut("admin/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] NewsUpdateSaveRequest request)
        {
            var validationError = ValidateRequest(request);

            if (validationError != null)
            {
                return BadRequest(new
                {
                    message = validationError
                });
            }

            var news = await _context.NewsUpdates
                .FirstOrDefaultAsync(x => x.NewsUpdateId == id);

            if (news == null)
            {
                return NotFound(new
                {
                    message = "News item not found."
                });
            }

            news.Title = request.Title;

            news.Summary = string.IsNullOrWhiteSpace(request.Summary)
                ? null
                : request.Summary;

            news.Content = request.Content;

            news.Category = string.IsNullOrWhiteSpace(request.Category)
                ? "General"
                : request.Category;

            // Draft -> Published
            if (request.IsPublished && !news.IsPublished)
            {
                news.PublishedAt = DateTime.UtcNow;
            }

            // Published -> Draft
            if (!request.IsPublished)
            {
                news.PublishedAt = null;
            }

            news.IsPublished = request.IsPublished;
            news.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "News item updated successfully."
            });
        }

        // =====================================================
        // ADMIN - DELETE NEWS
        // DELETE: api/NewsUpdates/admin/5
        // =====================================================

        [HttpDelete("admin/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var news = await _context.NewsUpdates
                .FirstOrDefaultAsync(x => x.NewsUpdateId == id);

            if (news == null)
            {
                return NotFound(new
                {
                    message = "News item not found."
                });
            }

            _context.NewsUpdates.Remove(news);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "News item deleted successfully."
            });
        }

        // =====================================================
        // VALIDATION
        // =====================================================

        private static string? ValidateRequest(
            NewsUpdateSaveRequest request)
        {
            request.Title = request.Title?.Trim() ?? string.Empty;
            request.Summary = request.Summary?.Trim() ?? string.Empty;
            request.Content = request.Content?.Trim() ?? string.Empty;
            request.Category = request.Category?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return "News title is required.";
            }

            if (request.Title.Length > 200)
            {
                return "News title cannot exceed 200 characters.";
            }

            if (request.Summary.Length > 500)
            {
                return "Summary cannot exceed 500 characters.";
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return "News content is required.";
            }

            if (string.IsNullOrWhiteSpace(request.Category))
            {
                request.Category = "General";
            }

            if (request.Category.Length > 100)
            {
                return "Category cannot exceed 100 characters.";
            }

            return null;
        }

        // =====================================================
        // REQUEST MODEL
        // =====================================================

        public class NewsUpdateSaveRequest
        {
            public string Title { get; set; } = string.Empty;

            public string Summary { get; set; } = string.Empty;

            public string Content { get; set; } = string.Empty;

            public string Category { get; set; } = "General";

            public bool IsPublished { get; set; }
        }
    }
}