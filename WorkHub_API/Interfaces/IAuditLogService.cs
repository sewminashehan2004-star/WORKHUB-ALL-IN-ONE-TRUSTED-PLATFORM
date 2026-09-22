namespace WorkHub.API.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(
            string action,
            string entityType,
            string? entityId = null,
            string? description = null);
    }
}