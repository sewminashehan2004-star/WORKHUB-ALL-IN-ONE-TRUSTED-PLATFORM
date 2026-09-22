using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class RegisterRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(150)]
        public string? Location { get; set; }

        [Required]
        public string AccountType { get; set; } = "RegisteredUser";
    }
}