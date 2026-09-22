using System.Security.Claims;
using WorkHub.API.Data;
using WorkHub.API.Interfaces;
using WorkHub.API.Models;

namespace WorkHub.API.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor =
                httpContextAccessor;
        }

        public async Task LogAsync(
            string action,
            string entityType,
            string? entityId = null,
            string? description = null)
        {
            int? userId = null;

            var httpContext =
                _httpContextAccessor.HttpContext;

            if (httpContext != null)
            {
                var userIdValue =
                    httpContext.User.FindFirstValue(
                        ClaimTypes.NameIdentifier);

                if (int.TryParse(
                        userIdValue,
                        out var parsedUserId))
                {
                    userId =
                        parsedUserId;
                }
            }

            var ipAddress =
                httpContext?
                    .Connection
                    .RemoteIpAddress?
                    .ToString();

            var auditLog =
                new AuditLog
                {
                    UserId =
                        userId,

                    Action =
                        action,

                    EntityType =
                        entityType,

                    EntityId =
                        entityId,

                    Description =
                        description,

                    IpAddress =
                        ipAddress,

                    CreatedAt =
                        DateTime.UtcNow
                };

            _context.AuditLogs.Add(
                auditLog);

            await _context.SaveChangesAsync();
        }
    }
}