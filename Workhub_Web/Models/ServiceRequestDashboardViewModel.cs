using System.ComponentModel.DataAnnotations;

namespace Workhub_Web.Models
{
    public class ServiceRequestDashboardViewModel
    {
        public CreateServiceRequestViewModel NewRequest { get; set; } = new();
        public List<ServiceCategoryItemViewModel> Categories { get; set; } = new();
        public List<MyServiceRequestViewModel> Requests { get; set; } = new();
        public Dictionary<int, List<ServiceRequestOfferViewModel>> OffersByRequest { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class CreateServiceRequestViewModel
    {
        [Required] public int ServiceCategoryId { get; set; }
        [Required, MaxLength(150)] public string Title { get; set; } = string.Empty;
        [Required, MaxLength(2000)] public string Description { get; set; } = string.Empty;
        [MaxLength(150)] public string? Location { get; set; }
        public DateTime? PreferredDate { get; set; }
        [Range(0, 100000000)] public decimal? Budget { get; set; }
    }

    public class MyServiceRequestViewModel
    {
        public int ServiceRequestId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Location { get; set; }
        public DateTime? PreferredDate { get; set; }
        public decimal? Budget { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Category { get; set; } = string.Empty;
        public int OfferCount { get; set; }
    }

    public class ServiceRequestOfferViewModel
    {
        public int ServiceOfferId { get; set; }
        public decimal OfferedPrice { get; set; }
        public string? Message { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public ServiceRequestProviderMiniViewModel Provider { get; set; } = new();
    }

    public class ServiceRequestProviderMiniViewModel
    {
        public int ServiceProviderProfileId { get; set; }
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? Location { get; set; }
        public int YearsOfExperience { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
    }
}
