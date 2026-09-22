namespace WorkHub_Admin.Models
{
    public class AdminCompanyRequestViewModel
    {
        public int CompanyProfileId { get; set; }

        public int UserId { get; set; }

        public string CompanyName { get; set; }
            = string.Empty;

        public string ContactPerson { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public string? Phone { get; set; }

        public string? Location { get; set; }

        public string? Industry { get; set; }

        public string? Website { get; set; }

        public string? Description { get; set; }

        public string? Address { get; set; }

        public string AccountStatus { get; set; }
            = string.Empty;

        public string VerificationStatus { get; set; }
            = string.Empty;

        public bool IsActive { get; set; }

        public DateTime UserCreatedAt { get; set; }

        public DateTime CompanyCreatedAt { get; set; }

        public string SubmittedDateText =>
            CompanyCreatedAt
                .ToLocalTime()
                .ToString("dd MMM yyyy, hh:mm tt");
    }
}