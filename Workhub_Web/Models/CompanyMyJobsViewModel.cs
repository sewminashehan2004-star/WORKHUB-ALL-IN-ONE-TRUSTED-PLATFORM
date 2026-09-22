namespace Workhub_Web.Models
{
    // =============================================
    // MY JOBS PAGE
    // =============================================

    public class CompanyMyJobsViewModel
    {
        public List<CompanyMyJobItemViewModel>
            Jobs
        { get; set; }
            = new();


        public int TotalJobs
        {
            get
            {
                return Jobs.Count;
            }
        }


        public int ActiveJobs
        {
            get
            {
                return Jobs.Count(x =>
                    string.Equals(
                        x.Status,
                        "Active",
                        StringComparison.OrdinalIgnoreCase));
            }
        }


        public int ClosedJobs
        {
            get
            {
                return Jobs.Count(x =>
                    string.Equals(
                        x.Status,
                        "Closed",
                        StringComparison.OrdinalIgnoreCase));
            }
        }


        public string? SuccessMessage
        {
            get;
            set;
        }


        public string? ErrorMessage
        {
            get;
            set;
        }
    }


    // =============================================
    // ONE COMPANY JOB
    // =============================================

    public class CompanyMyJobItemViewModel
    {
        public int JobId { get; set; }


        public string Title { get; set; }
            = string.Empty;


        public string? Category { get; set; }


        public int? CareerRoleId { get; set; }


        public string? CareerRoleName
        {
            get;
            set;
        }


        public int? CareerCategoryId
        {
            get;
            set;
        }


        public string? CareerCategoryName
        {
            get;
            set;
        }


        public string? Location { get; set; }


        public string? JobType { get; set; }


        public string? ImagePath { get; set; }


        public string Status { get; set; }
            = "Active";


        public DateTime? ClosingDate
        {
            get;
            set;
        }


        public DateTime CreatedAt
        {
            get;
            set;
        }


        // =========================================
        // DISPLAY HELPERS
        // =========================================

        public string CreatedDateText
        {
            get
            {
                return CreatedAt
                    .ToLocalTime()
                    .ToString("dd MMM yyyy");
            }
        }


        public string ClosingDateText
        {
            get
            {
                return ClosingDate.HasValue
                    ? ClosingDate.Value
                        .ToLocalTime()
                        .ToString("dd MMM yyyy")
                    : "No closing date";
            }
        }


        public bool IsActive
        {
            get
            {
                return string.Equals(
                    Status,
                    "Active",
                    StringComparison.OrdinalIgnoreCase);
            }
        }


        public bool IsClosed
        {
            get
            {
                return string.Equals(
                    Status,
                    "Closed",
                    StringComparison.OrdinalIgnoreCase);
            }
        }


        public bool IsNew
        {
            get
            {
                return CreatedAt >=
                    DateTime.UtcNow.AddDays(-7);
            }
        }
    }
}