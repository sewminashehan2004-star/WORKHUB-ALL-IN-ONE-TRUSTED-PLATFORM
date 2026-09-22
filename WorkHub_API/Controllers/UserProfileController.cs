using Microsoft.AspNetCore.Authorization;
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
    public class UserProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        private const long MaxImageSize =
            5 * 1024 * 1024;

        private static readonly string[] AllowedExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        public UserProfileController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }


        // =========================================
        // GET MY PROFILE
        // =========================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var user =
                await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.UserId == userId.Value);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            var profile =
                await _context.UserPublicProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.UserId == userId.Value);

            var names =
                SplitFullName(user.FullName);

            return Ok(new
            {
                user.UserId,

                fullName =
                    user.FullName,

                firstName =
                    names.FirstName,

                lastName =
                    names.LastName,

                user.Email,

                about =
                    profile?.About,

                dateOfBirth =
                    profile?.DateOfBirth,

                nationalIdNumber =
                    profile?.NationalIdNumber,

                country =
                    profile?.Country,

                location =
                    profile?.Location,

                phone =
                    profile?.Phone,

                website =
                    profile?.Website,

                linkedInUrl =
                    profile?.LinkedInUrl,

                facebookUrl =
                    profile?.FacebookUrl,

                gitHubUrl =
                    profile?.GitHubUrl,

                instagramUrl =
                    profile?.InstagramUrl,

                profileImageUrl =
                    profile?.ProfileImageFileName == null
                        ? null
                        : $"api/UserProfile/profile-image/{user.UserId}",

                coverImageUrl =
                    profile?.CoverImageFileName == null
                        ? null
                        : $"api/UserProfile/cover-image/{user.UserId}"
            });
        }


        // =========================================
        // UPDATE PROFILE
        // =========================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile(
            UpdateUserPublicProfileDto request)
        {
            var userId =
                GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(
                    request.FirstName))
            {
                return BadRequest(new
                {
                    message =
                        "First name is required."
                });
            }

            if (string.IsNullOrWhiteSpace(
                    request.LastName))
            {
                return BadRequest(new
                {
                    message =
                        "Last name is required."
                });
            }

            if (request.DateOfBirth.HasValue &&
                request.DateOfBirth.Value.Date >
                DateTime.UtcNow.Date)
            {
                return BadRequest(new
                {
                    message =
                        "Date of birth cannot be in the future."
                });
            }

            var user =
                await _context.Users
                    .FirstOrDefaultAsync(
                        x => x.UserId == userId.Value);

            if (user == null)
            {
                return NotFound(new
                {
                    message =
                        "User not found."
                });
            }

            var profile =
                await GetOrCreateProfileAsync(
                    userId.Value);

            var firstName =
                request.FirstName.Trim();

            var lastName =
                request.LastName.Trim();

            user.FullName =
                $"{firstName} {lastName}".Trim();

            profile.About =
                Clean(request.About);

            profile.DateOfBirth =
                request.DateOfBirth?.Date;

            profile.NationalIdNumber =
                Clean(request.NationalIdNumber);

            profile.Country =
                Clean(request.Country);

            profile.Location =
                Clean(request.Location);

            profile.Phone =
                Clean(request.Phone);

            profile.Website =
                Clean(request.Website);

            profile.LinkedInUrl =
                Clean(request.LinkedInUrl);

            profile.FacebookUrl =
                Clean(request.FacebookUrl);

            profile.GitHubUrl =
                Clean(request.GitHubUrl);

            profile.InstagramUrl =
                Clean(request.InstagramUrl);

            profile.UpdatedAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Profile updated successfully.",

                firstName,

                lastName,

                fullName =
                    user.FullName
            });
        }


        // =========================================
        // PROFILE IMAGE UPLOAD
        // =========================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpPost("profile-image")]
        [RequestSizeLimit(MaxImageSize)]
        public async Task<IActionResult>
            UploadProfileImage(
                IFormFile file)
        {
            var userId =
                GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var validationError =
                ValidateImage(file);

            if (validationError != null)
            {
                return BadRequest(new
                {
                    message =
                        validationError
                });
            }

            var profile =
                await GetOrCreateProfileAsync(
                    userId.Value);

            DeleteExistingFile(
                userId.Value,
                profile.ProfileImageFileName);

            profile.ProfileImageFileName =
                await SaveImageAsync(
                    userId.Value,
                    file,
                    "profile");

            profile.UpdatedAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Profile image uploaded successfully.",

                imageUrl =
                    $"api/UserProfile/profile-image/{userId.Value}"
            });
        }


        // =========================================
        // COVER IMAGE UPLOAD
        // =========================================

        [Authorize(Roles = "RegisteredUser")]
        [HttpPost("cover-image")]
        [RequestSizeLimit(MaxImageSize)]
        public async Task<IActionResult>
            UploadCoverImage(
                IFormFile file)
        {
            var userId =
                GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            var validationError =
                ValidateImage(file);

            if (validationError != null)
            {
                return BadRequest(new
                {
                    message =
                        validationError
                });
            }

            var profile =
                await GetOrCreateProfileAsync(
                    userId.Value);

            DeleteExistingFile(
                userId.Value,
                profile.CoverImageFileName);

            profile.CoverImageFileName =
                await SaveImageAsync(
                    userId.Value,
                    file,
                    "cover");

            profile.UpdatedAt =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message =
                    "Cover image uploaded successfully.",

                imageUrl =
                    $"api/UserProfile/cover-image/{userId.Value}"
            });
        }


        // =========================================
        // GET PROFILE IMAGE
        // =========================================

        [AllowAnonymous]
        [HttpGet("profile-image/{userId:int}")]
        public async Task<IActionResult>
            GetProfileImage(
                int userId)
        {
            var profile =
                await _context.UserPublicProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.UserId == userId);

            if (profile == null ||
                string.IsNullOrWhiteSpace(
                    profile.ProfileImageFileName))
            {
                return NotFound();
            }

            return GetImageFile(
                userId,
                profile.ProfileImageFileName);
        }


        // =========================================
        // GET COVER IMAGE
        // =========================================

        [AllowAnonymous]
        [HttpGet("cover-image/{userId:int}")]
        public async Task<IActionResult>
            GetCoverImage(
                int userId)
        {
            var profile =
                await _context.UserPublicProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.UserId == userId);

            if (profile == null ||
                string.IsNullOrWhiteSpace(
                    profile.CoverImageFileName))
            {
                return NotFound();
            }

            return GetImageFile(
                userId,
                profile.CoverImageFileName);
        }


        // =========================================
        // HELPERS
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


        private async Task<UserPublicProfile>
            GetOrCreateProfileAsync(
                int userId)
        {
            var profile =
                await _context.UserPublicProfiles
                    .FirstOrDefaultAsync(
                        x => x.UserId == userId);

            if (profile != null)
            {
                return profile;
            }

            profile =
                new UserPublicProfile
                {
                    UserId =
                        userId,

                    UpdatedAt =
                        DateTime.UtcNow
                };

            _context.UserPublicProfiles
                .Add(profile);

            await _context.SaveChangesAsync();

            return profile;
        }


        private static (
            string FirstName,
            string LastName)
            SplitFullName(
                string? fullName)
        {
            if (string.IsNullOrWhiteSpace(
                    fullName))
            {
                return (
                    string.Empty,
                    string.Empty);
            }

            var parts =
                fullName
                    .Trim()
                    .Split(
                        ' ',
                        StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
            {
                return (
                    parts[0],
                    string.Empty);
            }

            return (
                parts[0],
                string.Join(
                    " ",
                    parts.Skip(1)));
        }


        private static string? Clean(
            string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }


        private static string?
            ValidateImage(
                IFormFile? file)
        {
            if (file == null ||
                file.Length == 0)
            {
                return
                    "Please select an image.";
            }

            if (file.Length >
                MaxImageSize)
            {
                return
                    "Image must be 5 MB or smaller.";
            }

            var extension =
                Path.GetExtension(
                    file.FileName)
                    .ToLowerInvariant();

            if (!AllowedExtensions.Contains(
                    extension))
            {
                return
                    "Only JPG, JPEG, PNG and WEBP images are allowed.";
            }

            return null;
        }


        private async Task<string>
            SaveImageAsync(
                int userId,
                IFormFile file,
                string prefix)
        {
            var folder =
                GetUserImageFolder(
                    userId);

            Directory.CreateDirectory(
                folder);

            var extension =
                Path.GetExtension(
                    file.FileName)
                    .ToLowerInvariant();

            var fileName =
                $"{prefix}_{Guid.NewGuid():N}{extension}";

            var path =
                Path.Combine(
                    folder,
                    fileName);

            await using var stream =
                new FileStream(
                    path,
                    FileMode.Create);

            await file.CopyToAsync(
                stream);

            return fileName;
        }


        private void DeleteExistingFile(
            int userId,
            string? fileName)
        {
            if (string.IsNullOrWhiteSpace(
                    fileName))
            {
                return;
            }

            var path =
                Path.Combine(
                    GetUserImageFolder(
                        userId),
                    fileName);

            if (System.IO.File.Exists(
                    path))
            {
                System.IO.File.Delete(
                    path);
            }
        }


        private string GetUserImageFolder(
            int userId)
        {
            return Path.Combine(
                _environment.ContentRootPath,
                "Uploads",
                "UserProfiles",
                userId.ToString());
        }


        private IActionResult GetImageFile(
            int userId,
            string fileName)
        {
            var safeFileName =
                Path.GetFileName(fileName);

            var path =
                Path.Combine(
                    GetUserImageFolder(userId),
                    safeFileName);

            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }

            var extension =
                Path.GetExtension(path)
                    .ToLowerInvariant();

            var contentType =
                extension switch
                {
                    ".png" =>
                        "image/png",

                    ".webp" =>
                        "image/webp",

                    _ =>
                        "image/jpeg"
                };

            return PhysicalFile(
                path,
                contentType);
        }
    }
}