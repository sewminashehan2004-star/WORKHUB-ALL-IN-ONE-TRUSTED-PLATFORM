namespace WorkHub_Admin.Models
{
    public class AdminNewsViewModel
    {
        public List<AdminNewsItemViewModel> News
        {
            get;
            set;
        } = new();

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

        public int TotalCount =>
            News.Count;

        public int PublishedCount =>
            News.Count(x => x.IsPublished);

        public int DraftCount =>
            News.Count(x => !x.IsPublished);
    }


    public class AdminNewsItemViewModel
    {
        public int NewsUpdateId
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        } = string.Empty;

        public string? Summary
        {
            get;
            set;
        }

        public string Content
        {
            get;
            set;
        } = string.Empty;

        public string Category
        {
            get;
            set;
        } = "General";

        public bool IsPublished
        {
            get;
            set;
        }

        public DateTime? PublishedAt
        {
            get;
            set;
        }

        public DateTime CreatedAt
        {
            get;
            set;
        }

        public DateTime? UpdatedAt
        {
            get;
            set;
        }

        public string? CreatedBy
        {
            get;
            set;
        }

        public string DisplayDate
        {
            get
            {
                var date =
                    PublishedAt
                    ?? UpdatedAt
                    ?? CreatedAt;

                return date
                    .ToLocalTime()
                    .ToString(
                        "dd MMM yyyy, hh:mm tt");
            }
        }
    }


    public class AdminNewsFormViewModel
    {
        public int NewsUpdateId
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        } = string.Empty;

        public string Summary
        {
            get;
            set;
        } = string.Empty;

        public string Content
        {
            get;
            set;
        } = string.Empty;

        public string Category
        {
            get;
            set;
        } = "General";

        public bool IsPublished
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
}