using System.Collections.Generic;

namespace ErJobPortal.Models
{
    public class CandidateProfileViewModel
    {
        public CandidateProfileModel Profile { get; set; }
            = new CandidateProfileModel();


        // EDUCATION

        public List<DropdownModel> Divisions { get; set; }
            = new List<DropdownModel>();

        public List<DropdownModel> Streams { get; set; }
            = new List<DropdownModel>();

        public List<DropdownModel> GraduationStatuses { get; set; }
            = new List<DropdownModel>();


        // ADDRESS

        public List<DropdownModel> Countries { get; set; }
            = new List<DropdownModel>();

        public List<DropdownModel> States { get; set; }
            = new List<DropdownModel>();

        public List<DropdownModel> Cities { get; set; }
            = new List<DropdownModel>();


        // INTERNSHIP / FELLOWSHIP

        public List<DropdownModel> InternshipFellowshipType { get; set; }
            = new List<DropdownModel>();

        public List<DropdownModel> InternshipTitles { get; set; }
            = new List<DropdownModel>();

        public List<DropdownModel> InternshipDurations { get; set; }
            = new List<DropdownModel>();

        public List<DropdownModel> InternshipStatuses { get; set; }
            = new List<DropdownModel>();


        // REFERENCES

        public List<DropdownModel> Relationships { get; set; }
            = new List<DropdownModel>();
    }
}