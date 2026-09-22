using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class CompanyRegisterRequestDto
    {
        // =====================================
        // COMPANY INFORMATION
        // =====================================

        [Required]
        [MaxLength(150)]
        public string CompanyName { get; set; }
            = string.Empty;


        // =====================================
        // ACCOUNT MANAGER / CONTACT PERSON
        //
        // This is the "username/person name"
        // for the current WorkHub model.
        // Login still uses email.
        // =====================================

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }
            = string.Empty;


        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; }
            = string.Empty;


        [MaxLength(20)]
        public string? Phone { get; set; }


        [Required]
        [MaxLength(150)]
        public string Location { get; set; }
            = string.Empty;


        // =====================================
        // PASSWORD
        // =====================================

        [Required]
        [MinLength(6)]
        public string Password { get; set; }
            = string.Empty;


        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
            = string.Empty;
    }
}