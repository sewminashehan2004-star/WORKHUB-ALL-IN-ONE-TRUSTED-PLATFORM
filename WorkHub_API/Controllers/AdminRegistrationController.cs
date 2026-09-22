using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WorkHub.API.Data;
using WorkHub.API.Models;

namespace WorkHub.API.Controllers
{
    [ApiController]
    [Route("api/AdminManagement")]
    public class AdminManagementController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;


        public AdminManagementController(
            ApplicationDbContext context,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }


        // =====================================================
        // CREATE ADMIN
        //
        // POST:
        // /api/AdminManagement
        //
        // PUBLIC ENDPOINT
        //
        // NO JWT REQUIRED.
        // NO EXISTING ADMIN REQUIRED.
        // ANY API CLIENT CAN CREATE AN ADMIN.
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> CreateAdmin(
            AdminCreateRequest request)
        {
            CleanCreateRequest(request);


            // =========================================
            // FULL NAME
            // =========================================

            if (string.IsNullOrWhiteSpace(
                    request.FullName))
            {
                return BadRequest(new
                {
                    message =
                        "Full name is required."
                });
            }


            // =========================================
            // EMAIL
            // =========================================

            if (string.IsNullOrWhiteSpace(
                    request.Email))
            {
                return BadRequest(new
                {
                    message =
                        "Email is required."
                });
            }


            if (!IsValidEmail(
                    request.Email))
            {
                return BadRequest(new
                {
                    message =
                        "Please enter a valid email address."
                });
            }


            // =========================================
            // ACCOUNT TYPE
            // =========================================

            if (
                !string.IsNullOrWhiteSpace(
                    request.AccountType)
                &&
                !request.AccountType.Equals(
                    "Admin",
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                return BadRequest(new
                {
                    message =
                        "Account type must be Admin."
                });
            }


            // =========================================
            // PASSWORD
            // =========================================

            if (string.IsNullOrWhiteSpace(
                    request.Password))
            {
                return BadRequest(new
                {
                    message =
                        "Password is required."
                });
            }


            if (request.Password.Length < 8)
            {
                return BadRequest(new
                {
                    message =
                        "Admin password must contain at least 8 characters."
                });
            }


            // =========================================
            // CONFIRM PASSWORD
            // =========================================

            if (!string.Equals(
                    request.Password,
                    request.ConfirmPassword,
                    StringComparison.Ordinal))
            {
                return BadRequest(new
                {
                    message =
                        "Password and confirm password do not match."
                });
            }


            // =========================================
            // DUPLICATE EMAIL
            // =========================================

            var emailExists =
                await _context.Users
                    .AnyAsync(
                        x =>
                            x.Email ==
                            request.Email);


            if (emailExists)
            {
                return BadRequest(new
                {
                    message =
                        "An account with this email already exists."
                });
            }


            // =========================================
            // CREATE ADMIN USER
            // =========================================

            var admin =
                new User
                {
                    FullName =
                        request.FullName,

                    Email =
                        request.Email,

                    Phone =
                        string.IsNullOrWhiteSpace(
                            request.Phone)
                            ? null
                            : request.Phone,

                    Location =
                        string.IsNullOrWhiteSpace(
                            request.Location)
                            ? null
                            : request.Location,

                    Role =
                        "Admin",

                    AccountStatus =
                        "Active",

                    CreatedAt =
                        DateTime.UtcNow
                };


            // =========================================
            // HASH PASSWORD
            // =========================================

            admin.PasswordHash =
                _passwordHasher
                    .HashPassword(
                        admin,
                        request.Password);


            // =========================================
            // SAVE ADMIN
            // =========================================

            _context.Users.Add(
                admin);


            await _context
                .SaveChangesAsync();


            // =========================================
            // RESPONSE
            // =========================================

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    userId =
                        admin.UserId,

                    fullName =
                        admin.FullName,

                    email =
                        admin.Email,

                    phone =
                        admin.Phone,

                    location =
                        admin.Location,

                    accountType =
                        admin.Role,

                    role =
                        admin.Role,

                    accountStatus =
                        admin.AccountStatus,

                    createdAt =
                        admin.CreatedAt,

                    message =
                        "Administrator account created successfully."
                });
        }


        // =====================================================
        // SEE ALL ADMINS
        //
        // GET:
        // /api/AdminManagement
        //
        // ADMIN JWT REQUIRED
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> GetAllAdmins()
        {
            if (!IsCurrentUserAdmin())
            {
                return AdministratorAuthorizationRequired();
            }


            var admins =
                await _context.Users
                    .AsNoTracking()
                    .Where(
                        x =>
                            x.Role ==
                            "Admin")
                    .OrderByDescending(
                        x =>
                            x.CreatedAt)
                    .Select(
                        x => new
                        {
                            userId =
                                x.UserId,

                            fullName =
                                x.FullName,

                            email =
                                x.Email,

                            phone =
                                x.Phone,

                            location =
                                x.Location,

                            accountType =
                                x.Role,

                            role =
                                x.Role,

                            accountStatus =
                                x.AccountStatus,

                            createdAt =
                                x.CreatedAt
                        })
                    .ToListAsync();


            return Ok(admins);
        }


        // =====================================================
        // SEE ONE ADMIN
        //
        // GET:
        // /api/AdminManagement/5
        //
        // ADMIN JWT REQUIRED
        // =====================================================

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAdminById(
            int id)
        {
            if (!IsCurrentUserAdmin())
            {
                return AdministratorAuthorizationRequired();
            }


            var admin =
                await _context.Users
                    .AsNoTracking()
                    .Where(
                        x =>
                            x.UserId == id
                            &&
                            x.Role == "Admin")
                    .Select(
                        x => new
                        {
                            userId =
                                x.UserId,

                            fullName =
                                x.FullName,

                            email =
                                x.Email,

                            phone =
                                x.Phone,

                            location =
                                x.Location,

                            accountType =
                                x.Role,

                            role =
                                x.Role,

                            accountStatus =
                                x.AccountStatus,

                            createdAt =
                                x.CreatedAt
                        })
                    .FirstOrDefaultAsync();


            if (admin == null)
            {
                return NotFound(new
                {
                    message =
                        "Administrator account not found."
                });
            }


            return Ok(admin);
        }


        // =====================================================
        // UPDATE ADMIN
        //
        // PUT:
        // /api/AdminManagement/5
        //
        // ADMIN JWT REQUIRED
        //
        // PASSWORD CHANGE IS OPTIONAL.
        // =====================================================

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAdmin(
            int id,
            AdminUpdateRequest request)
        {
            if (!IsCurrentUserAdmin())
            {
                return AdministratorAuthorizationRequired();
            }


            CleanUpdateRequest(
                request);


            // =========================================
            // FIND ADMIN
            // =========================================

            var admin =
                await _context.Users
                    .FirstOrDefaultAsync(
                        x =>
                            x.UserId == id
                            &&
                            x.Role == "Admin");


            if (admin == null)
            {
                return NotFound(new
                {
                    message =
                        "Administrator account not found."
                });
            }


            // =========================================
            // FULL NAME
            // =========================================

            if (string.IsNullOrWhiteSpace(
                    request.FullName))
            {
                return BadRequest(new
                {
                    message =
                        "Full name is required."
                });
            }


            // =========================================
            // EMAIL
            // =========================================

            if (string.IsNullOrWhiteSpace(
                    request.Email))
            {
                return BadRequest(new
                {
                    message =
                        "Email is required."
                });
            }


            if (!IsValidEmail(
                    request.Email))
            {
                return BadRequest(new
                {
                    message =
                        "Please enter a valid email address."
                });
            }


            // =========================================
            // DUPLICATE EMAIL
            // =========================================

            var emailExists =
                await _context.Users
                    .AnyAsync(
                        x =>
                            x.Email ==
                                request.Email
                            &&
                            x.UserId != id);


            if (emailExists)
            {
                return BadRequest(new
                {
                    message =
                        "Another account is already using this email address."
                });
            }


            // =========================================
            // ACCOUNT STATUS
            // =========================================

            if (
                !request.AccountStatus.Equals(
                    "Active",
                    StringComparison.OrdinalIgnoreCase)
                &&
                !request.AccountStatus.Equals(
                    "Suspended",
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                return BadRequest(new
                {
                    message =
                        "Account status must be Active or Suspended."
                });
            }


            // =========================================
            // CURRENT ADMIN ID
            // =========================================

            var currentAdminId =
                GetCurrentUserId();


            // =========================================
            // CANNOT SUSPEND YOUR OWN ACCOUNT
            // =========================================

            if (
                currentAdminId.HasValue
                &&
                currentAdminId.Value ==
                    admin.UserId
                &&
                request.AccountStatus.Equals(
                    "Suspended",
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                return BadRequest(new
                {
                    message =
                        "You cannot suspend your own administrator account."
                });
            }


            // =========================================
            // UPDATE BASIC DETAILS
            // =========================================

            admin.FullName =
                request.FullName;


            admin.Email =
                request.Email;


            admin.Phone =
                string.IsNullOrWhiteSpace(
                    request.Phone)
                    ? null
                    : request.Phone;


            admin.Location =
                string.IsNullOrWhiteSpace(
                    request.Location)
                    ? null
                    : request.Location;


            admin.Role =
                "Admin";


            admin.AccountStatus =
                request.AccountStatus.Equals(
                    "Active",
                    StringComparison.OrdinalIgnoreCase)
                    ? "Active"
                    : "Suspended";


            // =========================================
            // OPTIONAL PASSWORD CHANGE
            // =========================================

            if (!string.IsNullOrWhiteSpace(
                    request.NewPassword))
            {
                if (request.NewPassword.Length < 8)
                {
                    return BadRequest(new
                    {
                        message =
                            "New admin password must contain at least 8 characters."
                    });
                }


                if (!string.Equals(
                        request.NewPassword,
                        request.ConfirmNewPassword,
                        StringComparison.Ordinal))
                {
                    return BadRequest(new
                    {
                        message =
                            "New password and confirm new password do not match."
                    });
                }


                admin.PasswordHash =
                    _passwordHasher
                        .HashPassword(
                            admin,
                            request.NewPassword);
            }


            // =========================================
            // SAVE
            // =========================================

            await _context
                .SaveChangesAsync();


            return Ok(new
            {
                userId =
                    admin.UserId,

                fullName =
                    admin.FullName,

                email =
                    admin.Email,

                phone =
                    admin.Phone,

                location =
                    admin.Location,

                accountType =
                    admin.Role,

                role =
                    admin.Role,

                accountStatus =
                    admin.AccountStatus,

                createdAt =
                    admin.CreatedAt,

                message =
                    "Administrator account updated successfully."
            });
        }


        // =====================================================
        // DELETE ADMIN
        //
        // DELETE:
        // /api/AdminManagement/5
        //
        // ADMIN JWT REQUIRED
        //
        // CANNOT DELETE YOURSELF.
        // CANNOT DELETE LAST ADMIN.
        // =====================================================

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAdmin(
            int id)
        {
            if (!IsCurrentUserAdmin())
            {
                return AdministratorAuthorizationRequired();
            }


            // =========================================
            // FIND ADMIN
            // =========================================

            var admin =
                await _context.Users
                    .FirstOrDefaultAsync(
                        x =>
                            x.UserId == id
                            &&
                            x.Role == "Admin");


            if (admin == null)
            {
                return NotFound(new
                {
                    message =
                        "Administrator account not found."
                });
            }


            // =========================================
            // CURRENT ADMIN
            // =========================================

            var currentAdminId =
                GetCurrentUserId();


            // =========================================
            // CANNOT DELETE YOURSELF
            // =========================================

            if (
                currentAdminId.HasValue
                &&
                currentAdminId.Value == id
            )
            {
                return BadRequest(new
                {
                    message =
                        "You cannot delete your own administrator account."
                });
            }


            // =========================================
            // CANNOT DELETE LAST ADMIN
            // =========================================

            var totalAdmins =
                await _context.Users
                    .CountAsync(
                        x =>
                            x.Role ==
                            "Admin");


            if (totalAdmins <= 1)
            {
                return BadRequest(new
                {
                    message =
                        "The last WorkHub administrator account cannot be deleted."
                });
            }


            // =========================================
            // DELETE
            // =========================================

            try
            {
                _context.Users.Remove(
                    admin);


                await _context
                    .SaveChangesAsync();


                return Ok(new
                {
                    message =
                        "Administrator account deleted successfully."
                });
            }
            catch (DbUpdateException)
            {
                return StatusCode(
                    StatusCodes.Status409Conflict,
                    new
                    {
                        message =
                            "This administrator account cannot be permanently deleted because related database records exist. Suspend the account instead."
                    });
            }
        }


        // =====================================================
        // ADMIN AUTH CHECK
        // =====================================================

        private bool IsCurrentUserAdmin()
        {
            var isAuthenticated =
                User.Identity?.IsAuthenticated
                ??
                false;


            if (!isAuthenticated)
            {
                return false;
            }


            var role =
                User.FindFirstValue(
                    ClaimTypes.Role)
                ??
                User.FindFirstValue(
                    "role");


            return string.Equals(
                role,
                "Admin",
                StringComparison.OrdinalIgnoreCase);
        }


        // =====================================================
        // CURRENT USER ID
        // =====================================================

        private int? GetCurrentUserId()
        {
            var value =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier)
                ??
                User.FindFirstValue(
                    "UserId")
                ??
                User.FindFirstValue(
                    "userId")
                ??
                User.FindFirstValue(
                    "userid")
                ??
                User.FindFirstValue(
                    "sub");


            if (int.TryParse(
                    value,
                    out var userId))
            {
                return userId;
            }


            return null;
        }


        // =====================================================
        // AUTHORIZATION RESPONSE
        // =====================================================

        private IActionResult
            AdministratorAuthorizationRequired()
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new
                {
                    message =
                        "Administrator authorization is required."
                });
        }


        // =====================================================
        // EMAIL VALIDATION
        // =====================================================

        private static bool IsValidEmail(
            string email)
        {
            try
            {
                var address =
                    new System.Net.Mail.MailAddress(
                        email);


                return address.Address.Equals(
                    email,
                    StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }


        // =====================================================
        // CLEAN CREATE REQUEST
        // =====================================================

        private static void CleanCreateRequest(
            AdminCreateRequest request)
        {
            request.FullName =
                request.FullName?.Trim()
                ??
                string.Empty;


            request.Email =
                request.Email?.Trim()
                    .ToLowerInvariant()
                ??
                string.Empty;


            request.Password =
                request.Password
                ??
                string.Empty;


            request.ConfirmPassword =
                request.ConfirmPassword
                ??
                string.Empty;


            request.Phone =
                request.Phone?.Trim();


            request.Location =
                request.Location?.Trim();


            request.AccountType =
                request.AccountType?.Trim()
                ??
                "Admin";
        }


        // =====================================================
        // CLEAN UPDATE REQUEST
        // =====================================================

        private static void CleanUpdateRequest(
            AdminUpdateRequest request)
        {
            request.FullName =
                request.FullName?.Trim()
                ??
                string.Empty;


            request.Email =
                request.Email?.Trim()
                    .ToLowerInvariant()
                ??
                string.Empty;


            request.Phone =
                request.Phone?.Trim();


            request.Location =
                request.Location?.Trim();


            request.AccountStatus =
                request.AccountStatus?.Trim()
                ??
                "Active";


            request.NewPassword =
                request.NewPassword
                ??
                string.Empty;


            request.ConfirmNewPassword =
                request.ConfirmNewPassword
                ??
                string.Empty;
        }


        // =====================================================
        // CREATE REQUEST MODEL
        // =====================================================

        public class AdminCreateRequest
        {
            public string FullName
            {
                get;
                set;
            } = string.Empty;


            public string Email
            {
                get;
                set;
            } = string.Empty;


            public string Password
            {
                get;
                set;
            } = string.Empty;


            public string ConfirmPassword
            {
                get;
                set;
            } = string.Empty;


            public string? Phone
            {
                get;
                set;
            }


            public string? Location
            {
                get;
                set;
            }


            public string AccountType
            {
                get;
                set;
            } = "Admin";
        }


        // =====================================================
        // UPDATE REQUEST MODEL
        // =====================================================

        public class AdminUpdateRequest
        {
            public string FullName
            {
                get;
                set;
            } = string.Empty;


            public string Email
            {
                get;
                set;
            } = string.Empty;


            public string? Phone
            {
                get;
                set;
            }


            public string? Location
            {
                get;
                set;
            }


            public string AccountStatus
            {
                get;
                set;
            } = "Active";


            public string NewPassword
            {
                get;
                set;
            } = string.Empty;


            public string ConfirmNewPassword
            {
                get;
                set;
            } = string.Empty;
        }
    }
}