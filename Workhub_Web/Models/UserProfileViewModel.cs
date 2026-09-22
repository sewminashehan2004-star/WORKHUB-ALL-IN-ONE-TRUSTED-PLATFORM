using System.Collections.Generic;
using System.Linq;

namespace Workhub_Web.Models
{
    public class UserProfileViewModel
    {
        public int UserId { get; set; }


        public string FullName { get; set; }
            = string.Empty;


        public string Email { get; set; }
            = string.Empty;


        public string Role { get; set; }
            = "RegisteredUser";


        public bool JobSeekerActive { get; set; }


        public bool ServiceProviderActive { get; set; }


        public bool MarketplaceSellerActive { get; set; }


        // =========================================
        // FIRST NAME
        // =========================================

        public string FirstName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(
                        FullName))
                {
                    return "User";
                }

                return FullName
                    .Trim()
                    .Split(
                        ' ',
                        StringSplitOptions
                            .RemoveEmptyEntries)
                    .FirstOrDefault()
                    ??
                    "User";
            }
        }


        // =========================================
        // INITIAL
        // =========================================

        public string Initial
        {
            get
            {
                if (string.IsNullOrWhiteSpace(
                        FullName))
                {
                    return "U";
                }

                return FullName
                    .Trim()
                    .Substring(0, 1)
                    .ToUpperInvariant();
            }
        }


        // =========================================
        // ACTIVE PROFILE COUNT
        // =========================================

        public int ActiveProfileCount
        {
            get
            {
                var count = 0;

                if (JobSeekerActive)
                {
                    count++;
                }

                if (ServiceProviderActive)
                {
                    count++;
                }

                if (MarketplaceSellerActive)
                {
                    count++;
                }

                return count;
            }
        }


        // =========================================
        // NOTIFICATIONS
        // =========================================

        public List<UserNotificationViewModel>
            Notifications
        { get; set; }
            = new();


        public int UnreadNotificationCount =>
            Notifications.Count(
                notification =>
                    !notification.IsRead);
    }


    // =================================================
    // USER NOTIFICATION
    // =================================================

    public class UserNotificationViewModel
    {
        public int UserNotificationId { get; set; }


        public int UserId { get; set; }


        public string Title { get; set; }
            = string.Empty;


        // IMPORTANT:
        // For InquiryReply this contains
        // the ACTUAL admin reply.
        public string Message { get; set; }
            = string.Empty;


        public string NotificationType { get; set; }
            = string.Empty;


        public int? RelatedInquiryId { get; set; }


        public bool IsRead { get; set; }


        public DateTime CreatedAt { get; set; }
    }
}