using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkHub.API.Data;

namespace WorkHub.API.Controllers
{
    [ApiController]
    [Route("api/Admin/CompanyApprovals")]
    [Authorize(Roles = "Admin")]
    public class AdminCompanyApprovalsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;


        public AdminCompanyApprovalsController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // GET ALL PENDING COMPANY REQUESTS
        //
        // GET:
        // api/Admin/CompanyApprovals/pending
        // =========================================

        [HttpGet("pending")]
        public async Task<IActionResult>
            GetPendingCompanies()
        {
            var companies =
                await _context.CompanyProfiles
                    .AsNoTracking()
                    .Include(x => x.User)
                    .Where(x =>
                        x.VerificationStatus == "Pending"
                        &&
                        x.User.AccountStatus == "Pending")
                    .OrderByDescending(x =>
                        x.CreatedAt)
                    .Select(x => new
                    {
                        x.CompanyProfileId,

                        x.UserId,

                        x.CompanyName,

                        ContactPerson =
                            x.User.FullName,

                        Email =
                            x.User.Email,

                        Phone =
                            x.User.Phone,

                        Location =
                            x.User.Location,

                        x.Industry,

                        x.Website,

                        x.Description,

                        Address =
                            x.Address,

                        AccountStatus =
                            x.User.AccountStatus,

                        x.VerificationStatus,

                        x.IsActive,

                        SubmittedAt =
                            x.CreatedAt
                    })
                    .ToListAsync();


            return Ok(new
            {
                count =
                    companies.Count,

                companies
            });
        }


        // =========================================
        // GET ONE COMPANY REQUEST
        //
        // GET:
        // api/Admin/CompanyApprovals/5
        // =========================================

        [HttpGet("{companyProfileId:int}")]
        public async Task<IActionResult>
            GetCompanyRequest(
                int companyProfileId)
        {
            var company =
                await _context.CompanyProfiles
                    .AsNoTracking()
                    .Include(x => x.User)
                    .Where(x =>
                        x.CompanyProfileId ==
                        companyProfileId)
                    .Select(x => new
                    {
                        x.CompanyProfileId,

                        x.UserId,

                        x.CompanyName,

                        ContactPerson =
                            x.User.FullName,

                        Email =
                            x.User.Email,

                        Phone =
                            x.User.Phone,

                        Location =
                            x.User.Location,

                        x.Industry,

                        x.Website,

                        x.Description,

                        x.Address,

                        x.LogoPath,

                        AccountStatus =
                            x.User.AccountStatus,

                        x.VerificationStatus,

                        x.IsActive,

                        UserCreatedAt =
                            x.User.CreatedAt,

                        CompanyCreatedAt =
                            x.CreatedAt
                    })
                    .FirstOrDefaultAsync();


            if (company == null)
            {
                return NotFound(new
                {
                    message =
                        "Company registration request not found."
                });
            }


            return Ok(company);
        }


        // =========================================
        // APPROVE COMPANY
        //
        // POST:
        // api/Admin/CompanyApprovals/5/approve
        //
        // User:
        // Pending → Active
        //
        // CompanyProfile:
        // Pending → Approved
        // IsActive → true
        // =========================================

        [HttpPost("{companyProfileId:int}/approve")]
        public async Task<IActionResult>
            ApproveCompany(
                int companyProfileId)
        {
            var company =
                await _context.CompanyProfiles
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x =>
                        x.CompanyProfileId ==
                        companyProfileId);


            if (company == null)
            {
                return NotFound(new
                {
                    message =
                        "Company registration request not found."
                });
            }


            if (company.VerificationStatus.Equals(
                    "Approved",
                    StringComparison.OrdinalIgnoreCase)
                &&
                company.User.AccountStatus.Equals(
                    "Active",
                    StringComparison.OrdinalIgnoreCase)
                &&
                company.IsActive)
            {
                return BadRequest(new
                {
                    message =
                        "This company account is already approved."
                });
            }


            if (company.VerificationStatus.Equals(
                    "Rejected",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message =
                        "This company registration request has already been rejected."
                });
            }


            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();


            try
            {
                // =================================
                // ACTIVATE LOGIN ACCOUNT
                // =================================

                company.User.AccountStatus =
                    "Active";


                // =================================
                // APPROVE COMPANY PROFILE
                // =================================

                company.VerificationStatus =
                    "Approved";


                company.IsActive =
                    true;


                await _context.SaveChangesAsync();


                await transaction.CommitAsync();


                return Ok(new
                {
                    message =
                        "Company registration approved successfully.",

                    company.CompanyProfileId,

                    company.CompanyName,

                    accountStatus =
                        company.User.AccountStatus,

                    verificationStatus =
                        company.VerificationStatus,

                    company.IsActive
                });
            }
            catch
            {
                await transaction.RollbackAsync();


                return StatusCode(
                    StatusCodes
                        .Status500InternalServerError,
                    new
                    {
                        message =
                            "Unable to approve the company registration."
                    });
            }
        }


        // =========================================
        // REJECT COMPANY
        //
        // POST:
        // api/Admin/CompanyApprovals/5/reject
        //
        // User:
        // Pending → Rejected
        //
        // CompanyProfile:
        // Pending → Rejected
        // IsActive → false
        // =========================================

        [HttpPost("{companyProfileId:int}/reject")]
        public async Task<IActionResult>
            RejectCompany(
                int companyProfileId)
        {
            var company =
                await _context.CompanyProfiles
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x =>
                        x.CompanyProfileId ==
                        companyProfileId);


            if (company == null)
            {
                return NotFound(new
                {
                    message =
                        "Company registration request not found."
                });
            }


            if (company.VerificationStatus.Equals(
                    "Approved",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message =
                        "An approved company cannot be rejected from this registration request."
                });
            }


            if (company.VerificationStatus.Equals(
                    "Rejected",
                    StringComparison.OrdinalIgnoreCase)
                &&
                company.User.AccountStatus.Equals(
                    "Rejected",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message =
                        "This company registration request is already rejected."
                });
            }


            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();


            try
            {
                company.User.AccountStatus =
                    "Rejected";


                company.VerificationStatus =
                    "Rejected";


                company.IsActive =
                    false;


                await _context.SaveChangesAsync();


                await transaction.CommitAsync();


                return Ok(new
                {
                    message =
                        "Company registration rejected.",

                    company.CompanyProfileId,

                    company.CompanyName,

                    accountStatus =
                        company.User.AccountStatus,

                    verificationStatus =
                        company.VerificationStatus,

                    company.IsActive
                });
            }
            catch
            {
                await transaction.RollbackAsync();


                return StatusCode(
                    StatusCodes
                        .Status500InternalServerError,
                    new
                    {
                        message =
                            "Unable to reject the company registration."
                    });
            }
        }
    }
}