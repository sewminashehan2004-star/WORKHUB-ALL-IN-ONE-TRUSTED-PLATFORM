namespace WorkHub.API.Models
{
    public class UserPublicProfile
    {
        public int UserPublicProfileId { get; set; }

        public int UserId { get; set; }

        // Kept for compatibility with the current database.
        // Not used in the current profile UI.
        public string? Headline { get; set; }

        public string? About { get; set; }

        public DateTime? DateOfBirth { get; set; }

        // Private identity field. Never exposed in public profile/search results.
        public string? NationalIdNumber { get; set; }

        public string? Country { get; set; }

        public string? Location { get; set; }

        public string? Phone { get; set; }

        public string? Website { get; set; }

        public string? LinkedInUrl { get; set; }

        public string? FacebookUrl { get; set; }

        public string? GitHubUrl { get; set; }

        public string? InstagramUrl { get; set; }

        public string? ProfileImageFileName { get; set; }

        public string? CoverImageFileName { get; set; }

        public DateTime UpdatedAt { get; set; }
            = DateTime.UtcNow;

        public User User { get; set; } = null!;

        public ICollection<UserPublicProfileSkill> Skills
        {
            get;
            set;
        } = new List<UserPublicProfileSkill>();

        public ICollection<UserPublicProfileEducation> Educations
        {
            get;
            set;
        } = new List<UserPublicProfileEducation>();
    }
}