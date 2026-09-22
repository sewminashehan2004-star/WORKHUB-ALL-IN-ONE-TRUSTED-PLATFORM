using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkHub.API.Data;
using WorkHub.API.DTOs;
using WorkHub.API.Models;

namespace WorkHub.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ApplicationDbContext
            _context;

        private readonly IWebHostEnvironment
            _environment;

        private readonly PasswordHasher<User>
            _passwordHasher;


        // =========================================
        // PROFILE IMAGE SETTINGS
        // =========================================

        private const long MaxProfileImageSize =
            5 * 1024 * 1024;


        private static readonly string[]
            AllowedImageExtensions =
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };


        private static readonly string[]
            AllowedImageContentTypes =
            {
                "image/jpeg",
                "image/png",
                "image/webp"
            };


        public CompanyController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context =
                context;

            _environment =
                environment;

            _passwordHasher =
                new PasswordHasher<User>();
        }


        // =========================================
        // CREATE COMPANY PROFILE
        // =========================================

        [Authorize(Roles = "Company")]
        [HttpPost("profile")]
        public async Task<IActionResult> CreateProfile(
            CompanyProfileDto request)
        {
            var userId =
                GetCurrentUserId();


            if (!userId.HasValue)
            {
                return Unauthorized();
            }


            if (string.IsNullOrWhiteSpace(
                    request.CompanyName))
            {
                return BadRequest(new
                {
                    message =
                        "Company / Organization name is required."
                });
            }


            var existingProfile =
                await _context.CompanyProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId ==
                        userId.Value);


            if (existingProfile != null)
            {
                return BadRequest(new
                {
                    message =
                        "Company profile already exists."
                });
            }


            var company =
                new CompanyProfile
                {
                    UserId =
                        userId.Value,

                    CompanyName =
                        request.CompanyName
                            .Trim(),

                    Industry =
                        Clean(
                            request.Industry),

                    Website =
                        Clean(
                            request.Website),

                    Description =
                        Clean(
                            request.Description),

                    Address =
                        Clean(
                            request.Address),

                    VerificationStatus =
                        "Pending",

                    IsActive =
                        true,

                    CreatedAt =
                        DateTime.UtcNow
                };


            _context.CompanyProfiles.Add(
                company);


            await _context.SaveChangesAsync();


            return Ok(new
            {
                message =
                    "Company profile created successfully.",

                company.CompanyProfileId,

                company.CompanyName,

                company.VerificationStatus
            });
        }


        // =========================================
        // GET MY COMPANY PROFILE
        // =========================================

        [Authorize(Roles = "Company")]
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId =
                GetCurrentUserId();


            if (!userId.HasValue)
            {
                return Unauthorized();
            }


            var company =
                await _context.CompanyProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId ==
                        userId.Value);


            if (company == null)
            {
                return NotFound(new
                {
                    message =
                        "Company profile not found."
                });
            }


            return Ok(new
            {
                company.CompanyProfileId,

                company.CompanyName,

                company.Industry,

                company.Website,

                company.Description,

                company.Address,

                company.LogoPath,

                company.VerificationStatus,

                company.IsActive,

                company.CreatedAt
            });
        }


        // =========================================
        // UPDATE COMPANY PROFILE
        // =========================================

        [Authorize(Roles = "Company")]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(
            CompanyProfileDto request)
        {
            var userId =
                GetCurrentUserId();


            if (!userId.HasValue)
            {
                return Unauthorized();
            }


            if (string.IsNullOrWhiteSpace(
                    request.CompanyName))
            {
                return BadRequest(new
                {
                    message =
                        "Company / Organization name is required."
                });
            }


            var company =
                await _context.CompanyProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId ==
                        userId.Value);


            if (company == null)
            {
                return NotFound(new
                {
                    message =
                        "Company profile not found."
                });
            }


            company.CompanyName =
                request.CompanyName
                    .Trim();


            company.Industry =
                Clean(
                    request.Industry);


            company.Website =
                Clean(
                    request.Website);


            company.Description =
                Clean(
                    request.Description);


            company.Address =
                Clean(
                    request.Address);


            await _context.SaveChangesAsync();


            return Ok(new
            {
                message =
                    "Company profile updated successfully."
            });
        }


        // =========================================
        // GET COMPANY PROFILE IMAGE
        //
        // AUTHENTICATED COMPANY ONLY
        //
        // GET:
        // api/Company/profile-image
        // =========================================

        [Authorize(Roles = "Company")]
        [HttpGet("profile-image")]
        public async Task<IActionResult>
            GetProfileImage()
        {
            var userId =
                GetCurrentUserId();


            if (!userId.HasValue)
            {
                return Unauthorized();
            }


            var company =
                await _context.CompanyProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.UserId ==
                        userId.Value);


            if (company == null)
            {
                return NotFound(new
                {
                    message =
                        "Company profile not found."
                });
            }


            if (string.IsNullOrWhiteSpace(
                    company.LogoPath))
            {
                return NotFound(new
                {
                    message =
                        "Company profile picture has not been uploaded."
                });
            }


            var physicalPath =
                GetProfileImagePhysicalPath(
                    company.LogoPath);


            if (physicalPath == null ||
                !System.IO.File.Exists(
                    physicalPath))
            {
                return NotFound(new
                {
                    message =
                        "Company profile picture could not be found."
                });
            }


            var extension =
                Path.GetExtension(
                        physicalPath)
                    .ToLowerInvariant();


            var contentType =
                GetImageContentType(
                    extension);


            var bytes =
                await System.IO.File
                    .ReadAllBytesAsync(
                        physicalPath);


            Response.Headers.CacheControl =
                "no-store, no-cache, must-revalidate";


            return File(
                bytes,
                contentType);
        }


        // =========================================
        // UPLOAD COMPANY PROFILE IMAGE
        //
        // POST:
        // api/Company/profile-image
        //
        // multipart/form-data
        // field name: file
        // =========================================

        [Authorize(Roles = "Company")]
        [HttpPost("profile-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult>
            UploadProfileImage(
                [FromForm] IFormFile file)
        {
            var userId =
                GetCurrentUserId();


            if (!userId.HasValue)
            {
                return Unauthorized();
            }


            var company =
                await _context.CompanyProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId ==
                        userId.Value);


            if (company == null)
            {
                return NotFound(new
                {
                    message =
                        "Company profile not found."
                });
            }


            // =====================================
            // FILE REQUIRED
            // =====================================

            if (file == null ||
                file.Length <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "Please select a profile picture."
                });
            }


            // =====================================
            // SIZE
            // =====================================

            if (file.Length >
                MaxProfileImageSize)
            {
                return BadRequest(new
                {
                    message =
                        "Profile picture must be 5 MB or smaller."
                });
            }


            // =====================================
            // EXTENSION
            // =====================================

            var extension =
                Path.GetExtension(
                        file.FileName)
                    .ToLowerInvariant();


            if (!AllowedImageExtensions
                .Contains(
                    extension))
            {
                return BadRequest(new
                {
                    message =
                        "Only JPG, JPEG, PNG and WEBP images are allowed."
                });
            }


            // =====================================
            // MIME TYPE
            // =====================================

            if (
                string.IsNullOrWhiteSpace(
                    file.ContentType)
                ||
                !AllowedImageContentTypes
                    .Contains(
                        file.ContentType,
                        StringComparer
                            .OrdinalIgnoreCase)
            )
            {
                return BadRequest(new
                {
                    message =
                        "Invalid profile image type."
                });
            }


            // =====================================
            // CREATE UPLOAD DIRECTORY
            // =====================================

            var uploadFolder =
                Path.Combine(
                    _environment.ContentRootPath,
                    "Uploads",
                    "Companies");


            Directory.CreateDirectory(
                uploadFolder);


            // =====================================
            // CREATE SAFE RANDOM FILE NAME
            // =====================================

            var storedFileName =
                $"{Guid.NewGuid():N}{extension}";


            var physicalPath =
                Path.Combine(
                    uploadFolder,
                    storedFileName);


            var relativePath =
                Path.Combine(
                    "Uploads",
                    "Companies",
                    storedFileName)
                .Replace(
                    "\\",
                    "/");


            // =====================================
            // REMEMBER OLD IMAGE
            // =====================================

            var oldLogoPath =
                company.LogoPath;


            try
            {
                // =================================
                // SAVE NEW FILE
                // =================================

                await using (
                    var stream =
                        new FileStream(
                            physicalPath,
                            FileMode.CreateNew,
                            FileAccess.Write))
                {
                    await file.CopyToAsync(
                        stream);
                }


                // =================================
                // UPDATE DATABASE
                // =================================

                company.LogoPath =
                    relativePath;


                await _context.SaveChangesAsync();


                // =================================
                // DELETE OLD IMAGE
                // ONLY AFTER DATABASE SAVE
                // =================================

                DeleteProfileImageFile(
                    oldLogoPath);


                return Ok(new
                {
                    message =
                        "Profile picture updated successfully.",

                    company.LogoPath
                });
            }
            catch
            {
                // =================================
                // REMOVE NEW FILE IF DB SAVE FAILED
                // =================================

                try
                {
                    if (System.IO.File.Exists(
                            physicalPath))
                    {
                        System.IO.File.Delete(
                            physicalPath);
                    }
                }
                catch
                {
                }


                return StatusCode(
                    StatusCodes
                        .Status500InternalServerError,
                    new
                    {
                        message =
                            "Unable to save the profile picture."
                    });
            }
        }


        // =========================================
        // REMOVE COMPANY PROFILE IMAGE
        //
        // DELETE:
        // api/Company/profile-image
        // =========================================

        [Authorize(Roles = "Company")]
        [HttpDelete("profile-image")]
        public async Task<IActionResult>
            RemoveProfileImage()
        {
            var userId =
                GetCurrentUserId();


            if (!userId.HasValue)
            {
                return Unauthorized();
            }


            var company =
                await _context.CompanyProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId ==
                        userId.Value);


            if (company == null)
            {
                return NotFound(new
                {
                    message =
                        "Company profile not found."
                });
            }


            if (string.IsNullOrWhiteSpace(
                    company.LogoPath))
            {
                return Ok(new
                {
                    message =
                        "Profile picture is already removed."
                });
            }


            var oldLogoPath =
                company.LogoPath;


            company.LogoPath =
                null;


            await _context.SaveChangesAsync();


            DeleteProfileImageFile(
                oldLogoPath);


            return Ok(new
            {
                message =
                    "Profile picture removed successfully."
            });
        }


        // =========================================
        // UPDATE ACCOUNT MANAGER NAME
        //
        // PUT:
        // api/Company/account/name
        // =========================================

        [Authorize(Roles = "Company")]
        [HttpPut("account/name")]
        public async Task<IActionResult>
            UpdateAccountName(
                UpdateCompanyAccountNameRequest
                    request)
        {
            var userId =
                GetCurrentUserId();


            if (!userId.HasValue)
            {
                return Unauthorized();
            }


            var fullName =
                request.FullName?.Trim()
                ??
                string.Empty;


            if (string.IsNullOrWhiteSpace(
                    fullName))
            {
                return BadRequest(new
                {
                    message =
                        "Account manager name is required."
                });
            }


            if (fullName.Length < 2)
            {
                return BadRequest(new
                {
                    message =
                        "Please enter a valid account manager name."
                });
            }


            if (fullName.Length > 150)
            {
                return BadRequest(new
                {
                    message =
                        "Account manager name cannot exceed 150 characters."
                });
            }


            var user =
                await _context.Users
                    .FirstOrDefaultAsync(x =>
                        x.UserId ==
                        userId.Value);


            if (user == null)
            {
                return NotFound(new
                {
                    message =
                        "Company account not found."
                });
            }


            if (!string.Equals(
                    user.Role,
                    "Company",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }


            user.FullName =
                fullName;


            await _context.SaveChangesAsync();


            return Ok(new
            {
                message =
                    "Account manager name updated successfully.",

                user.UserId,

                user.FullName,

                user.Email
            });
        }


        // =========================================
        // CHANGE COMPANY PASSWORD
        //
        // PUT:
        // api/Company/account/password
        // =========================================

        [Authorize(Roles = "Company")]
        [HttpPut("account/password")]
        public async Task<IActionResult>
            ChangePassword(
                ChangeCompanyPasswordRequest
                    request)
        {
            var userId =
                GetCurrentUserId();


            if (!userId.HasValue)
            {
                return Unauthorized();
            }


            request.CurrentPassword ??=
                string.Empty;


            request.NewPassword ??=
                string.Empty;


            request.ConfirmPassword ??=
                string.Empty;


            // =====================================
            // REQUIRED
            // =====================================

            if (
                string.IsNullOrWhiteSpace(
                    request.CurrentPassword)
                ||
                string.IsNullOrWhiteSpace(
                    request.NewPassword)
                ||
                string.IsNullOrWhiteSpace(
                    request.ConfirmPassword)
            )
            {
                return BadRequest(new
                {
                    message =
                        "Please complete all password fields."
                });
            }


            // =====================================
            // NEW PASSWORD LENGTH
            // =====================================

            if (request.NewPassword.Length < 6)
            {
                return BadRequest(new
                {
                    message =
                        "New password must contain at least 6 characters."
                });
            }


            // =====================================
            // CONFIRM PASSWORD
            // =====================================

            if (!string.Equals(
                    request.NewPassword,
                    request.ConfirmPassword,
                    StringComparison.Ordinal))
            {
                return BadRequest(new
                {
                    message =
                        "New password and confirmation password do not match."
                });
            }


            // =====================================
            // CURRENT != NEW
            // =====================================

            if (string.Equals(
                    request.CurrentPassword,
                    request.NewPassword,
                    StringComparison.Ordinal))
            {
                return BadRequest(new
                {
                    message =
                        "New password must be different from your current password."
                });
            }


            var user =
                await _context.Users
                    .FirstOrDefaultAsync(x =>
                        x.UserId ==
                        userId.Value);


            if (user == null)
            {
                return NotFound(new
                {
                    message =
                        "Company account not found."
                });
            }


            if (!string.Equals(
                    user.Role,
                    "Company",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }


            // =====================================
            // VERIFY CURRENT PASSWORD
            // =====================================

            var verificationResult =
                _passwordHasher
                    .VerifyHashedPassword(
                        user,
                        user.PasswordHash,
                        request.CurrentPassword);


            if (
                verificationResult ==
                PasswordVerificationResult
                    .Failed
            )
            {
                return BadRequest(new
                {
                    message =
                        "Current password is incorrect."
                });
            }


            // =====================================
            // CREATE NEW HASH
            // =====================================

            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    request.NewPassword);


            await _context.SaveChangesAsync();


            return Ok(new
            {
                message =
                    "Password changed successfully."
            });
        }


        // =========================================
        // DELETE / DEACTIVATE COMPANY ACCOUNT
        //
        // DELETE:
        // api/Company/account
        //
        // We preserve relational recruitment
        // history instead of physically deleting
        // Jobs / Applications.
        // =========================================

        [Authorize(Roles = "Company")]
        [HttpDelete("account")]
        public async Task<IActionResult>
            DeleteAccount(
                [FromBody]
                DeleteCompanyAccountRequest
                    request)
        {
            var userId =
                GetCurrentUserId();


            if (!userId.HasValue)
            {
                return Unauthorized();
            }


            request.Password ??=
                string.Empty;


            request.Confirmation ??=
                string.Empty;


            // =====================================
            // PASSWORD REQUIRED
            // =====================================

            if (string.IsNullOrWhiteSpace(
                    request.Password))
            {
                return BadRequest(new
                {
                    message =
                        "Password is required to delete the account."
                });
            }


            // =====================================
            // DELETE CONFIRMATION
            // =====================================

            if (!string.Equals(
                    request.Confirmation.Trim(),
                    "DELETE",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message =
                        "Type DELETE to confirm account deletion."
                });
            }


            var user =
                await _context.Users
                    .FirstOrDefaultAsync(x =>
                        x.UserId ==
                        userId.Value);


            if (user == null)
            {
                return NotFound(new
                {
                    message =
                        "Company account not found."
                });
            }


            if (!string.Equals(
                    user.Role,
                    "Company",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }


            // =====================================
            // VERIFY PASSWORD
            // =====================================

            var passwordResult =
                _passwordHasher
                    .VerifyHashedPassword(
                        user,
                        user.PasswordHash,
                        request.Password);


            if (
                passwordResult ==
                PasswordVerificationResult
                    .Failed
            )
            {
                return BadRequest(new
                {
                    message =
                        "Password is incorrect."
                });
            }


            var company =
                await _context.CompanyProfiles
                    .FirstOrDefaultAsync(x =>
                        x.UserId ==
                        userId.Value);


            if (company == null)
            {
                return NotFound(new
                {
                    message =
                        "Company profile not found."
                });
            }


            // =====================================
            // LOAD COMPANY JOBS
            // =====================================

            var companyJobs =
                await _context.Jobs
                    .Where(x =>
                        x.CompanyProfileId ==
                        company.CompanyProfileId)
                    .ToListAsync();


            // =====================================
            // CLOSE ALL JOBS
            // =====================================

            foreach (var job
                in companyJobs)
            {
                if (!string.Equals(
                        job.Status,
                        "Closed",
                        StringComparison.OrdinalIgnoreCase))
                {
                    job.Status =
                        "Closed";
                }
            }


            // =====================================
            // REMEMBER LOGO
            // =====================================

            var oldLogoPath =
                company.LogoPath;


            // =====================================
            // DEACTIVATE COMPANY
            // =====================================

            company.IsActive =
                false;


            company.VerificationStatus =
                "Deleted";


            company.LogoPath =
                null;


            // =====================================
            // DISABLE LOGIN
            //
            // Existing authentication already
            // treats suspended accounts as blocked.
            // =====================================

            user.AccountStatus =
                "Suspended";


            await _context.SaveChangesAsync();


            // =====================================
            // REMOVE STORED PROFILE IMAGE
            // =====================================

            DeleteProfileImageFile(
                oldLogoPath);


            return Ok(new
            {
                message =
                    "Company account deleted successfully."
            });
        }


        // =========================================
        // PUBLIC - GET COMPANY DETAILS
        // =========================================

        [AllowAnonymous]
        [HttpGet("{companyId:int}")]
        public async Task<IActionResult> GetCompany(
            int companyId)
        {
            var company =
                await _context.CompanyProfiles
                    .Where(x =>
                        x.CompanyProfileId ==
                        companyId
                        &&
                        x.IsActive)
                    .Select(x => new
                    {
                        x.CompanyProfileId,

                        x.CompanyName,

                        x.Industry,

                        x.Website,

                        x.Description,

                        x.Address,

                        x.LogoPath,

                        x.VerificationStatus
                    })
                    .FirstOrDefaultAsync();


            if (company == null)
            {
                return NotFound(new
                {
                    message =
                        "Company not found."
                });
            }


            return Ok(
                company);
        }


        // =========================================
        // PUBLIC - GET ALL COMPANIES
        // =========================================

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetCompanies()
        {
            var companies =
                await _context.CompanyProfiles
                    .Where(x =>
                        x.IsActive)
                    .OrderBy(x =>
                        x.CompanyName)
                    .Select(x => new
                    {
                        x.CompanyProfileId,

                        x.CompanyName,

                        x.Industry,

                        x.Address,

                        x.LogoPath,

                        x.VerificationStatus
                    })
                    .ToListAsync();


            return Ok(
                companies);
        }


        // =========================================
        // GET CURRENT JWT USER ID
        // =========================================

        private int? GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            if (!int.TryParse(
                    userIdValue,
                    out var userId))
            {
                return null;
            }


            return userId;
        }


        // =========================================
        // PROFILE IMAGE PHYSICAL PATH
        // =========================================

        private string?
            GetProfileImagePhysicalPath(
                string? logoPath)
        {
            if (string.IsNullOrWhiteSpace(
                    logoPath))
            {
                return null;
            }


            var normalized =
                logoPath
                    .Replace(
                        "\\",
                        "/")
                    .TrimStart('/');


            // =====================================
            // ONLY ALLOW OUR COMPANY UPLOAD FOLDER
            // =====================================

            if (!normalized.StartsWith(
                    "Uploads/Companies/",
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }


            var fileName =
                Path.GetFileName(
                    normalized);


            if (string.IsNullOrWhiteSpace(
                    fileName))
            {
                return null;
            }


            return Path.Combine(
                _environment.ContentRootPath,
                "Uploads",
                "Companies",
                fileName);
        }


        // =========================================
        // DELETE PROFILE IMAGE FILE
        // =========================================

        private void DeleteProfileImageFile(
            string? logoPath)
        {
            if (string.IsNullOrWhiteSpace(
                    logoPath))
            {
                return;
            }


            try
            {
                var physicalPath =
                    GetProfileImagePhysicalPath(
                        logoPath);


                if (
                    physicalPath != null
                    &&
                    System.IO.File.Exists(
                        physicalPath)
                )
                {
                    System.IO.File.Delete(
                        physicalPath);
                }
            }
            catch
            {
                // File cleanup should not cause
                // account/profile operation to fail.
            }
        }


        // =========================================
        // IMAGE MIME TYPE
        // =========================================

        private static string
            GetImageContentType(
                string extension)
        {
            return extension
                .ToLowerInvariant()
                switch
            {
                ".png" =>
                    "image/png",

                ".webp" =>
                    "image/webp",

                ".jpg" =>
                    "image/jpeg",

                ".jpeg" =>
                    "image/jpeg",

                _ =>
                    "application/octet-stream"
            };
        }


        // =========================================
        // CLEAN OPTIONAL STRING
        // =========================================

        private static string? Clean(
            string? value)
        {
            return string.IsNullOrWhiteSpace(
                    value)
                ? null
                : value.Trim();
        }


        // =========================================
        // REQUEST MODELS
        // =========================================

        public class UpdateCompanyAccountNameRequest
        {
            public string FullName
            {
                get;
                set;
            } = string.Empty;
        }


        public class ChangeCompanyPasswordRequest
        {
            public string CurrentPassword
            {
                get;
                set;
            } = string.Empty;


            public string NewPassword
            {
                get;
                set;
            } = string.Empty;


            public string ConfirmPassword
            {
                get;
                set;
            } = string.Empty;
        }


        public class DeleteCompanyAccountRequest
        {
            public string Password
            {
                get;
                set;
            } = string.Empty;


            public string Confirmation
            {
                get;
                set;
            } = string.Empty;
        }
    }
}