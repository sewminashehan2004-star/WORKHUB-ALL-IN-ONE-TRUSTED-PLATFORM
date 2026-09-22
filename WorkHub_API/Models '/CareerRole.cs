using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class CareerRole
    {
        public int CareerRoleId { get; set; }

        public int CareerCategoryId { get; set; }


        [Required]
        [MaxLength(150)]
        public string RoleName { get; set; }
            = string.Empty;


        [MaxLength(1500)]
        public string? Description { get; set; }


        // Baseline fallback only.
        // Live WorkHub job data will later
        // improve the actual benchmark.

        public decimal? BaselineExperienceYears
        {
            get;
            set;
        }


        [MaxLength(500)]
        public string? BaselineEducationRequirement
        {
            get;
            set;
        }


        public bool IsActive { get; set; }
            = true;


        public CareerCategory CareerCategory
        {
            get;
            set;
        } = null!;


        public ICollection<CareerRoleSkill>
            CareerRoleSkills
        {
            get;
            set;
        } = new List<CareerRoleSkill>();
    }
}