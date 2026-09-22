using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkHub.API.Data;

namespace WorkHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SkillsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSkills()
        {
            var skills = await _context.Skills
                .Where(x => x.IsActive)
                .OrderBy(x => x.SkillName)
                .Select(x => new
                {
                    x.SkillId,
                    x.SkillName,
                    x.Category
                })
                .ToListAsync();

            return Ok(skills);
        }
    }
}