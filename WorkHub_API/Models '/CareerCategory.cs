using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.Models
{
    public class CareerCategory
    {
        public int CareerCategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; }
            = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }
            = true;


        public ICollection<CareerRole> CareerRoles
        {
            get;
            set;
        } = new List<CareerRole>();
    }
}