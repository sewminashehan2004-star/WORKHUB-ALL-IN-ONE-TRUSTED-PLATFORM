using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkHub.API.Data;
using WorkHub.API.DTOs;
using WorkHub.API.Models;

namespace WorkHub.API.Controllers
{
    [Route("api/Admin/MasterData")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminMasterDataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminMasterDataController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // SKILLS - GET ALL
        // =====================================================

        [HttpGet("skills")]
        public async Task<IActionResult> GetSkills()
        {
            var skills =
                await _context.Skills
                    .OrderBy(x => x.SkillName)
                    .Select(x => new
                    {
                        x.SkillId,
                        x.SkillName,
                        x.Category,
                        x.IsActive,

                        JobUsageCount =
                            _context.JobSkills.Count(j =>
                                j.SkillId == x.SkillId),

                        JobSeekerUsageCount =
                            _context.JobSeekerSkills.Count(j =>
                                j.SkillId == x.SkillId)
                    })
                    .ToListAsync();

            return Ok(skills);
        }

        // =====================================================
        // SKILLS - CREATE
        // =====================================================

        [HttpPost("skills")]
        public async Task<IActionResult> CreateSkill(
            SkillAdminDto request)
        {
            var skillName =
                request.SkillName.Trim();

            var exists =
                await _context.Skills
                    .AnyAsync(x =>
                        x.SkillName.ToLower() ==
                        skillName.ToLower());

            if (exists)
            {
                return BadRequest(new
                {
                    message =
                        "This skill already exists."
                });
            }

            var skill =
                new Skill
                {
                    SkillName =
                        skillName,

                    Category =
                        request.Category?.Trim(),

                    IsActive =
                        request.IsActive
                };

            _context.Skills.Add(skill);

            await _context.SaveChangesAsync();

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    message =
                        "Skill created successfully.",

                    skill.SkillId,
                    skill.SkillName,
                    skill.Category,
                    skill.IsActive
                });
        }

        // =====================================================
        // SKILLS - UPDATE
        // =====================================================

        [HttpPut("skills/{skillId}")]
        public async Task<IActionResult> UpdateSkill(
            int skillId,
            SkillAdminDto request)
        {
            var skill =
                await _context.Skills
                    .FirstOrDefaultAsync(x =>
                        x.SkillId == skillId);

            if (skill == null)
            {
                return NotFound(new
                {
                    message =
                        "Skill not found."
                });
            }

            var skillName =
                request.SkillName.Trim();

            var duplicate =
                await _context.Skills
                    .AnyAsync(x =>
                        x.SkillId != skillId &&
                        x.SkillName.ToLower() ==
                        skillName.ToLower());

            if (duplicate)
            {
                return BadRequest(new
                {
                    message =
                        "Another skill with this name already exists."
                });
            }

            skill.SkillName =
                skillName;

            skill.Category =
                request.Category?.Trim();

            skill.IsActive =
                request.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Skill updated successfully.",

                skill.SkillId,
                skill.SkillName,
                skill.Category,
                skill.IsActive
            });
        }

        // =====================================================
        // SKILLS - DISABLE
        // =====================================================

        [HttpPatch("skills/{skillId}/disable")]
        public async Task<IActionResult> DisableSkill(
            int skillId)
        {
            var skill =
                await _context.Skills
                    .FirstOrDefaultAsync(x =>
                        x.SkillId == skillId);

            if (skill == null)
            {
                return NotFound(new
                {
                    message =
                        "Skill not found."
                });
            }

            skill.IsActive =
                false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Skill disabled successfully.",

                skill.SkillId,
                skill.SkillName,
                skill.IsActive
            });
        }

        // =====================================================
        // SKILLS - ENABLE
        // =====================================================

        [HttpPatch("skills/{skillId}/enable")]
        public async Task<IActionResult> EnableSkill(
            int skillId)
        {
            var skill =
                await _context.Skills
                    .FirstOrDefaultAsync(x =>
                        x.SkillId == skillId);

            if (skill == null)
            {
                return NotFound(new
                {
                    message =
                        "Skill not found."
                });
            }

            skill.IsActive =
                true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Skill enabled successfully.",

                skill.SkillId,
                skill.SkillName,
                skill.IsActive
            });
        }

        // =====================================================
        // SERVICE CATEGORIES - GET ALL
        // =====================================================

        [HttpGet("service-categories")]
        public async Task<IActionResult>
            GetServiceCategories()
        {
            var categories =
                await _context.ServiceCategories
                    .OrderBy(x =>
                        x.CategoryName)
                    .Select(x => new
                    {
                        x.ServiceCategoryId,
                        x.CategoryName,
                        x.Description,
                        x.IsActive,

                        OfferingCount =
                            _context.ServiceOfferings
                                .Count(o =>
                                    o.ServiceCategoryId ==
                                    x.ServiceCategoryId),

                        RequestCount =
                            _context.ServiceRequests
                                .Count(r =>
                                    r.ServiceCategoryId ==
                                    x.ServiceCategoryId)
                    })
                    .ToListAsync();

            return Ok(categories);
        }

        // =====================================================
        // SERVICE CATEGORY - CREATE
        // =====================================================

        [HttpPost("service-categories")]
        public async Task<IActionResult>
            CreateServiceCategory(
                ServiceCategoryAdminDto request)
        {
            var categoryName =
                request.CategoryName.Trim();

            var exists =
                await _context.ServiceCategories
                    .AnyAsync(x =>
                        x.CategoryName.ToLower() ==
                        categoryName.ToLower());

            if (exists)
            {
                return BadRequest(new
                {
                    message =
                        "This service category already exists."
                });
            }

            var category =
                new ServiceCategory
                {
                    CategoryName =
                        categoryName,

                    Description =
                        request.Description?.Trim(),

                    IsActive =
                        request.IsActive
                };

            _context.ServiceCategories.Add(
                category);

            await _context.SaveChangesAsync();

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    message =
                        "Service category created successfully.",

                    category.ServiceCategoryId,
                    category.CategoryName,
                    category.Description,
                    category.IsActive
                });
        }

        // =====================================================
        // SERVICE CATEGORY - UPDATE
        // =====================================================

        [HttpPut(
            "service-categories/{categoryId}")]
        public async Task<IActionResult>
            UpdateServiceCategory(
                int categoryId,
                ServiceCategoryAdminDto request)
        {
            var category =
                await _context.ServiceCategories
                    .FirstOrDefaultAsync(x =>
                        x.ServiceCategoryId ==
                        categoryId);

            if (category == null)
            {
                return NotFound(new
                {
                    message =
                        "Service category not found."
                });
            }

            var categoryName =
                request.CategoryName.Trim();

            var duplicate =
                await _context.ServiceCategories
                    .AnyAsync(x =>
                        x.ServiceCategoryId !=
                        categoryId &&
                        x.CategoryName.ToLower() ==
                        categoryName.ToLower());

            if (duplicate)
            {
                return BadRequest(new
                {
                    message =
                        "Another service category with this name already exists."
                });
            }

            category.CategoryName =
                categoryName;

            category.Description =
                request.Description?.Trim();

            category.IsActive =
                request.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Service category updated successfully.",

                category.ServiceCategoryId,
                category.CategoryName,
                category.Description,
                category.IsActive
            });
        }

        // =====================================================
        // SERVICE CATEGORY - DISABLE
        // =====================================================

        [HttpPatch(
            "service-categories/{categoryId}/disable")]
        public async Task<IActionResult>
            DisableServiceCategory(
                int categoryId)
        {
            var category =
                await _context.ServiceCategories
                    .FirstOrDefaultAsync(x =>
                        x.ServiceCategoryId ==
                        categoryId);

            if (category == null)
            {
                return NotFound(new
                {
                    message =
                        "Service category not found."
                });
            }

            category.IsActive =
                false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Service category disabled successfully.",

                category.ServiceCategoryId,
                category.CategoryName,
                category.IsActive
            });
        }

        // =====================================================
        // SERVICE CATEGORY - ENABLE
        // =====================================================

        [HttpPatch(
            "service-categories/{categoryId}/enable")]
        public async Task<IActionResult>
            EnableServiceCategory(
                int categoryId)
        {
            var category =
                await _context.ServiceCategories
                    .FirstOrDefaultAsync(x =>
                        x.ServiceCategoryId ==
                        categoryId);

            if (category == null)
            {
                return NotFound(new
                {
                    message =
                        "Service category not found."
                });
            }

            category.IsActive =
                true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Service category enabled successfully.",

                category.ServiceCategoryId,
                category.CategoryName,
                category.IsActive
            });
        }

        // =====================================================
        // MARKETPLACE CATEGORIES - GET ALL
        // =====================================================

        [HttpGet("marketplace-categories")]
        public async Task<IActionResult>
            GetMarketplaceCategories()
        {
            var categories =
                await _context.MarketplaceCategories
                    .OrderBy(x =>
                        x.CategoryName)
                    .Select(x => new
                    {
                        x.MarketplaceCategoryId,
                        x.CategoryName,
                        x.Description,
                        x.IsActive,

                        ListingCount =
                            _context.MarketplaceListings
                                .Count(l =>
                                    l.MarketplaceCategoryId ==
                                    x.MarketplaceCategoryId)
                    })
                    .ToListAsync();

            return Ok(categories);
        }

        // =====================================================
        // MARKETPLACE CATEGORY - CREATE
        // =====================================================

        [HttpPost("marketplace-categories")]
        public async Task<IActionResult>
            CreateMarketplaceCategory(
                MarketplaceCategoryAdminDto request)
        {
            var categoryName =
                request.CategoryName.Trim();

            var exists =
                await _context.MarketplaceCategories
                    .AnyAsync(x =>
                        x.CategoryName.ToLower() ==
                        categoryName.ToLower());

            if (exists)
            {
                return BadRequest(new
                {
                    message =
                        "This marketplace category already exists."
                });
            }

            var category =
                new MarketplaceCategory
                {
                    CategoryName =
                        categoryName,

                    Description =
                        request.Description?.Trim(),

                    IsActive =
                        request.IsActive
                };

            _context.MarketplaceCategories.Add(
                category);

            await _context.SaveChangesAsync();

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    message =
                        "Marketplace category created successfully.",

                    category.MarketplaceCategoryId,
                    category.CategoryName,
                    category.Description,
                    category.IsActive
                });
        }

        // =====================================================
        // MARKETPLACE CATEGORY - UPDATE
        // =====================================================

        [HttpPut(
            "marketplace-categories/{categoryId}")]
        public async Task<IActionResult>
            UpdateMarketplaceCategory(
                int categoryId,
                MarketplaceCategoryAdminDto request)
        {
            var category =
                await _context.MarketplaceCategories
                    .FirstOrDefaultAsync(x =>
                        x.MarketplaceCategoryId ==
                        categoryId);

            if (category == null)
            {
                return NotFound(new
                {
                    message =
                        "Marketplace category not found."
                });
            }

            var categoryName =
                request.CategoryName.Trim();

            var duplicate =
                await _context.MarketplaceCategories
                    .AnyAsync(x =>
                        x.MarketplaceCategoryId !=
                        categoryId &&
                        x.CategoryName.ToLower() ==
                        categoryName.ToLower());

            if (duplicate)
            {
                return BadRequest(new
                {
                    message =
                        "Another marketplace category with this name already exists."
                });
            }

            category.CategoryName =
                categoryName;

            category.Description =
                request.Description?.Trim();

            category.IsActive =
                request.IsActive;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Marketplace category updated successfully.",

                category.MarketplaceCategoryId,
                category.CategoryName,
                category.Description,
                category.IsActive
            });
        }

        // =====================================================
        // MARKETPLACE CATEGORY - DISABLE
        // =====================================================

        [HttpPatch(
            "marketplace-categories/{categoryId}/disable")]
        public async Task<IActionResult>
            DisableMarketplaceCategory(
                int categoryId)
        {
            var category =
                await _context.MarketplaceCategories
                    .FirstOrDefaultAsync(x =>
                        x.MarketplaceCategoryId ==
                        categoryId);

            if (category == null)
            {
                return NotFound(new
                {
                    message =
                        "Marketplace category not found."
                });
            }

            category.IsActive =
                false;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Marketplace category disabled successfully.",

                category.MarketplaceCategoryId,
                category.CategoryName,
                category.IsActive
            });
        }

        // =====================================================
        // MARKETPLACE CATEGORY - ENABLE
        // =====================================================

        [HttpPatch(
            "marketplace-categories/{categoryId}/enable")]
        public async Task<IActionResult>
            EnableMarketplaceCategory(
                int categoryId)
        {
            var category =
                await _context.MarketplaceCategories
                    .FirstOrDefaultAsync(x =>
                        x.MarketplaceCategoryId ==
                        categoryId);

            if (category == null)
            {
                return NotFound(new
                {
                    message =
                        "Marketplace category not found."
                });
            }

            category.IsActive =
                true;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Marketplace category enabled successfully.",

                category.MarketplaceCategoryId,
                category.CategoryName,
                category.IsActive
            });
        }
    }
}