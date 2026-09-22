using WorkHub.API.Models;

namespace WorkHub.API.Interfaces
{
    public interface ICVIntelligenceService
    {
        Task<CVIntelligenceResult?>
            AnalyseAsync(
                int userId,
                int careerRoleId);
    }
}