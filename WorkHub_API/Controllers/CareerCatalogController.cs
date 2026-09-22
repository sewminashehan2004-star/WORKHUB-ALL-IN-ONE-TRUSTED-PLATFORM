using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkHub.API.Data;

namespace WorkHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CareerCatalogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CareerCatalogController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // GET ALL ACTIVE CAREER CATEGORIES
        // GET: api/CareerCatalog/categories
        // =========================================

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories =
                await _context.CareerCategories
                    .Where(x => x.IsActive)
                    .OrderBy(x => x.CategoryName)
                    .Select(x => new
                    {
                        x.CareerCategoryId,
                        x.CategoryName,
                        x.Description
                    })
                    .ToListAsync();


            return Ok(categories);
        }


        // =========================================
        // GET ROLES BY CATEGORY
        // GET: api/CareerCatalog/categories/1/roles
        // =========================================

        [HttpGet("categories/{categoryId}/roles")]
        public async Task<IActionResult> GetRolesByCategory(
            int categoryId)
        {
            var category =
                await _context.CareerCategories
                    .FirstOrDefaultAsync(x =>
                        x.CareerCategoryId == categoryId &&
                        x.IsActive);


            if (category == null)
            {
                return NotFound(new
                {
                    message =
                        "Career category not found."
                });
            }


            var roles =
                await _context.CareerRoles
                    .Where(x =>
                        x.CareerCategoryId == categoryId &&
                        x.IsActive)
                    .OrderBy(x => x.RoleName)
                    .Select(x => new
                    {
                        x.CareerRoleId,
                        x.RoleName,
                        x.Description,

                        x.BaselineExperienceYears,

                        x.BaselineEducationRequirement
                    })
                    .ToListAsync();


            return Ok(new
            {
                category = new
                {
                    category.CareerCategoryId,
                    category.CategoryName
                },

                roles
            });
        }


        // =========================================
        // GET ONE CAREER ROLE
        // GET: api/CareerCatalog/roles/1
        // =========================================

        [HttpGet("roles/{roleId}")]
        public async Task<IActionResult> GetCareerRole(
            int roleId)
        {
            var role =
                await _context.CareerRoles
                    .Where(x =>
                        x.CareerRoleId == roleId &&
                        x.IsActive)
                    .Select(x => new
                    {
                        x.CareerRoleId,

                        x.CareerCategoryId,

                        CategoryName =
                            x.CareerCategory.CategoryName,

                        x.RoleName,

                        x.Description,

                        x.BaselineExperienceYears,

                        x.BaselineEducationRequirement,

                        Skills =
                            x.CareerRoleSkills
                                .OrderBy(x =>
                                    x.Importance == "Required"
                                        ? 0
                                        : 1)
                                .ThenBy(x =>
                                    x.Skill.SkillName)
                                .Select(x => new
                                {
                                    x.CareerRoleSkillId,

                                    x.SkillId,

                                    x.Skill.SkillName,

                                    x.Skill.Category,

                                    x.Importance,

                                    x.RecommendedYearsOfExperience
                                })
                                .ToList()
                    })
                    .FirstOrDefaultAsync();


            if (role == null)
            {
                return NotFound(new
                {
                    message =
                        "Career role not found."
                });
            }


            return Ok(role);
        }


        // =========================================
        // SEARCH CAREER ROLES
        // GET: api/CareerCatalog/search?query=network
        // =========================================

        [HttpGet("search")]
        public async Task<IActionResult> SearchCareerRoles(
            string? query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Ok(
                    Array.Empty<object>());
            }


            query =
                query.Trim();


            var roles =
                await _context.CareerRoles
                    .Where(x =>
                        x.IsActive &&
                        (
                            x.RoleName.Contains(query)
                            ||
                            x.CareerCategory
                                .CategoryName
                                .Contains(query)
                        ))
                    .OrderBy(x => x.RoleName)
                    .Take(20)
                    .Select(x => new
                    {
                        x.CareerRoleId,

                        x.RoleName,

                        x.CareerCategoryId,

                        CategoryName =
                            x.CareerCategory.CategoryName
                    })
                    .ToListAsync();


            return Ok(roles);
        }
    }
}