using ErJobPortal.Models;
using ErJobPortal.Repositories;
using JobPortalTrainee.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ErJobPortal.Controllers
{
    public class ResumeController : Controller
    {
        private readonly CandidateProfileRepository _repository;

        public ResumeController(
            CandidateProfileRepository repository)
        {
            _repository = repository;
        }

        //radhika 21-09 Trainee

        [HttpGet]
        public IActionResult Resume(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid Candidate ID.");
            }

            // Get latest candidate profile data
            var profile = _repository.GetProfile(id);

            if (profile == null)
            {
                return NotFound(
                    $"Candidate profile not found for CandidateID: {id}"
                );
            }

            var model = new ResumeViewModel
            {
                CandidateID = id,

                Profile = profile,

                Relationships =
                    _repository.GetRelationships(),

                Streams =
                    _repository.GetStreams(),

                Divisions =
                    _repository.GetDivisions(),

                InternshipFellowshipType =
                    _repository.GetInternshipFellowshipType(),

                InternshipTitles =
                    _repository.GetInternshipTitles(),

                InternshipDurations =
                    _repository.GetInternshipDurations(),

                InternshipStatuses =
                    _repository.GetInternshipStatuses(),

                CandidateName = "Candidate",
                CandidateEmail = "",
                CandidatePhone = ""
            };

            return View(
                "~/Views/Resume/Resume.cshtml",
                model
            );
        }


        //radhika 21-09 Org

        [HttpGet]
        public IActionResult OrganizationResume(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid Organization ID.");
            }

            string connectionString =
                HttpContext.RequestServices
                    .GetRequiredService<IConfiguration>()
                    .GetConnectionString("DefaultConnection")!;

            OrgProfile organization = new OrgProfile
            {
                nOrgID = id
            };

            using (SqlConnection con =
                   new SqlConnection(connectionString))
            {
                using (SqlCommand cmd =
                       new SqlCommand(
                           "SP_GetOrganizationProfile",
                           con))
                {
                    cmd.CommandType =
                        CommandType.StoredProcedure;

                    cmd.Parameters.Add(
                        "@nOrgID",
                        SqlDbType.Int).Value = id;

                    con.Open();

                    using (SqlDataReader dr =
                           cmd.ExecuteReader())
                    {
                        if (!dr.Read())
                        {
                            return NotFound(
                                $"Organization profile not found for OrgID: {id}"
                            );
                        }

                        organization.nID =
                            dr["nID"] != DBNull.Value
                                ? Convert.ToInt32(dr["nID"])
                                : 0;

                        organization.nOrgID =
                            dr["nOrgID"] != DBNull.Value
                                ? Convert.ToInt32(dr["nOrgID"])
                                : id;

                        organization.sOrganizationName =
                            dr["sName"] != DBNull.Value
                                ? dr["sName"].ToString()!
                                : "";

                        organization.sOrganizationEmail =
                            dr["sEmail"] != DBNull.Value
                                ? dr["sEmail"].ToString()!
                                : "";

                        organization.sMobile =
                            dr["sMobile"] != DBNull.Value
                                ? dr["sMobile"].ToString()!
                                : "";

                        organization.sDesignation =
                            dr["sDesignation"] != DBNull.Value
                                ? dr["sDesignation"].ToString()!
                                : "";

                        organization.dDateOfBirth =
                            dr["dDateOfBirth"] != DBNull.Value
                                ? Convert.ToDateTime(
                                    dr["dDateOfBirth"])
                                : null;

                        organization.sCompanyLogo =
                            dr["sCompanyLogo"] != DBNull.Value
                                ? dr["sCompanyLogo"].ToString()!
                                : "";

                        organization.sCompanyAddress =
                            dr["sCompanyAddress"] != DBNull.Value
                                ? dr["sCompanyAddress"].ToString()!
                                : "";

                        organization.nEstablishmentYear =
                            dr["nEstablishmentYear"] != DBNull.Value
                                ? Convert.ToInt32(
                                    dr["nEstablishmentYear"])
                                : 0;

                        organization.sGSTNo =
                            dr["sGSTNo"] != DBNull.Value
                                ? dr["sGSTNo"].ToString()!
                                : "";

                        organization.sCINNo =
                            dr["sCINNo"] != DBNull.Value
                                ? dr["sCINNo"].ToString()!
                                : "";

                        organization.nEmployeeStrength =
                            dr["nEmployeeStrength"] != DBNull.Value
                                ? dr["nEmployeeStrength"].ToString()!
                                : "";

                        organization.dCreatedDate =
                            dr["dCreatedDate"] != DBNull.Value
                                ? Convert.ToDateTime(
                                    dr["dCreatedDate"])
                                : DateTime.MinValue;

                        organization.dModifiedDate =
                            dr["dModifiedDate"] != DBNull.Value
                                ? Convert.ToDateTime(
                                    dr["dModifiedDate"])
                                : null;

                        organization.nBit =
                            dr["nBit"] != DBNull.Value &&
                            Convert.ToBoolean(dr["nBit"]);

                        organization.nSABit =
                            dr["nSABit"] != DBNull.Value
                                ? Convert.ToBoolean(dr["nSABit"])
                                : null;
                    }
                }
            }

            return View(
                "~/Views/Resume/OrganizationResume.cshtml",
                organization
            );
        }
    }
}