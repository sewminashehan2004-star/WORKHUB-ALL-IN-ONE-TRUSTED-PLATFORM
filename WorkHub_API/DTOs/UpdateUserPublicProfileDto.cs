using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class UpdateUserPublicProfileDto
    {
        [Required]
        [MaxLength(60)]
        public string FirstName { get; set; }
            = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }
            = string.Empty;

        [MaxLength(3000)]
        public string? About { get; set; }

        public DateTime? DateOfBirth { get; set; }

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
    }
}