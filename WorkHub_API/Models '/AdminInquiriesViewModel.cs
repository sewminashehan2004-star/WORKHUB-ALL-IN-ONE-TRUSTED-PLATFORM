namespace WorkHub_Admin.Models
{
    public class AdminInquiriesViewModel
    {
        public List<AdminInquiryItemViewModel>
            Inquiries
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


        public int TotalCount
        {
            get
            {
                return Inquiries.Count;
            }
        }


        public int NewCount
        {
            get
            {
                return Inquiries.Count(
                    x =>
                        string.Equals(
                            x.Status,
                            "New",
                            StringComparison.OrdinalIgnoreCase));
            }
        }


        public int RepliedCount
        {
            get
            {
                return Inquiries.Count(
                    x =>
                        string.Equals(
                            x.Status,
                            "Replied",
                            StringComparison.OrdinalIgnoreCase));
            }
        }
    }


    public class AdminInquiryItemViewModel
    {
        public int ContactInquiryId
        {
            get;
            set;
        }


        public string FullName
        {
            get;
            set;
        } = string.Empty;


        public string Email
        {
            get;
            set;
        } = string.Empty;


        public string Topic
        {
            get;
            set;
        } = string.Empty;


        public string Message
        {
            get;
            set;
        } = string.Empty;


        public string Status
        {
            get;
            set;
        } = "New";


        public DateTime CreatedAt
        {
            get;
            set;
        }


        public string? ReplyMessage
        {
            get;
            set;
        }


        public DateTime? RepliedAt
        {
            get;
            set;
        }


        public string? RepliedBy
        {
            get;
            set;
        }


        public string CreatedDateText
        {
            get
            {
                return
                    CreatedAt
                        .ToLocalTime()
                        .ToString(
                            "dd MMM yyyy · hh:mm tt");
            }
        }


        public string ShortMessage
        {
            get
            {
                if (string.IsNullOrWhiteSpace(
                        Message))
                {
                    return "";
                }


                if (Message.Length <= 110)
                {
                    return Message;
                }


                return
                    Message.Substring(
                        0,
                        110)
                    +
                    "...";
            }
        }
    }


    public class AdminInquiryDetailsViewModel
    {
        public int ContactInquiryId
        {
            get;
            set;
        }


        public string FullName
        {
            get;
            set;
        } = string.Empty;


        public string Email
        {
            get;
            set;
        } = string.Empty;


        public string Topic
        {
            get;
            set;
        } = string.Empty;


        public string Message
        {
            get;
            set;
        } = string.Empty;


        public string Status
        {
            get;
            set;
        } = "New";


        public DateTime CreatedAt
        {
            get;
            set;
        }


        public string? ReplyMessage
        {
            get;
            set;
        }


        public DateTime? RepliedAt
        {
            get;
            set;
        }


        public string? RepliedBy
        {
            get;
            set;
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


        public string CreatedDateText
        {
            get
            {
                return
                    CreatedAt
                        .ToLocalTime()
                        .ToString(
                            "dd MMM yyyy · hh:mm tt");
            }
        }


        public string RepliedDateText
        {
            get
            {
                if (!RepliedAt.HasValue)
                {
                    return "";
                }


                return
                    RepliedAt.Value
                        .ToLocalTime()
                        .ToString(
                            "dd MMM yyyy · hh:mm tt");
            }
        }
    }
}