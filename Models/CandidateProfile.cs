namespace ErJobPortal.Models
{
    public class CandidateProfile
    {
        // =========================
        // ADDRESS
        // =========================

        public string? Country { get; set; }

        public string? State { get; set; }

        public string? City { get; set; }

        public string? Pincode { get; set; }


        // =========================
        // EDUCATION
        // =========================

        public int? SSCYear { get; set; }

        public string? SSCDivision { get; set; }

        public int? HSCDiplomaYear { get; set; }

        public string? HSCDiplomaDivision { get; set; }

        public int? GraduationYear { get; set; }

        public string? GraduationDivision { get; set; }

        public string? GraduationStream { get; set; }

        public string? OtherGraduationStream { get; set; }

        public int? PGYear { get; set; }

        public string? PGDivision { get; set; }

        public string? PGStream { get; set; }

        public string? OtherPGSpecialization { get; set; }


        // =========================
        // PHD
        // =========================

        public int? PhDYear { get; set; }

        public string? PhDStatus { get; set; }

        public string? PhDTopic { get; set; }

        public string? PreviousPhDTopic { get; set; }

        public int? PreviousPhDYear { get; set; }


        // =========================
        // INTERNSHIP / FELLOWSHIP
        // =========================

        public string? InternshipFellowshipType { get; set; }

        public string? PreferredCountry { get; set; }

        public string? PreferredState { get; set; }

        public string? PreferredCity { get; set; }


        // =========================
        // DOCUMENTS
        // =========================

        public IFormFile? ResumeFile { get; set; }

        public IFormFile? PhotoFile { get; set; }

        public IFormFile? SignatureFile { get; set; }

        public bool Divyang { get; set; }


        // =========================
        // HOBBIES
        // =========================

        public string? Hobby1 { get; set; }

        public string? Hobby2 { get; set; }

        public string? Hobby3 { get; set; }


        // =========================
        // INTERNSHIP 1
        // =========================

        public string? Internship1Organization { get; set; }

        public string? Internship1Title { get; set; }

        public string? Internship1Duration { get; set; }

        public string? Internship1Status { get; set; }


        // =========================
        // INTERNSHIP 2
        // =========================

        public string? Internship2Organization { get; set; }

        public string? Internship2Title { get; set; }

        public string? Internship2Duration { get; set; }

        public string? Internship2Status { get; set; }


        // =========================
        // INTERNSHIP 3
        // =========================

        public string? Internship3Organization { get; set; }

        public string? Internship3Title { get; set; }

        public string? Internship3Duration { get; set; }

        public string? Internship3Status { get; set; }


        // =========================
        // INTERNSHIP 4
        // =========================

        public string? Internship4Organization { get; set; }

        public string? Internship4Title { get; set; }

        public string? Internship4Duration { get; set; }

        public string? Internship4Status { get; set; }


        // =========================
        // LANGUAGES
        // =========================

        public string? Language1 { get; set; }

        public string? Language2 { get; set; }

        public string? Language3 { get; set; }

        public string? Language4 { get; set; }

        public string? Language5 { get; set; }

        public string? Language6 { get; set; }

        public string? Language7 { get; set; }

        public string? Language8 { get; set; }


        // =========================
        // REFERENCES
        // =========================

        public string? Reference1 { get; set; }

        public string? Reference2 { get; set; }


        // =========================
        // ACHIEVEMENTS
        // =========================

        public string? Achievement1 { get; set; }

        public string? Achievement2 { get; set; }

        public string? Achievement3 { get; set; }


        // =========================
        // LINKS
        // =========================

        public string? GitHub { get; set; }

        public string? LinkedIn { get; set; }


        // =========================
        // OBJECTIVE
        // =========================

        public string? Objective { get; set; }


        // =========================
        // RESUME PROFILE
        // =========================

        public int? ResumeProfile { get; set; }


        // =========================
        // SKILLS
        // =========================

        public string? SkillType { get; set; }

        public string? AdministrativeSkills { get; set; }

        public string? MedicalSkills { get; set; }

        public string? TechnicalSkills { get; set; }
    }
}