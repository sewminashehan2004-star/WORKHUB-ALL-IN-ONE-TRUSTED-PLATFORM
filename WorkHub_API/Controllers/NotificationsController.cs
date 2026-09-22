using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkHub.API.Data;

namespace WorkHub.API.Controllers
{
    [ApiController]
    [Route("api/Notifications")]
    [Authorize(Roles = "RegisteredUser")]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMine(bool unreadOnly = false, int take = 50)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            take = Math.Clamp(take, 1, 100);
            var query = _context.UserNotifications.Where(x => x.UserId == userId.Value);
            if (unreadOnly) query = query.Where(x => !x.IsRead);

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(take)
                .Select(x => new
                {
                    x.UserNotificationId,
                    x.Title,
                    x.Message,
                    x.NotificationType,
                    x.RelatedInquiryId,
                    x.IsRead,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                unreadCount = await _context.UserNotifications.CountAsync(x => x.UserId == userId.Value && !x.IsRead),
                notifications = items
            });
        }

        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var item = await _context.UserNotifications
                .FirstOrDefaultAsync(x => x.UserNotificationId == id && x.UserId == userId.Value);

            if (item == null) return NotFound(new { message = "Notification not found." });
            item.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Notification marked as read." });
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var items = await _context.UserNotifications
                .Where(x => x.UserId == userId.Value && !x.IsRead)
                .ToListAsync();

            foreach (var item in items) item.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = "All notifications marked as read." });
        }

        private int? GetUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : null;
        }
    }
}
