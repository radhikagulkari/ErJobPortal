namespace ErJobPortal.Models
{
    public class SAObjectiveM
    {
        public int nID { get; set; }

        public int nGenderID { get; set; }

        public string sObjective { get; set; } = string.Empty;

        public DateTime dCreatedDate { get; set; }

        public bool nBit { get; set; }

        public bool nSABit { get; set; }
    }
}
