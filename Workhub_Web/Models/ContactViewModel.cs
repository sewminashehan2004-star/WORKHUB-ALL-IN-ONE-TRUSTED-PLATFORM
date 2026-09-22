using System.ComponentModel.DataAnnotations;

namespace Workhub_Web.Models
{
    public class ContactViewModel
    {
        [Required(
            ErrorMessage =
                "Please enter your full name.")]
        [MaxLength(150)]
        [Display(Name = "Full Name")]
        public string FullName
        {
            get;
            set;
        } = string.Empty;


        [Required(
            ErrorMessage =
                "Please enter your email address.")]
        [EmailAddress(
            ErrorMessage =
                "Please enter a valid email address.")]
        [MaxLength(200)]
        [Display(Name = "Email Address")]
        public string Email
        {
            get;
            set;
        } = string.Empty;


        [Required(
            ErrorMessage =
                "Please select a topic.")]
        [MaxLength(100)]
        public string Topic
        {
            get;
            set;
        } = string.Empty;


        [Required(
            ErrorMessage =
                "Please enter your message.")]
        [MaxLength(
            4000,
            ErrorMessage =
                "Message cannot exceed 4000 characters.")]
        public string Message
        {
            get;
            set;
        } = string.Empty;


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
}