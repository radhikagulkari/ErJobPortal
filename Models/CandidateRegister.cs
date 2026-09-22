using Microsoft.AspNetCore.Http;

namespace ErJobPortal.Models
{
    public class CandidateRegister
    {
        public string? sFName { get; set; }

        public string? sLName { get; set; }

        public string? sMobile { get; set; }

        public string? sEmail { get; set; }

        public DateTime? DOB { get; set; }

        public int nGender { get; set; }

        // Profile Image
        public string? sProfileImage { get; set; }

        public IFormFile? ProfileImageFile { get; set; }

        // College
        public int nCollegeCode { get; set; }

        // College ID
        public int sCollegeName { get; set; }

        // Department ID
        public int nDepartment { get; set; }

        // Branch ID
        public int nBranch { get; set; }

        // Broad Group
        public int nBroadGroup { get; set; }

        // Passout Year
        public int? nPassoutYear { get; set; }

        // Password
        public string? sPassword { get; set; }

        // OTP
        public string? sOTP { get; set; }

        public string? sConfirmPassword { get; set; }

        public string? RegistrationType { get; set; }
    }
}