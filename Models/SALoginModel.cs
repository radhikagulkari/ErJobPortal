using System.ComponentModel.DataAnnotations;

namespace ErJobPortal.Models
{
    public class SALoginModel
    {
        public int nID { get; set; }

        public int SAID { get; set; }

        public string? sFName { get; set; }

        public string? sLName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string sEmail { get; set; }

        public string? sMobile { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string sPassword { get; set; }

        public DateTime RegDate { get; set; }

        public DateTime? ModDate { get; set; }

        public bool nBit { get; set; }

        public string? sRole { get; set; }
    }
}
