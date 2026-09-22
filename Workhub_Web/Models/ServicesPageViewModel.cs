namespace Workhub_Web.Models
{
    public class ServicesPageViewModel
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public string? Location { get; set; }
        public List<ServiceCategoryItemViewModel> Categories { get; set; } = new();
        public List<ServiceCardViewModel> Services { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class ServiceCategoryItemViewModel
    {
        public int ServiceCategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class ServiceCardViewModel
    {
        public int ServiceOfferingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal StartingPrice { get; set; }
        public string? Location { get; set; }
        public string Category { get; set; } = string.Empty;
        public ServiceProviderMiniViewModel Provider { get; set; } = new();
    }

    public class ServiceProviderMiniViewModel
    {
        public int ServiceProviderProfileId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public int? YearsOfExperience { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
    }
}
