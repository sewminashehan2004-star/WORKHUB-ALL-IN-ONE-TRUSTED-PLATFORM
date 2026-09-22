using WorkHub.API.Models;

namespace WorkHub.API.Interfaces
{
    public interface ICVMatchingService
    {
        Task<CVMatchResult?> CalculateAndSaveAsync(
            int applicationId);
    }
}