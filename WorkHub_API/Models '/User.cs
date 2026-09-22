using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(150)]
        public string? Location { get; set; }

        [Required]
        [MaxLength(30)]
        public string Role { get; set; } = "RegisteredUser";

        [Required]
        [MaxLength(20)]
        public string AccountStatus { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public JobSeekerProfile? JobSeekerProfile { get; set; }

        public ServiceProviderProfile? ServiceProviderProfile { get; set; }

        public MarketplaceSellerProfile? MarketplaceSellerProfile { get; set; }

        public CompanyProfile? CompanyProfile { get; set; }
    }
}