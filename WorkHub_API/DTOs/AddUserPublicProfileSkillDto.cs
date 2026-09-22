using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class AddUserPublicProfileSkillDto
    {
        [Required]
        [MaxLength(100)]
        public string SkillName { get; set; } =
            string.Empty;
    }
}