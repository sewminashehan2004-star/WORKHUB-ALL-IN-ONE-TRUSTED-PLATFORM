using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkHub.API.Data;

namespace WorkHub.API.Controllers
{
    [Route("api/Admin/AuditLogs")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminAuditLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminAuditLogsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // GET AUDIT LOGS
        // =========================================

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs(
            string? action,
            string? entityType,
            int? userId)
        {
            var query =
                _context.AuditLogs
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(
                    action))
            {
                query =
                    query.Where(x =>
                        x.Action == action);
            }

            if (!string.IsNullOrWhiteSpace(
                    entityType))
            {
                query =
                    query.Where(x =>
                        x.EntityType ==
                        entityType);
            }

            if (userId.HasValue)
            {
                query =
                    query.Where(x =>
                        x.UserId ==
                        userId.Value);
            }

            var logs =
                await query
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Take(500)
                    .Select(x => new
                    {
                        x.AuditLogId,

                        x.Action,

                        x.EntityType,

                        x.EntityId,

                        x.Description,

                        x.IpAddress,

                        x.CreatedAt,

                        Admin = x.User == null
                            ? null
                            : new
                            {
                                x.User.UserId,
                                x.User.FullName,
                                x.User.Email
                            }
                    })
                    .ToListAsync();

            return Ok(logs);
        }

        // =========================================
        // GET ONE AUDIT LOG
        // =========================================

        [HttpGet("{auditLogId}")]
        public async Task<IActionResult> GetAuditLog(
            int auditLogId)
        {
            var log =
                await _context.AuditLogs
                    .Where(x =>
                        x.AuditLogId ==
                        auditLogId)
                    .Select(x => new
                    {
                        x.AuditLogId,

                        x.Action,

                        x.EntityType,

                        x.EntityId,

                        x.Description,

                        x.IpAddress,

                        x.CreatedAt,

                        Admin = x.User == null
                            ? null
                            : new
                            {
                                x.User.UserId,
                                x.User.FullName,
                                x.User.Email,
                                x.User.Role
                            }
                    })
                    .FirstOrDefaultAsync();

            if (log == null)
            {
                return NotFound(new
                {
                    message =
                        "Audit log not found."
                });
            }

            return Ok(log);
        }
    }
}