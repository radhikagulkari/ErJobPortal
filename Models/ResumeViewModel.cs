

using System.Collections.Generic;

namespace ErJobPortal.Models
{
    public class ResumeViewModel
    {
        public CandidateProfileModel Profile { get; set; } = new CandidateProfileModel();
        public List<DropdownModel> Relationships { get; set; } = new List<DropdownModel>();
        public List<DropdownModel> Streams { get; set; } = new List<DropdownModel>();
        public List<DropdownModel> Divisions { get; set; } = new List<DropdownModel>();
        public List<DropdownModel> InternshipFellowshipType { get; set; } = new List<DropdownModel>();
        public List<DropdownModel> InternshipTitles { get; set; } = new List<DropdownModel>();
        public List<DropdownModel> InternshipDurations { get; set; } = new List<DropdownModel>();
        public List<DropdownModel> InternshipStatuses { get; set; } = new List<DropdownModel>();

        public int CandidateID { get; set; }
        public string CandidateName { get; set; } = "";
        public string CandidateEmail { get; set; } = "";
        public string CandidatePhone { get; set; } = "";
    }
}