namespace WorkHub.API.Models
{
    public class UserPublicProfileEducation
    {
        public int UserPublicProfileEducationId { get; set; }

        public int UserPublicProfileId { get; set; }

        public string Institution { get; set; } =
            string.Empty;

        public string Qualification { get; set; } =
            string.Empty;

        public string? FieldOfStudy { get; set; }

        public int? StartYear { get; set; }

        public int? EndYear { get; set; }

        public string? Description { get; set; }

        public UserPublicProfile UserPublicProfile
        {
            get;
            set;
        } = null!;
    }
}