using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkHub.API.Data;
using WorkHub.API.DTOs;
using WorkHub.API.Interfaces;
using WorkHub.API.Models;

namespace WorkHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;


        public AuthController(
            ApplicationDbContext context,
            IPasswordHasher<User> passwordHasher,
            ITokenService tokenService,
            IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _configuration = configuration;
        }


        // =====================================
        // NORMAL USER REGISTER
        //
        // POST:
        // /api/Auth/register
        //
        // IMPORTANT:
        // Company accounts are NOT created here.
        // =====================================

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            RegisterRequestDto request)
        {
            var email =
                request.Email
                    .Trim()
                    .ToLowerInvariant();


            // =================================
            // COMPANY MUST USE COMPANY REQUEST
            // =================================

            if (request.AccountType.Equals(
                    "Company",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message =
                        "Company accounts must be submitted through the Company Registration Request."
                });
            }


            if (!request.AccountType.Equals(
                    "RegisteredUser",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message =
                        "Account type must be RegisteredUser."
                });
            }


            // =================================
            // EMAIL CHECK
            // =================================

            var emailExists =
                await _context.Users.AnyAsync(
                    x => x.Email == email);


            if (emailExists)
            {
                return BadRequest(new
                {
                    message =
                        "An account with this email already exists."
                });
            }


            // =================================
            // CREATE NORMAL USER
            // =================================

            var user =
                new User
                {
                    FullName =
                        request.FullName.Trim(),

                    Email =
                        email,

                    Phone =
                        string.IsNullOrWhiteSpace(
                            request.Phone)
                            ? null
                            : request.Phone.Trim(),

                    Location =
                        string.IsNullOrWhiteSpace(
                            request.Location)
                            ? null
                            : request.Location.Trim(),

                    Role =
                        "RegisteredUser",

                    AccountStatus =
                        "Active",

                    CreatedAt =
                        DateTime.UtcNow
                };


            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    request.Password);


            _context.Users.Add(user);

            await _context.SaveChangesAsync();


            var response =
                new RegisterResponseDto
                {
                    UserId =
                        user.UserId,

                    FullName =
                        user.FullName,

                    Email =
                        user.Email,

                    Role =
                        user.Role,

                    Message =
                        "WorkHub account registered successfully."
                };


            return StatusCode(
                StatusCodes.Status201Created,
                response);
        }


        // =====================================
        // COMPANY REGISTRATION REQUEST
        //
        // POST:
        // /api/Auth/company-register
        //
        // Creates:
        //
        // User:
        // Role = Company
        // AccountStatus = Pending
        //
        // CompanyProfile:
        // VerificationStatus = Pending
        // IsActive = false
        // =====================================

        [HttpPost("company-register")]
        public async Task<IActionResult>
            RegisterCompany(
                CompanyRegisterRequestDto request)
        {
            var email =
                request.Email
                    .Trim()
                    .ToLowerInvariant();


            // =================================
            // EMAIL CHECK
            // =================================

            var emailExists =
                await _context.Users.AnyAsync(
                    x => x.Email == email);


            if (emailExists)
            {
                return BadRequest(new
                {
                    message =
                        "An account with this email already exists."
                });
            }


            // =================================
            // COMPANY NAME CHECK
            // =================================

            var companyName =
                request.CompanyName.Trim();


            var companyExists =
                await _context.CompanyProfiles
                    .AnyAsync(x =>
                        x.CompanyName ==
                        companyName);


            if (companyExists)
            {
                return BadRequest(new
                {
                    message =
                        "A company or organization with this name is already registered."
                });
            }


            // =================================
            // DATABASE TRANSACTION
            // =================================

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();


            try
            {
                // =============================
                // CREATE PENDING COMPANY USER
                // =============================

                var user =
                    new User
                    {
                        FullName =
                            request.FullName.Trim(),

                        Email =
                            email,

                        Phone =
                            string.IsNullOrWhiteSpace(
                                request.Phone)
                                ? null
                                : request.Phone.Trim(),

                        Location =
                            request.Location.Trim(),

                        Role =
                            "Company",

                        AccountStatus =
                            "Pending",

                        CreatedAt =
                            DateTime.UtcNow
                    };


                user.PasswordHash =
                    _passwordHasher.HashPassword(
                        user,
                        request.Password);


                _context.Users.Add(user);

                await _context.SaveChangesAsync();


                // =============================
                // CREATE COMPANY PROFILE
                // =============================

                var companyProfile =
                    new CompanyProfile
                    {
                        UserId =
                            user.UserId,

                        CompanyName =
                            companyName,

                        Industry =
                            null,

                        Website =
                            null,

                        Description =
                            null,

                        Address =
                            request.Location.Trim(),

                        LogoPath =
                            null,

                        VerificationStatus =
                            "Pending",

                        // Not active until approved
                        IsActive =
                            false,

                        CreatedAt =
                            DateTime.UtcNow
                    };


                _context.CompanyProfiles.Add(
                    companyProfile);


                await _context.SaveChangesAsync();


                await transaction.CommitAsync();


                return StatusCode(
                    StatusCodes.Status201Created,
                    new
                    {
                        userId =
                            user.UserId,

                        companyProfileId =
                            companyProfile
                                .CompanyProfileId,

                        companyName =
                            companyProfile
                                .CompanyName,

                        fullName =
                            user.FullName,

                        email =
                            user.Email,

                        role =
                            user.Role,

                        accountStatus =
                            user.AccountStatus,

                        verificationStatus =
                            companyProfile
                                .VerificationStatus,

                        message =
                            "Your company registration request has been submitted successfully and is awaiting WorkHub administrator approval."
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
                            "Unable to submit the company registration request."
                    });
            }
        }


        // =====================================
        // LOGIN
        // =====================================

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginRequestDto request)
        {
            var email =
                request.Email
                    .Trim()
                    .ToLowerInvariant();


            var user =
                await _context.Users
                    .FirstOrDefaultAsync(
                        x => x.Email == email);


            // =================================
            // EMAIL CHECK
            // =================================

            if (user == null)
            {
                return Unauthorized(new
                {
                    message =
                        "Invalid email or password."
                });
            }


            // =================================
            // PASSWORD CHECK
            // =================================

            var passwordResult =
                _passwordHasher
                    .VerifyHashedPassword(
                        user,
                        user.PasswordHash,
                        request.Password);


            if (passwordResult ==
                PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    message =
                        "Invalid email or password."
                });
            }


            // =================================
            // PENDING COMPANY
            // =================================

            if (user.Role.Equals(
                    "Company",
                    StringComparison.OrdinalIgnoreCase)
                &&
                user.AccountStatus.Equals(
                    "Pending",
                    StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message =
                            "Your company registration request is currently awaiting WorkHub administrator approval."
                    });
            }


            // =================================
            // REJECTED COMPANY
            // =================================

            if (user.Role.Equals(
                    "Company",
                    StringComparison.OrdinalIgnoreCase)
                &&
                user.AccountStatus.Equals(
                    "Rejected",
                    StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message =
                            "Your company registration request was not approved. Please contact WorkHub administration for more information."
                    });
            }


            // =================================
            // SUSPENDED ACCOUNT
            // =================================

            if (user.AccountStatus.Equals(
                    "Suspended",
                    StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message =
                            "Your WorkHub account is currently suspended."
                    });
            }


            // =================================
            // OTHER INACTIVE STATUS
            // =================================

            if (!user.AccountStatus.Equals(
                    "Active",
                    StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message =
                            "Your WorkHub account is currently inactive."
                    });
            }


            // =================================
            // EXTRA COMPANY APPROVAL CHECK
            // =================================

            if (user.Role.Equals(
                    "Company",
                    StringComparison.OrdinalIgnoreCase))
            {
                var companyProfile =
                    await _context.CompanyProfiles
                        .FirstOrDefaultAsync(
                            x =>
                                x.UserId ==
                                user.UserId);


                if (companyProfile == null)
                {
                    return StatusCode(
                        StatusCodes
                            .Status403Forbidden,
                        new
                        {
                            message =
                                "Your company profile is not available."
                        });
                }


                if (!companyProfile
                    .VerificationStatus.Equals(
                        "Approved",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    !companyProfile.IsActive)
                {
                    return StatusCode(
                        StatusCodes
                            .Status403Forbidden,
                        new
                        {
                            message =
                                "Your company account has not yet been approved by WorkHub."
                        });
                }
            }


            // =================================
            // PASSWORD HASH UPGRADE
            // =================================

            if (passwordResult ==
                PasswordVerificationResult
                    .SuccessRehashNeeded)
            {
                user.PasswordHash =
                    _passwordHasher.HashPassword(
                        user,
                        request.Password);


                await _context.SaveChangesAsync();
            }


            // =================================
            // CREATE JWT
            // =================================

            var token =
                _tokenService.CreateToken(
                    user);


            var expiryMinutes =
                int.TryParse(
                    _configuration[
                        "Jwt:ExpiryMinutes"],
                    out var minutes)
                    ? minutes
                    : 120;


            var response =
                new LoginResponseDto
                {
                    UserId =
                        user.UserId,

                    FullName =
                        user.FullName,

                    Email =
                        user.Email,

                    Role =
                        user.Role,

                    Token =
                        token,

                    ExpiresAt =
                        DateTime.UtcNow
                            .AddMinutes(
                                expiryMinutes)
                };


            return Ok(response);
        }


        // =====================================
        // AUTHENTICATED USER TEST
        // =====================================

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            var fullName =
                User.FindFirstValue(
                    ClaimTypes.Name);


            var email =
                User.FindFirstValue(
                    ClaimTypes.Email);


            var role =
                User.FindFirstValue(
                    ClaimTypes.Role);


            return Ok(new
            {
                userId,
                fullName,
                email,
                role
            });
        }
    }
}