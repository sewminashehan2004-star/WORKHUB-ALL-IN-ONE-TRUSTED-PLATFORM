using System.ComponentModel.DataAnnotations;

namespace Workhub_Web.Models
{
    public class EditUserProfileViewModel
    {
        public int UserId { get; set; }


        [Required(ErrorMessage = "First name is required.")]
        [Display(Name = "First Name")]
        [MaxLength(60)]
        public string FirstName { get; set; }
            = string.Empty;


        [Required(ErrorMessage = "Last name is required.")]
        [Display(Name = "Last Name")]
        [MaxLength(100)]
        public string LastName { get; set; }
            = string.Empty;


        public string FullName
        {
            get
            {
                return
                    $"{FirstName} {LastName}"
                    .Trim();
            }
        }


        public string Email { get; set; }
            = string.Empty;


        [MaxLength(3000)]
        public string? About { get; set; }


        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }


        [Display(Name = "National ID / NIC")]
        [MaxLength(50)]
        public string? NationalIdNumber { get; set; }


        [MaxLength(100)]
        public string? Country { get; set; }


        [MaxLength(180)]
        public string? Location { get; set; }


        [MaxLength(50)]
        public string? Phone { get; set; }


        [MaxLength(300)]
        public string? Website { get; set; }


        [MaxLength(500)]
        public string? LinkedInUrl { get; set; }


        [MaxLength(500)]
        public string? FacebookUrl { get; set; }


        [MaxLength(500)]
        public string? GitHubUrl { get; set; }


        [MaxLength(500)]
        public string? InstagramUrl { get; set; }


        public string? ProfileImageUrl { get; set; }

        public string? CoverImageUrl { get; set; }
    }
}