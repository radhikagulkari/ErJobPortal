using System.ComponentModel.DataAnnotations;

namespace ErJobPortal.Models
{
    public class FeedbackM
    {
        [Key]
        public int nID { get; set; }

        public int? sQue1 { get; set; }

        public int? sQue2 { get; set; }

        public int? sQue3 { get; set; }

        public int? sQue4 { get; set; }

        public string? sQue5 { get; set; }

        public int? nAdminID { get; set; }

        public DateTime? RegDate { get; set; }

        public DateTime? ModDate { get; set; }

        public bool nBit { get; set; } = true;

        public bool nSABit { get; set; } = false;
    }
}
