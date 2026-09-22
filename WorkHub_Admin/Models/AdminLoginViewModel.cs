using System.ComponentModel.DataAnnotations;

namespace WorkHub_Admin.Models
{
    public class AdminLoginViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Admin Email")]
        public string Email { get; set; }
            = string.Empty;


        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
            = string.Empty;
    }


    // =========================================
    // RESPONSE RETURNED BY:
    // POST api/Auth/login
    // =========================================

    public class AdminLoginApiResponse
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