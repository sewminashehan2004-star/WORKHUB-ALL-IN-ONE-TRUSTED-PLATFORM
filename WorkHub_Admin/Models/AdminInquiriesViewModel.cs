namespace WorkHub_Admin.Models
{
    // =============================================
    // INQUIRIES LIST PAGE
    // =============================================

    public class AdminInquiriesViewModel
    {
        public List<AdminInquiryItemViewModel> Inquiries
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

        // =========================================
        // COUNTS
        // =========================================

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


    // =============================================
    // INQUIRY LIST ITEM
    // =============================================

    public class AdminInquiryItemViewModel
    {
        // Matches API: InquiryId
        public int InquiryId
        {
            get;
            set;
        }

        // Matches API: Name
        public string Name
        {
            get;
            set;
        } = string.Empty;

        // Matches API: Email
        public string Email
        {
            get;
            set;
        } = string.Empty;

        // Matches API: InquiryType
        public string InquiryType
        {
            get;
            set;
        } = string.Empty;

        // Matches API: Subject
        public string Subject
        {
            get;
            set;
        } = string.Empty;

        // Matches API: Message
        public string Message
        {
            get;
            set;
        } = string.Empty;

        // Matches API: Status
        public string Status
        {
            get;
            set;
        } = "New";

        // Matches API: CreatedAt
        public DateTime CreatedAt
        {
            get;
            set;
        }

        // Matches API: ReplyMessage
        public string? ReplyMessage
        {
            get;
            set;
        }

        // Matches API: RepliedAt
        public DateTime? RepliedAt
        {
            get;
            set;
        }

        // Matches API: RepliedBy
        public string? RepliedBy
        {
            get;
            set;
        }


        // =========================================
        // DISPLAY DATE
        // =========================================

        public string CreatedDateText
        {
            get
            {
                if (CreatedAt == default)
                {
                    return "-";
                }

                return
                    CreatedAt
                        .ToLocalTime()
                        .ToString(
                            "dd MMM yyyy · hh:mm tt");
            }
        }


        // =========================================
        // SHORT MESSAGE PREVIEW
        // =========================================

        public string ShortMessage
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Message))
                {
                    return string.Empty;
                }

                if (Message.Length <= 110)
                {
                    return Message;
                }

                return
                    Message.Substring(0, 110)
                    +
                    "...";
            }
        }
    }


    // =============================================
    // INQUIRY DETAILS PAGE
    // =============================================

    public class AdminInquiryDetailsViewModel
    {
        // Matches API: InquiryId
        public int InquiryId
        {
            get;
            set;
        }

        // Matches API: Name
        public string Name
        {
            get;
            set;
        } = string.Empty;

        // Matches API: Email
        public string Email
        {
            get;
            set;
        } = string.Empty;

        // Matches API: InquiryType
        public string InquiryType
        {
            get;
            set;
        } = string.Empty;

        // Matches API: Subject
        public string Subject
        {
            get;
            set;
        } = string.Empty;

        // Matches API: Message
        public string Message
        {
            get;
            set;
        } = string.Empty;

        // Matches API: Status
        public string Status
        {
            get;
            set;
        } = "New";

        // Matches API: CreatedAt
        public DateTime CreatedAt
        {
            get;
            set;
        }

        // Matches API: ReplyMessage
        public string? ReplyMessage
        {
            get;
            set;
        }

        // Matches API: RepliedAt
        public DateTime? RepliedAt
        {
            get;
            set;
        }

        // Matches API: RepliedBy
        public string? RepliedBy
        {
            get;
            set;
        }


        // =========================================
        // PAGE MESSAGES
        // =========================================

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


        // =========================================
        // CREATED DATE
        // =========================================

        public string CreatedDateText
        {
            get
            {
                if (CreatedAt == default)
                {
                    return "-";
                }

                return
                    CreatedAt
                        .ToLocalTime()
                        .ToString(
                            "dd MMM yyyy · hh:mm tt");
            }
        }


        // =========================================
        // REPLIED DATE
        // =========================================

        public string RepliedDateText
        {
            get
            {
                if (!RepliedAt.HasValue)
                {
                    return string.Empty;
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