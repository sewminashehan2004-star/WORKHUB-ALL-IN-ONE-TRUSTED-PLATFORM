using System.ComponentModel.DataAnnotations;

namespace Workhub_Web.Models
{
    public class ServiceProviderPortalViewModel
    {
        public ServiceProviderProfileEditViewModel Profile { get; set; } = new();
        public ServiceOfferingCreateViewModel NewOffering { get; set; } = new();
        public List<ServiceCategoryItemViewModel> Categories { get; set; } = new();
        public List<MyServiceOfferingViewModel> Offerings { get; set; } = new();
        public List<OpenServiceRequestViewModel> OpenRequests { get; set; } = new();
        public List<MyServiceOfferViewModel> MyOffers { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class ServiceProviderProfileEditViewModel
    {
        [MaxLength(100)] public string? DisplayName { get; set; }
        [MaxLength(1000)] public string? Bio { get; set; }
        [MaxLength(150)] public string? Location { get; set; }
        [Range(0, 50)] public int YearsOfExperience { get; set; }
        [Range(0, 100000000)] public decimal? StartingPrice { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
    }

    public class ServiceOfferingCreateViewModel
    {
        [Required] public int ServiceCategoryId { get; set; }
        [Required, MaxLength(150)] public string Title { get; set; } = string.Empty;
        [MaxLength(2000)] public string? Description { get; set; }
        [Range(0, 100000000)] public decimal StartingPrice { get; set; }
        [MaxLength(150)] public string? Location { get; set; }
        public bool IsAvailable { get; set; } = true;
    }

    public class MyServiceOfferingViewModel
    {
        public int ServiceOfferingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal StartingPrice { get; set; }
        public string? Location { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public ServiceCategoryItemViewModel Category { get; set; } = new();
    }

    public class OpenServiceRequestViewModel
    {
        public int ServiceRequestId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Location { get; set; }
        public DateTime? PreferredDate { get; set; }
        public decimal? Budget { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public ServiceCategoryItemViewModel Category { get; set; } = new();
    }

    public class MyServiceOfferViewModel
    {
        public int ServiceOfferId { get; set; }
        public int ServiceRequestId { get; set; }
        public decimal OfferedPrice { get; set; }
        public string? Message { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public ServiceOfferRequestMiniViewModel Request { get; set; } = new();
    }

    public class ServiceOfferRequestMiniViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Location { get; set; }
        public decimal? Budget { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
