using System.ComponentModel.DataAnnotations;

namespace WorkHub.API.DTOs
{
    public class ServiceProviderProfileDto
    {
        [MaxLength(100)]
        public string? DisplayName { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }

        [MaxLength(150)]
        public string? Location { get; set; }

        [Range(0, 50)]
        public int YearsOfExperience { get; set; }

        [Range(0, 100000000)]
        public decimal? StartingPrice { get; set; }
    }
}