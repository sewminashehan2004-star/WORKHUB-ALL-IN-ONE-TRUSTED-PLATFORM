using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class CareerRoleSkill
    {
        public int CareerRoleSkillId
        {
            get;
            set;
        }


        public int CareerRoleId
        {
            get;
            set;
        }


        public int SkillId
        {
            get;
            set;
        }


        [Required]
        [MaxLength(30)]
        public string Importance
        {
            get;
            set;
        } = "Required";


        public decimal? RecommendedYearsOfExperience
        {
            get;
            set;
        }


        public CareerRole CareerRole
        {
            get;
            set;
        } = null!;


        public Skill Skill
        {
            get;
            set;
        } = null!;
    }
}