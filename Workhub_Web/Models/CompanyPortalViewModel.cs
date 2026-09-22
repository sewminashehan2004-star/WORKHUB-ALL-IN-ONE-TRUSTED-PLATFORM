using System.ComponentModel.DataAnnotations;

namespace Workhub_Web.Models
{
    // =========================================
    // COMPANY REGISTRATION REQUEST
    // =========================================

    public class CompanyRegisterViewModel
    {
        [Required]
        [MaxLength(150)]
        [Display(Name = "Company / Organization Name")]
        public string CompanyName { get; set; }
            = string.Empty;


        [Required]
        [MaxLength(100)]
        [Display(Name = "Contact Person Name")]
        public string FullName { get; set; }
            = string.Empty;


        [Required]
        [EmailAddress]
        [MaxLength(150)]
        [Display(Name = "Company Email")]
        public string Email { get; set; }
            = string.Empty;


        [MaxLength(20)]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }


        [Required]
        [MaxLength(150)]
        [Display(Name = "Location")]
        public string Location { get; set; }
            = string.Empty;


        [Required]
        [MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
            = string.Empty;


        [Required]
        [Compare(nameof(Password))]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
            = string.Empty;
    }


    // =========================================
    // COMPANY LOGIN
    // =========================================

    public class CompanyLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
            = string.Empty;


        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
            = string.Empty;
    }


    // =========================================
    // COMPANY PROFILE
    // =========================================

    public class CompanyProfileFormViewModel
    {
        public int CompanyProfileId { get; set; }


        [Required]
        [MaxLength(150)]
        [Display(Name = "Company / Organization Name")]
        public string CompanyName { get; set; }
            = string.Empty;


        [MaxLength(100)]
        public string? Industry { get; set; }


        [MaxLength(200)]
        public string? Website { get; set; }


        [MaxLength(1500)]
        [Display(Name = "Company Description")]
        public string? Description { get; set; }


        [MaxLength(200)]
        [Display(Name = "Business Address")]
        public string? Address { get; set; }


        public string? LogoPath { get; set; }


        public string VerificationStatus { get; set; }
            = "Pending";


        public bool IsActive { get; set; }


        public DateTime CreatedAt { get; set; }
    }


    // =========================================
    // COMPANY DASHBOARD
    // =========================================

    public class CompanyDashboardViewModel
    {
        public string FullName { get; set; }
            = string.Empty;


        public string Email { get; set; }
            = string.Empty;


        public bool CompanyProfileExists { get; set; }


        public CompanyProfileFormViewModel Company
        {
            get;
            set;
        } = new();


        public string? SuccessMessage { get; set; }


        public string? ErrorMessage { get; set; }
    }


    // =========================================
    // LOGIN API RESPONSE
    // =========================================

    public class CompanyLoginApiResponse
    {
        public int UserId { get; set; }


        public string FullName { get; set; }
            = string.Empty;


        public string Email { get; set; }
            = string.Empty;


        public string Role { get; set; }
            = string.Empty;


        public string Token { get; set; }
            = string.Empty;


        public DateTime ExpiresAt { get; set; }
    }
}