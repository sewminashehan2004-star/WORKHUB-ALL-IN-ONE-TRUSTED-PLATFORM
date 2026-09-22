namespace WorkHub.API.Models
{
    public class UserPublicProfileSkill
    {
        public int UserPublicProfileSkillId { get; set; }

        public int UserPublicProfileId { get; set; }

        public string SkillName { get; set; } =
            string.Empty;

        public UserPublicProfile UserPublicProfile
        {
            get;
            set;
        } = null!;
    }
}