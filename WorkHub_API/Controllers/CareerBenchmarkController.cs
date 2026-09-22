using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkHub.API.Interfaces;

namespace WorkHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CareerBenchmarkController
        : ControllerBase
    {
        private readonly ICareerBenchmarkService
            _careerBenchmarkService;


        public CareerBenchmarkController(
            ICareerBenchmarkService
                careerBenchmarkService)
        {
            _careerBenchmarkService =
                careerBenchmarkService;
        }


        // =========================================
        // GET CAREER BENCHMARK
        // =========================================
        //
        // GET:
        // api/CareerBenchmark/role/1
        //
        // =========================================

        [HttpGet("role/{careerRoleId}")]
        public async Task<IActionResult>
            GetCareerBenchmark(
                int careerRoleId)
        {
            if (careerRoleId <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "Please provide a valid career role."
                });
            }


            var result =
                await _careerBenchmarkService
                    .BuildBenchmarkAsync(
                        careerRoleId);


            if (result == null)
            {
                return NotFound(new
                {
                    message =
                        "Career role not found."
                });
            }


            return Ok(new
            {
                result.CareerRoleId,

                result.CareerCategoryId,

                result.RoleName,

                result.CategoryName,


                market = new
                {
                    result.ActiveJobCount,

                    result.DataSource,

                    result.ConfidenceLevel
                },


                requirements = new
                {
                    averageMinimumExperienceYears =
                        result
                            .AverageMinimumExperienceYears,

                    education =
                        result
                            .CommonEducationRequirements,

                    skills =
                        result.Skills
                },


                result.GeneratedAt
            });
        }
    }
}