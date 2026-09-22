using Microsoft.AspNetCore.Mvc;

namespace WorkHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetHealth()
        {
            return Ok(new
            {
                status = "success",
                message = "WorkHub API is running"
            });
        }
    }
}