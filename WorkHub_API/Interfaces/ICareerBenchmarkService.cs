using WorkHub.API.Models;

namespace WorkHub.API.Interfaces
{
    public interface ICareerBenchmarkService
    {
        Task<CareerBenchmarkResult?>
            BuildBenchmarkAsync(
                int careerRoleId);
    }
}