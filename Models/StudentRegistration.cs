using System.ComponentModel.DataAnnotations;

namespace ErJobPortal.Models
{
    public class StudentRegistration
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string MobileNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string CollegeName { get; set; }

        [Required]
        public string BranchName { get; set; }

        [Required]
        public int PassingYear { get; set; }

        public string Gender { get; set; }


        public string Password { get; set; }


        public string ConfirmPassword { get; set; }


        // OTP Verification Type
        public string OTPType { get; set; }


        public string Captcha { get; set; }


        // Captcha value generated internally
        public string CaptchaCode { get; set; }
    }
}