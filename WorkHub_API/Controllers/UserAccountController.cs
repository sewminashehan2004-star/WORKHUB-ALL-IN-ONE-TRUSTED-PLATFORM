using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkHub.API.Data;
using WorkHub.API.Models;

namespace WorkHub.API.Controllers
{
    [ApiController]
    [Route("api/UserAccount")]
    [Authorize(Roles = "RegisteredUser")]
    public class UserAccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly PasswordHasher<User>
            _passwordHasher =
                new PasswordHasher<User>();


        public UserAccountController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // CHANGE PASSWORD
        //
        // PUT:
        // /api/UserAccount/password
        // =========================================

        [HttpPut("password")]
        public async Task<IActionResult>
            ChangePassword(
                ChangePasswordRequest request)
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
                return BadRequest(
                    new
                    {
                        message =
                            "Please complete all password fields."
                    });
            }


            // =====================================
            // MINIMUM LENGTH
            // =====================================

            if (request.NewPassword.Length < 6)
            {
                return BadRequest(
                    new
                    {
                        message =
                            "New password must contain at least 6 characters."
                    });
            }


            // =====================================
            // PASSWORD MATCH
            // =====================================

            if (!string.Equals(
                    request.NewPassword,
                    request.ConfirmPassword,
                    StringComparison.Ordinal))
            {
                return BadRequest(
                    new
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
                return BadRequest(
                    new
                    {
                        message =
                            "New password must be different from your current password."
                    });
            }


            // =====================================
            // USER
            // =====================================

            var user =
                await _context.Users
                    .FirstOrDefaultAsync(
                        x =>
                            x.UserId ==
                            userId.Value);


            if (user == null)
            {
                return NotFound(
                    new
                    {
                        message =
                            "User account not found."
                    });
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


            if (verificationResult ==
                PasswordVerificationResult.Failed)
            {
                return BadRequest(
                    new
                    {
                        message =
                            "Current password is incorrect."
                    });
            }


            // =====================================
            // SAVE NEW PASSWORD
            // =====================================

            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    request.NewPassword);


            await _context.SaveChangesAsync();


            return Ok(
                new
                {
                    message =
                        "Password changed successfully."
                });
        }


        // =========================================
        // DELETE / DISABLE ACCOUNT
        //
        // DELETE:
        // /api/UserAccount/account
        //
        // We use SAFE SOFT DELETE because the
        // user can already have jobs/applications,
        // services, listings, notifications etc.
        // =========================================

        [HttpDelete("account")]
        public async Task<IActionResult>
            DeleteAccount(
                DeleteAccountRequest request)
        {
            var userId =
                GetCurrentUserId();


            if (!userId.HasValue)
            {
                return Unauthorized();
            }


            request.Password ??=
                string.Empty;

            request.Confirmation =
                request.Confirmation?.Trim()
                ??
                string.Empty;


            // =====================================
            // PASSWORD REQUIRED
            // =====================================

            if (string.IsNullOrWhiteSpace(
                    request.Password))
            {
                return BadRequest(
                    new
                    {
                        message =
                            "Enter your password before deleting the account."
                    });
            }


            // =====================================
            // REQUIRE DELETE
            // =====================================

            if (!string.Equals(
                    request.Confirmation,
                    "DELETE",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(
                    new
                    {
                        message =
                            "Type DELETE to confirm account deletion."
                    });
            }


            // =====================================
            // USER
            // =====================================

            var user =
                await _context.Users
                    .FirstOrDefaultAsync(
                        x =>
                            x.UserId ==
                            userId.Value);


            if (user == null)
            {
                return NotFound(
                    new
                    {
                        message =
                            "User account not found."
                    });
            }


            // =====================================
            // VERIFY PASSWORD
            // =====================================

            var verificationResult =
                _passwordHasher
                    .VerifyHashedPassword(
                        user,
                        user.PasswordHash,
                        request.Password);


            if (verificationResult ==
                PasswordVerificationResult.Failed)
            {
                return BadRequest(
                    new
                    {
                        message =
                            "Password is incorrect."
                    });
            }


            // =====================================
            // DISABLE WORKHUB ACCOUNT
            //
            // Existing login system already blocks
            // Suspended accounts.
            // =====================================

            user.AccountStatus =
                "Suspended";


            // =====================================
            // DISABLE JOB SEEKER PROFILE
            // =====================================

            var jobSeeker =
                await _context
                    .JobSeekerProfiles
                    .FirstOrDefaultAsync(
                        x =>
                            x.UserId ==
                            userId.Value);


            if (jobSeeker != null)
            {
                jobSeeker.IsActive =
                    false;
            }


            // =====================================
            // DISABLE SERVICE PROVIDER PROFILE
            // =====================================

            var serviceProvider =
                await _context
                    .ServiceProviderProfiles
                    .FirstOrDefaultAsync(
                        x =>
                            x.UserId ==
                            userId.Value);


            if (serviceProvider != null)
            {
                serviceProvider.IsActive =
                    false;
            }


            // =====================================
            // DISABLE MARKETPLACE SELLER PROFILE
            // =====================================

            var marketplaceSeller =
                await _context
                    .MarketplaceSellerProfiles
                    .FirstOrDefaultAsync(
                        x =>
                            x.UserId ==
                            userId.Value);


            if (marketplaceSeller != null)
            {
                marketplaceSeller.IsActive =
                    false;
            }


            await _context.SaveChangesAsync();


            return Ok(
                new
                {
                    message =
                        "Your WorkHub account has been deleted successfully."
                });
        }


        // =========================================
        // CURRENT USER ID
        // =========================================

        private int? GetCurrentUserId()
        {
            var value =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            if (!int.TryParse(
                    value,
                    out var userId))
            {
                return null;
            }


            return userId;
        }


        // =========================================
        // REQUEST MODELS
        // =========================================

        public class ChangePasswordRequest
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


        public class DeleteAccountRequest
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