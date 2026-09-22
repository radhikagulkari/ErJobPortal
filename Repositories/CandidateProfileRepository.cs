using ErJobPortal.Data;
using ErJobPortal.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ErJobPortal.Repositories
{
    public class CandidateProfileRepository
    {
        private readonly DbConnection _db;

        public CandidateProfileRepository(DbConnection db)
        {
            _db = db;
        }


        // =========================================================
        // ENSURE PROFILE EXISTS
        // =========================================================

        private void EnsureProfileExists(SqlConnection con, int candidateId)
        {
            string sql = @"
        IF NOT EXISTS
        (
            SELECT 1
            FROM tblCandidateProfile
            WHERE CandidateID = @CandidateID
        )
        BEGIN
            INSERT INTO tblCandidateProfile
            (
                CandidateID,
                RegDate,
                ModDate,
                nBit,
                nSABit
            )
            VALUES
            (
                @CandidateID,
                GETDATE(),
                GETDATE(),
                1,
                1
            )
        END";

            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@CandidateID", SqlDbType.Int)
                .Value = candidateId;

            cmd.ExecuteNonQuery();
        }


        // =========================================================
        // GET PROFILE
        // =========================================================

        // =========================================================
        // GET PROFILE
        // =========================================================

        public CandidateProfileModel? GetProfile(int candidateId)
        {
            CandidateProfileModel? profile = null;

            using SqlConnection con = _db.GetConnection();

            string query = @"
        SELECT *
        FROM tblCandidateProfile
        WHERE CandidateID = @CandidateID
          AND ISNULL(nBit, 1) = 1";

            using SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.Add("@CandidateID", SqlDbType.Int)
                .Value = candidateId;

            con.Open();

            using SqlDataReader dr = cmd.ExecuteReader();

            if (!dr.Read())
            {
                return null;
            }

            profile = new CandidateProfileModel
            {
                CandidateID = candidateId,

                // =====================================================
                // ADDRESS
                // =====================================================

                CountryID = GetString(dr, "CountryID"),
                StateID = GetString(dr, "StateID"),
                CityID = GetString(dr, "CityID"),
                Pincode = GetString(dr, "Pincode"),


                // =====================================================
                // INTERNSHIP / FELLOWSHIP PREFERENCE
                // =====================================================

                Internship_FellowshipType =
                    GetNullableInt(dr, "Internship_FellowshipType"),

                Preferred_Country =
                    GetString(dr, "Preferred_Country"),

                Preferred_State =
                    GetString(dr, "Preferred_State"),

                Preferred_City =
                    GetString(dr, "Preferred_City"),


                // =====================================================
                // EDUCATION
                // =====================================================

                SSC_YEAR =
                    GetNullableInt(dr, "SSC_YEAR"),

                SSC_DIVISION =
                    GetNullableInt(dr, "SSC_DIVISION"),

                HSC_DIPLOMA_YEAR =
                    GetNullableInt(dr, "HSC_DIPLOMA_YEAR"),

                HSC_DIPLOMA_DIVISION =
                    GetNullableInt(dr, "HSC_DIPLOMA_DIVISION"),

                Graduation_Year =
                    GetNullableInt(dr, "Graduation_Year"),

                Graduation_Stream =
                    GetNullableInt(dr, "Graduation_Stream"),

                Graduation_Division =
                    GetNullableInt(dr, "Graduation_Division"),

                Other_Stream =
                    GetString(dr, "Other_Stream"),

                PG_Year =
                    GetNullableInt(dr, "PG_Year"),

                PG_Stream =
                    GetNullableInt(dr, "PG_Stream"),

                PG_Division =
                    GetNullableInt(dr, "PG_Division"),

                Other_Specialization =
                    GetString(dr, "Other_Specialization"),

                PhD_Year =
                    GetNullableInt(dr, "PhD_Year"),

                PhD_Status =
                    GetNullableInt(dr, "PhD_Status"),

                PhD_Topic =
                    GetString(dr, "PhD_Topic"),

                Previous_PhD_Topic_Year =
                    GetString(dr, "Previous_PhD_Topic_Year"),


                // =====================================================
                // SKILLS
                // =====================================================

                sMedicalSkillIDs =
                    GetString(dr, "sMedicalSkillIDs"),

                sMedicalSkillStarIDs =
                    GetString(dr, "sMedicalSkillStarIDs"),

                sTechnicalSkillIDs =
                    GetString(dr, "sTechnicalSkillIDs"),

                sTechnicalSkillStarIDs =
                    GetString(dr, "sTechnicalSkillStarIDs"),

                sNonTechnicalSkillIDs =
                    GetString(dr, "sNonTechnicalSkillIDs"),

                sNonTechnicalSkillStarIDs =
                    GetString(dr, "sNonTechnicalSkillStarIDs"),


                // =====================================================
                // LANGUAGES
                // =====================================================

                sLanguage1 =
                    GetString(dr, "sLanguage1"),

                sLanguageStar1 =
                    GetNullableInt(dr, "sLanguageStar1"),

                sLanguage2 =
                    GetString(dr, "sLanguage2"),

                sLanguageStar2 =
                    GetNullableInt(dr, "sLanguageStar2"),

                sLanguage3 =
                    GetString(dr, "sLanguage3"),

                sLanguageStar3 =
                    GetNullableInt(dr, "sLanguageStar3"),

                sLanguage4 =
                    GetString(dr, "sLanguage4"),

                sLanguageStar4 =
                    GetNullableInt(dr, "sLanguageStar4"),

                sLanguage5 =
                    GetString(dr, "sLanguage5"),

                sLanguageStar5 =
                    GetNullableInt(dr, "sLanguageStar5"),

                sLanguage6 =
                    GetString(dr, "sLanguage6"),

                sLanguageStar6 =
                    GetNullableInt(dr, "sLanguageStar6"),

                sLanguage7 =
                    GetString(dr, "sLanguage7"),

                sLanguageStar7 =
                    GetNullableInt(dr, "sLanguageStar7"),

                sLanguage8 =
                    GetString(dr, "sLanguage8"),

                sLanguageStar8 =
                    GetNullableInt(dr, "sLanguageStar8"),


                // =====================================================
                // REFERENCES
                // =====================================================

                sRefName1 =
                    GetString(dr, "sRefName1"),

                sRefRelationName1 =
                    GetNullableInt(dr, "sRefRelationName1"),

                sLocation1 =
                    GetString(dr, "sLocation1"),

                sMobile1 =
                    GetString(dr, "sMobile1"),

                sRefName2 =
                    GetString(dr, "sRefName2"),

                sRefRelationName2 =
                    GetNullableInt(dr, "sRefRelationName2"),

                sLocation2 =
                    GetString(dr, "sLocation2"),

                sMobile2 =
                    GetString(dr, "sMobile2"),


                // =====================================================
                // SOCIAL LINKS
                // =====================================================

                GitHub =
                    GetString(dr, "GitHub"),

                Linkedin =
                    GetString(dr, "Linkedin"),


                // =====================================================
                // INTERNSHIP 1
                // =====================================================

                sOrgName1 =
                    GetString(dr, "sOrgName1"),

                sOrgIntTitle1 =
                    GetNullableInt(dr, "sOrgIntTitle1"),

                sOrgIntDuration1 =
                    GetNullableInt(dr, "sOrgIntDuration1"),

                sOrgIntStatus1 =
                    GetNullableInt(dr, "sOrgIntStatus1"),


                // =====================================================
                // INTERNSHIP 2
                // =====================================================

                sOrgName2 =
                    GetString(dr, "sOrgName2"),

                sOrgIntTitle2 =
                    GetNullableInt(dr, "sOrgIntTitle2"),

                sOrgIntDuration2 =
                    GetNullableInt(dr, "sOrgIntDuration2"),

                sOrgIntStatus2 =
                    GetNullableInt(dr, "sOrgIntStatus2"),


                // =====================================================
                // INTERNSHIP 3
                // =====================================================

                sOrgName3 =
                    GetString(dr, "sOrgName3"),

                sOrgIntTitle3 =
                    GetNullableInt(dr, "sOrgIntTitle3"),

                sOrgIntDuration3 =
                    GetNullableInt(dr, "sOrgIntDuration3"),

                sOrgIntStatus3 =
                    GetNullableInt(dr, "sOrgIntStatus3"),


                // =====================================================
                // INTERNSHIP 4
                // =====================================================

                sOrgName4 =
                    GetString(dr, "sOrgName4"),

                sOrgIntTitle4 =
                    GetNullableInt(dr, "sOrgIntTitle4"),

                sOrgIntDuration4 =
                    GetNullableInt(dr, "sOrgIntDuration4"),

                sOrgIntStatus4 =
                    GetNullableInt(dr, "sOrgIntStatus4"),


                // =====================================================
                // DOCUMENTS
                // =====================================================

                sResume =
                    GetString(dr, "sResume"),

                sPhoto =
                    GetString(dr, "sPhoto"),

                sSignature =
                    GetString(dr, "sSignature"),

                sDivyang =
                    GetString(dr, "sDivyang"),

                sHobbies =
                    GetString(dr, "sHobbies"),


                // =====================================================
                // OBJECTIVE
                // =====================================================

                Objective =
                    GetString(dr, "Objective"),

                Resume_Profile =
                    GetNullableInt(dr, "Resume_Profile"),


                // =====================================================
                // ACHIEVEMENTS
                // =====================================================

                Achievements_Certification1 =
                    GetString(dr, "Achievements_Certification1"),

                Achievements_Certification2 =
                    GetString(dr, "Achievements_Certification2"),

                Achievements_Certification3 =
                    GetString(dr, "Achievements_Certification3")
            };

            return profile;
        }


        // =========================================================
        // UPDATE ADDRESS
        // =========================================================

        // =========================================================
        // UPDATE ADDRESS
        // =========================================================

        public void UpdateAddress(
            int candidateId,
            string? countryId,
            string? stateId,
            string? cityId,
            string? pincode)
        {
            using SqlConnection con = _db.GetConnection();

            con.Open();

            EnsureProfileExists(con, candidateId);

            string sql = @"
        UPDATE tblCandidateProfile
        SET
            CountryID = @CountryID,
            StateID = @StateID,
            CityID = @CityID,
            Pincode = @Pincode,
            ModDate = GETDATE()
        WHERE CandidateID = @CandidateID";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.Add(
                "@CandidateID",
                SqlDbType.Int
            ).Value = candidateId;

            cmd.Parameters.Add(
                "@CountryID",
                SqlDbType.NVarChar,
                100
            ).Value = (object?)countryId ?? DBNull.Value;

            cmd.Parameters.Add(
                "@StateID",
                SqlDbType.NVarChar,
                100
            ).Value = (object?)stateId ?? DBNull.Value;

            cmd.Parameters.Add(
                "@CityID",
                SqlDbType.NVarChar,
                100
            ).Value = (object?)cityId ?? DBNull.Value;

            cmd.Parameters.Add(
                "@Pincode",
                SqlDbType.NVarChar,
                20
            ).Value = (object?)pincode ?? DBNull.Value;

            cmd.ExecuteNonQuery();
        }

        // =========================================================
        // UPDATE EDUCATION
        // =========================================================

        public void UpdateEducation(CandidateProfileModel model)
        {
            using SqlConnection con = _db.GetConnection();

            con.Open();

            EnsureProfileExists(con, model.CandidateID);

            string sql = @"

        UPDATE tblCandidateProfile
        SET

            SSC_YEAR = @SSC_YEAR,
            SSC_DIVISION = @SSC_DIVISION,

            HSC_DIPLOMA_YEAR = @HSC_DIPLOMA_YEAR,
            HSC_DIPLOMA_DIVISION = @HSC_DIPLOMA_DIVISION,

            Graduation_Year = @Graduation_Year,
            Graduation_Division = @Graduation_Division,
            Graduation_Stream = @Graduation_Stream,
            Other_Stream = @Other_Stream,

            PG_Year = @PG_Year,
            PG_Division = @PG_Division,
            PG_Stream = @PG_Stream,
            Other_Specialization = @Other_Specialization,

            PhD_Year = @PhD_Year,
            PhD_Status = @PhD_Status,
            PhD_Topic = @PhD_Topic,
            Previous_PhD_Topic_Year = @Previous_PhD_Topic_Year,

            ModDate = GETDATE()

        WHERE CandidateID = @CandidateID";

            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@CandidateID", SqlDbType.Int)
                .Value = model.CandidateID;

            cmd.Parameters.Add("@SSC_YEAR", SqlDbType.Int)
                .Value = (object?)model.SSC_YEAR ?? DBNull.Value;

            cmd.Parameters.Add("@SSC_DIVISION", SqlDbType.Int)
                .Value = (object?)model.SSC_DIVISION ?? DBNull.Value;

            cmd.Parameters.Add("@HSC_DIPLOMA_YEAR", SqlDbType.Int)
                .Value = (object?)model.HSC_DIPLOMA_YEAR ?? DBNull.Value;

            cmd.Parameters.Add("@HSC_DIPLOMA_DIVISION", SqlDbType.Int)
                .Value = (object?)model.HSC_DIPLOMA_DIVISION ?? DBNull.Value;

            cmd.Parameters.Add("@Graduation_Year", SqlDbType.Int)
                .Value = (object?)model.Graduation_Year ?? DBNull.Value;

            cmd.Parameters.Add("@Graduation_Division", SqlDbType.Int)
                .Value = (object?)model.Graduation_Division ?? DBNull.Value;

            cmd.Parameters.Add("@Graduation_Stream", SqlDbType.Int)
                .Value = (object?)model.Graduation_Stream ?? DBNull.Value;

            cmd.Parameters.Add("@Other_Stream", SqlDbType.NVarChar, 100)
                .Value = (object?)model.Other_Stream ?? DBNull.Value;

            cmd.Parameters.Add("@PG_Year", SqlDbType.Int)
                .Value = (object?)model.PG_Year ?? DBNull.Value;

            cmd.Parameters.Add("@PG_Division", SqlDbType.Int)
                .Value = (object?)model.PG_Division ?? DBNull.Value;

            cmd.Parameters.Add("@PG_Stream", SqlDbType.Int)
                .Value = (object?)model.PG_Stream ?? DBNull.Value;

            cmd.Parameters.Add("@Other_Specialization", SqlDbType.NVarChar, 100)
                .Value = (object?)model.Other_Specialization ?? DBNull.Value;

            cmd.Parameters.Add("@PhD_Year", SqlDbType.Int)
                .Value = (object?)model.PhD_Year ?? DBNull.Value;

            cmd.Parameters.Add("@PhD_Status", SqlDbType.Int)
                .Value = (object?)model.PhD_Status ?? DBNull.Value;

            cmd.Parameters.Add("@PhD_Topic", SqlDbType.NVarChar, 300)
                .Value = (object?)model.PhD_Topic ?? DBNull.Value;

            cmd.Parameters.Add("@Previous_PhD_Topic_Year", SqlDbType.NVarChar, 300)
                .Value = (object?)model.Previous_PhD_Topic_Year ?? DBNull.Value;

            cmd.ExecuteNonQuery();
        }


        // =========================================================
        // UPDATE INTERNSHIP / FELLOWSHIP PREFERENCE
        // =========================================================

        public void UpdateInternshipPreference(CandidateProfileModel model)
        {
            using SqlConnection con = _db.GetConnection();

            con.Open();

            EnsureProfileExists(con, model.CandidateID);

            string sql = @"
        UPDATE tblCandidateProfile
        SET
            Internship_FellowshipType = @Internship_FellowshipType,
            Preferred_Country = @Preferred_Country,
            Preferred_State = @Preferred_State,
            Preferred_City = @Preferred_City,
            ModDate = GETDATE()
        WHERE CandidateID = @CandidateID";

            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@CandidateID", SqlDbType.Int)
                .Value = model.CandidateID;

            cmd.Parameters.Add(
                "@Internship_FellowshipType",
                SqlDbType.Int
            ).Value = (object?)model.Internship_FellowshipType ?? DBNull.Value;

            cmd.Parameters.Add(
                "@Preferred_Country",
                SqlDbType.NVarChar,
                100
            ).Value = (object?)model.Preferred_Country ?? DBNull.Value;

            cmd.Parameters.Add(
                "@Preferred_State",
                SqlDbType.NVarChar,
                100
            ).Value = (object?)model.Preferred_State ?? DBNull.Value;

            cmd.Parameters.Add(
                "@Preferred_City",
                SqlDbType.NVarChar,
                100
            ).Value = (object?)model.Preferred_City ?? DBNull.Value;

            cmd.ExecuteNonQuery();
        }


        // =========================================================
        // UPDATE INTERNSHIP DETAILS
        // =========================================================

        public void UpdateInternshipDetails(CandidateProfileModel model)
        {
            using SqlConnection con = _db.GetConnection();
            con.Open();
            EnsureProfileExists(con, model.CandidateID);

            string sql = @"
UPDATE tblCandidateProfile
SET

    sOrgName1 = @sOrgName1,
    sOrgIntTitle1 = @sOrgIntTitle1,
    sOrgIntDuration1 = @sOrgIntDuration1,
    sOrgIntStatus1 = @sOrgIntStatus1,

    sOrgName2 = @sOrgName2,
    sOrgIntTitle2 = @sOrgIntTitle2,
    sOrgIntDuration2 = @sOrgIntDuration2,
    sOrgIntStatus2 = @sOrgIntStatus2,

    sOrgName3 = @sOrgName3,
    sOrgIntTitle3 = @sOrgIntTitle3,
    sOrgIntDuration3 = @sOrgIntDuration3,
    sOrgIntStatus3 = @sOrgIntStatus3,

    sOrgName4 = @sOrgName4,
    sOrgIntTitle4 = @sOrgIntTitle4,
    sOrgIntDuration4 = @sOrgIntDuration4,
    sOrgIntStatus4 = @sOrgIntStatus4,

    ModDate = GETDATE()

WHERE CandidateID = @CandidateID";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.Add("@CandidateID", SqlDbType.Int)
                .Value = model.CandidateID;

            AddNullableString(cmd, "@sOrgName1", model.sOrgName1, 300);
            AddNullableInt(cmd, "@sOrgIntTitle1", model.sOrgIntTitle1);
            AddNullableInt(cmd, "@sOrgIntDuration1", model.sOrgIntDuration1);
            AddNullableInt(cmd, "@sOrgIntStatus1", model.sOrgIntStatus1);

            AddNullableString(cmd, "@sOrgName2", model.sOrgName2, 300);
            AddNullableInt(cmd, "@sOrgIntTitle2", model.sOrgIntTitle2);
            AddNullableInt(cmd, "@sOrgIntDuration2", model.sOrgIntDuration2);
            AddNullableInt(cmd, "@sOrgIntStatus2", model.sOrgIntStatus2);

            AddNullableString(cmd, "@sOrgName3", model.sOrgName3, 300);
            AddNullableInt(cmd, "@sOrgIntTitle3", model.sOrgIntTitle3);
            AddNullableInt(cmd, "@sOrgIntDuration3", model.sOrgIntDuration3);
            AddNullableInt(cmd, "@sOrgIntStatus3", model.sOrgIntStatus3);

            AddNullableString(cmd, "@sOrgName4", model.sOrgName4, 300);
            AddNullableInt(cmd, "@sOrgIntTitle4", model.sOrgIntTitle4);
            AddNullableInt(cmd, "@sOrgIntDuration4", model.sOrgIntDuration4);
            AddNullableInt(cmd, "@sOrgIntStatus4", model.sOrgIntStatus4);

            cmd.ExecuteNonQuery();
        }


        // =========================================================
        // UPDATE LANGUAGES
        // =========================================================

        public void UpdateLanguages(
            CandidateProfileModel model)
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            EnsureProfileExists(con, model.CandidateID);

            string sql = @"
UPDATE tblCandidateProfile
SET

    sLanguage1 = @sLanguage1,
    sLanguage2 = @sLanguage2,
    sLanguage3 = @sLanguage3,
    sLanguage4 = @sLanguage4,
    sLanguage5 = @sLanguage5,
    sLanguage6 = @sLanguage6,
    sLanguage7 = @sLanguage7,
    sLanguage8 = @sLanguage8,

    sLanguageStar1 = @sLanguageStar1,
    sLanguageStar2 = @sLanguageStar2,
    sLanguageStar3 = @sLanguageStar3,
    sLanguageStar4 = @sLanguageStar4,
    sLanguageStar5 = @sLanguageStar5,
    sLanguageStar6 = @sLanguageStar6,
    sLanguageStar7 = @sLanguageStar7,
    sLanguageStar8 = @sLanguageStar8,

    ModDate = GETDATE()

WHERE CandidateID = @CandidateID";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.Add("@CandidateID", SqlDbType.Int)
                .Value = model.CandidateID;

            AddNullableString(cmd, "@sLanguage1", model.sLanguage1, 100);
            AddNullableString(cmd, "@sLanguage2", model.sLanguage2, 100);
            AddNullableString(cmd, "@sLanguage3", model.sLanguage3, 100);
            AddNullableString(cmd, "@sLanguage4", model.sLanguage4, 100);
            AddNullableString(cmd, "@sLanguage5", model.sLanguage5, 100);
            AddNullableString(cmd, "@sLanguage6", model.sLanguage6, 100);
            AddNullableString(cmd, "@sLanguage7", model.sLanguage7, 100);
            AddNullableString(cmd, "@sLanguage8", model.sLanguage8, 100);

            AddNullableInt(cmd, "@sLanguageStar1", model.sLanguageStar1);
            AddNullableInt(cmd, "@sLanguageStar2", model.sLanguageStar2);
            AddNullableInt(cmd, "@sLanguageStar3", model.sLanguageStar3);
            AddNullableInt(cmd, "@sLanguageStar4", model.sLanguageStar4);
            AddNullableInt(cmd, "@sLanguageStar5", model.sLanguageStar5);
            AddNullableInt(cmd, "@sLanguageStar6", model.sLanguageStar6);
            AddNullableInt(cmd, "@sLanguageStar7", model.sLanguageStar7);
            AddNullableInt(cmd, "@sLanguageStar8", model.sLanguageStar8);

            cmd.ExecuteNonQuery();
        }


        // =========================================================
        // UPDATE REFERENCES
        // =========================================================

        public void UpdateReferences(
            CandidateProfileModel model)
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            EnsureProfileExists(con, model.CandidateID);

            string sql = @"
UPDATE tblCandidateProfile
SET

    sRefName1 = @sRefName1,
    sRefName2 = @sRefName2,

    sRefRelationName1 = @sRefRelationName1,
    sRefRelationName2 = @sRefRelationName2,

    sLocation1 = @sLocation1,
    sLocation2 = @sLocation2,

    sMobile1 = @sMobile1,
    sMobile2 = @sMobile2,

    ModDate = GETDATE()

WHERE CandidateID = @CandidateID";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.Add("@CandidateID", SqlDbType.Int)
                .Value = model.CandidateID;

            AddNullableString(cmd, "@sRefName1", model.sRefName1, 200);
            AddNullableString(cmd, "@sRefName2", model.sRefName2, 200);

            AddNullableInt(cmd,
                "@sRefRelationName1",
                model.sRefRelationName1);

            AddNullableInt(cmd,
                "@sRefRelationName2",
                model.sRefRelationName2);

            AddNullableString(cmd,
                "@sLocation1",
                model.sLocation1,
                300);

            AddNullableString(cmd,
                "@sLocation2",
                model.sLocation2,
                300);

            AddNullableString(cmd,
                "@sMobile1",
                model.sMobile1,
                20);

            AddNullableString(cmd,
                "@sMobile2",
                model.sMobile2,
                20);

            cmd.ExecuteNonQuery();
        }


        // =========================================================
        // UPDATE ACHIEVEMENTS
        // =========================================================

        public void UpdateAchievements(
            CandidateProfileModel model)
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            EnsureProfileExists(con, model.CandidateID);

            string sql = @"
UPDATE tblCandidateProfile
SET

    Achievements_Certification1 =
        @Achievements_Certification1,

    Achievements_Certification2 =
        @Achievements_Certification2,

    Achievements_Certification3 =
        @Achievements_Certification3,

    ModDate = GETDATE()

WHERE CandidateID = @CandidateID";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.Add("@CandidateID", SqlDbType.Int)
                .Value = model.CandidateID;

            AddNullableString(
                cmd,
                "@Achievements_Certification1",
                model.Achievements_Certification1,
                500
            );

            AddNullableString(
                cmd,
                "@Achievements_Certification2",
                model.Achievements_Certification2,
                500
            );

            AddNullableString(
                cmd,
                "@Achievements_Certification3",
                model.Achievements_Certification3,
                500
            );

            cmd.ExecuteNonQuery();
        }


        // =========================================================
        // UPDATE LINKS
        // =========================================================

        public void UpdateLinks(
            CandidateProfileModel model)
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            EnsureProfileExists(con, model.CandidateID);

            string sql = @"
UPDATE tblCandidateProfile
SET

    GitHub = @GitHub,
    Linkedin = @Linkedin,

    ModDate = GETDATE()

WHERE CandidateID = @CandidateID";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.Add("@CandidateID", SqlDbType.Int)
                .Value = model.CandidateID;

            AddNullableString(
                cmd,
                "@GitHub",
                model.GitHub,
                500
            );

            AddNullableString(
                cmd,
                "@Linkedin",
                model.Linkedin,
                500
            );

            cmd.ExecuteNonQuery();
        }


        // =========================================================
        // UPDATE OBJECTIVE
        // =========================================================

        public void UpdateObjective(
            CandidateProfileModel model)
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            EnsureProfileExists(con, model.CandidateID);

            string sql = @"
UPDATE tblCandidateProfile
SET

    Objective = @Objective,
    Resume_Profile = @Resume_Profile,

    ModDate = GETDATE()

WHERE CandidateID = @CandidateID";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.Add("@CandidateID", SqlDbType.Int)
                .Value = model.CandidateID;

            AddNullableString(
                cmd,
                "@Objective",
                model.Objective,
                400
            );

            AddNullableInt(
                cmd,
                "@Resume_Profile",
                model.Resume_Profile
            );

            cmd.ExecuteNonQuery();
        }


        // =========================================================
        // UPDATE SKILLS
        // =========================================================

        public void UpdateSkills(
            CandidateProfileModel model)
        {
            using SqlConnection con =
                _db.GetConnection();

            con.Open();

            EnsureProfileExists(con, model.CandidateID);

            string sql = @"
UPDATE tblCandidateProfile
SET

    sMedicalSkillIDs =
        @sMedicalSkillIDs,

    sMedicalSkillStarIDs =
        @sMedicalSkillStarIDs,

    sTechnicalSkillIDs =
        @sTechnicalSkillIDs,

    sTechnicalSkillStarIDs =
        @sTechnicalSkillStarIDs,

    sNonTechnicalSkillIDs =
        @sNonTechnicalSkillIDs,

    sNonTechnicalSkillStarIDs =
        @sNonTechnicalSkillStarIDs,

    ModDate = GETDATE()

WHERE CandidateID = @CandidateID";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.Add("@CandidateID", SqlDbType.Int)
                .Value = model.CandidateID;

            AddNullableString(
                cmd,
                "@sMedicalSkillIDs",
                model.sMedicalSkillIDs,
                500
            );

            AddNullableString(
                cmd,
                "@sMedicalSkillStarIDs",
                model.sMedicalSkillStarIDs,
                100
            );

            AddNullableString(
                cmd,
                "@sTechnicalSkillIDs",
                model.sTechnicalSkillIDs,
                500
            );

            AddNullableString(
                cmd,
                "@sTechnicalSkillStarIDs",
                model.sTechnicalSkillStarIDs,
                100
            );

            AddNullableString(
                cmd,
                "@sNonTechnicalSkillIDs",
                model.sNonTechnicalSkillIDs,
                500
            );

            AddNullableString(
                cmd,
                "@sNonTechnicalSkillStarIDs",
                model.sNonTechnicalSkillStarIDs,
                100
            );

            cmd.ExecuteNonQuery();
        }


        // =========================================================
        // UPDATE DOCUMENTS
        // =========================================================

        // =========================================================
        // UPDATE DOCUMENTS / HOBBIES
        // =========================================================

        public void UpdateDocuments(CandidateProfileModel model)
        {
            using SqlConnection con = _db.GetConnection();

            con.Open();

            EnsureProfileExists(con, model.CandidateID);

            string sql = @"
        UPDATE tblCandidateProfile
        SET
            sResume = @sResume,
            sPhoto = @sPhoto,
            sSignature = @sSignature,
            sDivyang = @sDivyang,
            sHobbies = @sHobbies,
            ModDate = GETDATE()
        WHERE CandidateID = @CandidateID";

            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@CandidateID", SqlDbType.Int)
                .Value = model.CandidateID;

            cmd.Parameters.Add("@sResume", SqlDbType.NVarChar, 300)
                .Value = (object?)model.sResume ?? DBNull.Value;

            cmd.Parameters.Add("@sPhoto", SqlDbType.NVarChar, 300)
                .Value = (object?)model.sPhoto ?? DBNull.Value;

            cmd.Parameters.Add("@sSignature", SqlDbType.NVarChar, 300)
                .Value = (object?)model.sSignature ?? DBNull.Value;

            cmd.Parameters.Add("@sDivyang", SqlDbType.NVarChar, 20)
                .Value = (object?)model.sDivyang ?? DBNull.Value;

            cmd.Parameters.Add("@sHobbies", SqlDbType.NVarChar, 300)
                .Value = (object?)model.sHobbies ?? DBNull.Value;

            cmd.ExecuteNonQuery();
        }


        // =========================================================
        // GET DIVISIONS
        // =========================================================

        public List<DropdownModel> GetDivisions()
        {
            return GetDropdownData(
                "SELECT nID AS ID, sName AS Name FROM tblDivision ORDER BY nID"
            );
        }


        // =========================================================
        // GET STREAMS
        // =========================================================

        public List<DropdownModel> GetStreams()
        {
            return GetDropdownData(
                "SELECT nID AS ID, sName AS Name FROM tblStream ORDER BY nID"
            );
        }


        // =========================================================
        // GET GRADUATION STATUS
        // =========================================================

        public List<DropdownModel> GetGraduationStatuses()
        {
            return GetDropdownData(
                "SELECT nID AS ID, sName AS Name FROM tblGraduationstatus ORDER BY nID"
            );
        }


        // =========================================================
        // GET INTERNSHIP FELLOWSHIP TYPE
        // =========================================================

        public List<DropdownModel> GetInternshipFellowshipType()
        {
            return GetDropdownData(
                "SELECT nID AS ID, sName AS Name FROM tblInternshipFellowshipType ORDER BY nID"
            );
        }


        // =========================================================
        // GET INTERNSHIP TITLES
        // =========================================================

        public List<DropdownModel> GetInternshipTitles()
        {
            return GetDropdownData(
                "SELECT nID AS ID, sName AS Name FROM tblInternshipTitle ORDER BY nID"
            );
        }


        // =========================================================
        // GET INTERNSHIP DURATIONS
        // =========================================================

        public List<DropdownModel> GetInternshipDurations()
        {
            return GetDropdownData(
                "SELECT nID AS ID, sName AS Name FROM tblInternshipsDoneDuration ORDER BY nID"
            );
        }


        // =========================================================
        // GET INTERNSHIP STATUS
        // =========================================================

        public List<DropdownModel> GetInternshipStatuses()
        {
            return GetDropdownData(
                "SELECT nID AS ID, sName AS Name FROM tblInternshipStatus ORDER BY nID"
            );
        }


        // =========================================================
        // GET RELATIONSHIPS
        // =========================================================

        public List<DropdownModel> GetRelationships()
        {
            return GetDropdownData(
                "SELECT nID AS ID, sName AS Name FROM tblRelationship ORDER BY nID"
            );
        }


        // =========================================================
        // COMMON DROPDOWN METHOD
        // =========================================================

        private List<DropdownModel> GetDropdownData(
            string sql)
        {
            List<DropdownModel> list = new();

            using SqlConnection con =
                _db.GetConnection();

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            con.Open();

            using SqlDataReader reader =
                cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new DropdownModel
                {
                    ID =
                        Convert.ToInt32(reader["ID"]),

                    Name =
                        reader["Name"]?.ToString() ?? ""
                });
            }

            return list;
        }


        // =========================================================
        // PARAMETER HELPERS
        // =========================================================

        private void AddNullableInt(
            SqlCommand cmd,
            string parameterName,
            int? value)
        {
            cmd.Parameters.Add(
                parameterName,
                SqlDbType.Int
            ).Value =
                (object?)value ?? DBNull.Value;
        }


        private void AddNullableString(
            SqlCommand cmd,
            string parameterName,
            string? value,
            int size)
        {
            cmd.Parameters.Add(
                parameterName,
                SqlDbType.NVarChar,
                size
            ).Value =
                (object?)value ?? DBNull.Value;
        }


        // =========================================================
        // DATA READER HELPERS
        // =========================================================

        //private int? GetNullableInt(
        //    SqlDataReader reader,
        //    string column)
        //{
        //    if (reader[column] == DBNull.Value)
        //        return null;

        //    return Convert.ToInt32(
        //        reader[column]
        //    );
        //}


       

        // =========================================================
        // UPDATE SOCIAL LINKS
        // =========================================================

        public void UpdateSocialLinks(CandidateProfileModel model)
        {
            using SqlConnection con = _db.GetConnection();

            con.Open();

            // Make sure profile exists
            EnsureProfileExists(con, model.CandidateID);

            string sql = @"
 UPDATE tblCandidateProfile
 SET
     GitHub = @GitHub,
     Linkedin = @Linkedin,
     ModDate = GETDATE()
 WHERE CandidateID = @CandidateID";

            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@CandidateID", SqlDbType.Int)
                .Value = model.CandidateID;

            AddNullableString(
                cmd,
                "@GitHub",
                model.GitHub,
                500
            );

            AddNullableString(
                cmd,
                "@Linkedin",
                model.Linkedin,
                500
            );

            cmd.ExecuteNonQuery();
        }

        // =========================================================
        // DATA READER HELPERS
        // =========================================================

        private string GetString(
            SqlDataReader reader,
            string column)
        {
            int index = reader.GetOrdinal(column);

            if (reader.IsDBNull(index))
            {
                return string.Empty;
            }

            return reader.GetValue(index)?.ToString() ?? string.Empty;
        }


        private int GetInt(
            SqlDataReader reader,
            string column)
        {
            int index = reader.GetOrdinal(column);

            if (reader.IsDBNull(index))
            {
                return 0;
            }

            return Convert.ToInt32(reader.GetValue(index));
        }


        private int? GetNullableInt(
            SqlDataReader reader,
            string column)
        {
            int index = reader.GetOrdinal(column);

            if (reader.IsDBNull(index))
            {
                return null;
            }

            object value = reader.GetValue(index);

            if (value == null)
            {
                return null;
            }

            if (int.TryParse(value.ToString(), out int result))
            {
                return result;
            }

            return null;
        }


        private string? GetNullableString(
            SqlDataReader reader,
            string column)
        {
            int index = reader.GetOrdinal(column);

            if (reader.IsDBNull(index))
            {
                return null;
            }

            return reader.GetValue(index)?.ToString();
        }
    }
}