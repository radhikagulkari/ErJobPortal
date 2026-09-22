using ErJobPortal.Models;
using ErJobPortal.Repositories;
using JobPortalTrainee.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace JobPortalTrainee.Controllers
{
    public class OrganizationController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AccountRepository _repository;
        private readonly IWebHostEnvironment _environment;

        public OrganizationController(
    IConfiguration configuration,
    AccountRepository repository)
        {
            _configuration = configuration;
            _repository = repository;
        }

        // =========================================================
        // GET LOGGED-IN ORGANIZATION CODE
        // =========================================================

        private string? GetOrganizationCode()
        {
            int? orgId = HttpContext.Session.GetInt32("OrgID");

            if (orgId == null || orgId <= 0)
                return null;

            var organization =
                _repository.GetOrganizationRegistrationDetails(orgId.Value);

            if (organization == null ||
                organization.Value.RegDate == null)
                return null;

            return "OR" +
                   organization.Value.RegDate.Value.ToString("ddMMyy") +
                   organization.Value.OrganizationID.ToString("D2");
        }

        // =========================================================
        // VALIDATE ORGANIZATION CODE
        // =========================================================

        private bool IsValidOrganizationCode(string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            string? organizationCode = GetOrganizationCode();

            return !string.IsNullOrWhiteSpace(organizationCode) &&
                   string.Equals(
                       id,
                       organizationCode,
                       StringComparison.OrdinalIgnoreCase);
        }

        // =========================================================
        // DASHBOARD
        // =========================================================

        [HttpGet]
        [Route("Organization/Dashboard/{id?}")]
        [ResponseCache(
            NoStore = true,
            Location = ResponseCacheLocation.None)]
        public IActionResult Dashboard(string? id)
        {
            // =====================================================
            // GET ORGANIZATION ID FROM SESSION
            // =====================================================

            int? orgId = HttpContext.Session.GetInt32("OrgID");

            if (orgId == null)
            {
                return RedirectToAction(
                    "OrganizationLogin",
                    "Account");
            }

            // =====================================================
            // GET ORGANIZATION REGISTRATION DETAILS
            // =====================================================

            var organization =
                _repository.GetOrganizationRegistrationDetails(
                    orgId.Value);

            if (organization == null)
            {
                return NotFound(
                    "Organization registration not found.");
            }

            // =====================================================
            // CHECK REGISTRATION DATE
            // =====================================================

            if (organization.Value.RegDate == null)
            {
                return BadRequest(
                    "Organization Registration Date is missing.");
            }

            // =====================================================
            // CREATE ORGANIZATION CODE
            //
            // Example:
            // RegDate = 26/07/2026
            // nID     = 1
            //
            // Result = OR26072601
            // =====================================================

            string organizationCode =
                "OR" +
                organization.Value.RegDate.Value
                    .ToString("ddMMyy") +
                organization.Value.OrganizationID
                    .ToString("D2");

            // =====================================================
            // IF ID IS NOT PRESENT
            // REDIRECT TO CODE URL
            // =====================================================

            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(
                    "Dashboard",
                    "Organization",
                    new
                    {
                        id = organizationCode
                    });
            }

            // =====================================================
            // CHECK ORGANIZATION CODE
            // =====================================================

            if (!string.Equals(
                    id,
                    organizationCode,
                    StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }

            // =====================================================
            // SESSION DATA
            // =====================================================

            ViewBag.OrgID =
                orgId.Value;

            ViewBag.OrgName =
                HttpContext.Session.GetString("OrgName");

            ViewBag.OrgEmail =
                HttpContext.Session.GetString("OrgEmail");

            ViewBag.OrgCode =
                organizationCode;

            // =====================================================
            // DASHBOARD COUNTS
            // =====================================================

            int traineeRegistrationCount = 0;
            int organizationRegistrationCount = 0;

            int internshipEligibleCount = 0;

            List<int> monthlyCandidateRegistrations =
                Enumerable.Repeat(0, 12).ToList();

            string connectionString =
                _configuration.GetConnectionString(
                    "DefaultConnection");

            using (SqlConnection con =
                   new SqlConnection(connectionString))
            {
                con.Open();

                // =================================================
                // TOTAL CANDIDATE & ORGANIZATION COUNT
                // =================================================

                string query = @"
            SELECT
                (SELECT COUNT(nID)
                 FROM tblCandidateRegister)
                    AS CandidateCount,

                (SELECT COUNT(nID)
                 FROM tblOrgRegistration)
                    AS OrganizationCount;";

                using (SqlCommand cmd =
                       new SqlCommand(query, con))
                using (SqlDataReader dr =
                       cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        traineeRegistrationCount =
                            Convert.ToInt32(
                                dr["CandidateCount"]);

                        organizationRegistrationCount =
                            Convert.ToInt32(
                                dr["OrganizationCount"]);
                    }
                }

                // =================================================
                // INTERNSHIP ELIGIBLE CANDIDATES
                // =================================================

                string eligibleQuery = @"
            SELECT COUNT(nID)
            FROM tblCandidateRegister
            WHERE ISNULL(nBit, 1) = 1;";

                using (SqlCommand cmd =
                       new SqlCommand(
                           eligibleQuery,
                           con))
                {
                    object result =
                        cmd.ExecuteScalar();

                    if (result != null &&
                        result != DBNull.Value)
                    {
                        internshipEligibleCount =
                            Convert.ToInt32(result);
                    }
                }

                // =================================================
                // MONTHLY CANDIDATE REGISTRATIONS
                // =================================================

                string monthlyQuery = @"
            SELECT
                MONTH(RegDate)
                    AS RegistrationMonth,

                COUNT(nID)
                    AS RegistrationCount

            FROM tblCandidateRegister

            WHERE YEAR(RegDate) =
                  YEAR(GETDATE())

            GROUP BY
                MONTH(RegDate)

            ORDER BY
                MONTH(RegDate);";

                using (SqlCommand cmd =
                       new SqlCommand(
                           monthlyQuery,
                           con))
                using (SqlDataReader dr =
                       cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        int month =
                            Convert.ToInt32(
                                dr["RegistrationMonth"]);

                        int count =
                            Convert.ToInt32(
                                dr["RegistrationCount"]);

                        if (month >= 1 &&
                            month <= 12)
                        {
                            monthlyCandidateRegistrations[
                                month - 1] = count;
                        }
                    }
                }
            }

            // =====================================================
            // GET TRAINEES
            // =====================================================

            List<SATraineeListM> trainees =
                _repository.GetAllTrainees();

            // =====================================================
            // GET ORGANIZATIONS
            // =====================================================

            List<OrganizationUser> organizations =
                _repository.GetAllOrganizationList();

            // =====================================================
            // SEND DATA TO VIEW
            // =====================================================

            ViewBag.TraineeRegistrationCount =
                traineeRegistrationCount;

            ViewBag.OrganizationRegistrationCount =
                organizationRegistrationCount;

            ViewBag.Trainees =
                trainees;

            ViewBag.Organizations =
                organizations;

            ViewBag.InternshipEligibleCount =
                internshipEligibleCount;

            ViewBag.MonthlyCandidateRegistrations =
                monthlyCandidateRegistrations;

            // =====================================================
            // RETURN VIEW
            // =====================================================

            return View();
        }


        public IActionResult CreatePost()
        {
            return View();
        }

        // ==========================================
        // CANDIDATE LIST
        // ==========================================
        [HttpGet]
        [Route("Organization/TraineeList/{id}")]
        public IActionResult TraineeList(string id)
        {
            if (!IsValidOrganizationCode(id))
                return NotFound();

            ViewBag.OrgCode = id;

            List<SATraineeListM> trainees =
                _repository.GetSATraineeList();

            return View(trainees);
        }

        // ==========================================
        // ORGANIZATION LIST
        // ==========================================
        // Organization List
        [HttpGet]
        [Route("Organization/OrgList/{id}")]
        public IActionResult OrgList(string id)
        {
            if (!IsValidOrganizationCode(id))
                return NotFound();

            ViewBag.OrgCode = id;

            List<OrganizationUser> organization =
                _repository.GetAllOrganizationList();

            return View(organization);
        }

        // =========================================================
        // EDIT PROFILE - GET
        // =========================================================

        [HttpGet]
        [Route("Organization/EditProfile/{id}")]
        public IActionResult EditProfile(string id)
        {
            int? orgId = HttpContext.Session.GetInt32("OrgID");

            if (orgId == null)
            {
                return RedirectToAction("OrganizationLogin", "Account");
            }

            // Validate Organization Code
            if (!IsValidOrganizationCode(id))
            {
                return NotFound();
            }

            OrgProfile model = new OrgProfile
            {
                nOrgID = orgId.Value
            };

            string connectionString = _configuration.GetConnectionString("DefaultConnection")!;

            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_GetOrganizationProfile", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@nOrgID", SqlDbType.Int).Value = orgId.Value;

                    cn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            model.nID = dr["nID"] != DBNull.Value ? Convert.ToInt32(dr["nID"]) : 0;

                            model.nOrgID = dr["nOrgID"] != DBNull.Value ? Convert.ToInt32(dr["nOrgID"]) : orgId.Value;

                            model.sOrganizationName =
                                dr["sName"] != DBNull.Value
                                    ? dr["sName"].ToString()!
                                    : string.Empty;

                            model.sOrganizationEmail =
                                dr["sEmail"] != DBNull.Value
                                    ? dr["sEmail"].ToString()!
                                    : string.Empty;

                            model.sMobile =
                                dr["sMobile"] != DBNull.Value
                                    ? dr["sMobile"].ToString()!
                                    : string.Empty;

                            model.sDesignation =
                                dr["sDesignation"] != DBNull.Value
                                    ? dr["sDesignation"].ToString()!
                                    : string.Empty;

                            model.dDateOfBirth =
                                dr["dDateOfBirth"] != DBNull.Value
                                    ? Convert.ToDateTime(dr["dDateOfBirth"])
                                    : null;

                            model.sCompanyLogo =
                                dr["sCompanyLogo"] != DBNull.Value
                                    ? dr["sCompanyLogo"].ToString()!
                                    : string.Empty;

                            model.sCompanyAddress =
                                dr["sCompanyAddress"] != DBNull.Value
                                    ? dr["sCompanyAddress"].ToString()!
                                    : string.Empty;

                            model.nEstablishmentYear =
                                dr["nEstablishmentYear"] != DBNull.Value
                                    ? Convert.ToInt32(dr["nEstablishmentYear"])
                                    : 0;

                            model.sGSTNo =
                                dr["sGSTNo"] != DBNull.Value
                                    ? dr["sGSTNo"].ToString()!
                                    : string.Empty;

                            model.sCINNo =
                                dr["sCINNo"] != DBNull.Value
                                    ? dr["sCINNo"].ToString()!
                                    : string.Empty;

                            model.nEmployeeStrength =
                                dr["nEmployeeStrength"] != DBNull.Value
                                    ? dr["nEmployeeStrength"].ToString()!
                                    : string.Empty;

                            model.dCreatedDate =
                                dr["dCreatedDate"] != DBNull.Value
                                    ? Convert.ToDateTime(dr["dCreatedDate"])
                                    : DateTime.MinValue;

                            model.dModifiedDate =
                                dr["dModifiedDate"] != DBNull.Value
                                    ? Convert.ToDateTime(dr["dModifiedDate"])
                                    : null;

                            model.nBit =
                                dr["nBit"] != DBNull.Value &&
                                Convert.ToBoolean(dr["nBit"]);

                            model.nSABit =
                                dr["nSABit"] != DBNull.Value
                                    ? Convert.ToBoolean(dr["nSABit"])
                                    : null;
                        }
                    }
                }
            }

            // Send ID to View
            ViewBag.OrgCode = id;
            ViewBag.OrgID = orgId.Value;
            ViewBag.OrgName =
                HttpContext.Session.GetString("OrgName");
            ViewBag.OrgEmail =
                HttpContext.Session.GetString("OrgEmail");

            return View(model);
        }


        [HttpGet]
        [Route("Organization/ViewProfile/{id}")]
        public IActionResult ViewOrgProfile(string id)
        {
            if (!IsValidOrganizationCode(id))
                return NotFound();

            ViewBag.OrgCode = id;

            return View();
        }

        public IActionResult ViewOrgPost()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(
OrgProfile model,
IFormFile? CompanyLogo)
        {
            // =====================================================
            // ORGANIZATION SESSION
            // =====================================================

            int? orgId =
                HttpContext.Session.GetInt32("OrgID");

            if (orgId == null || orgId <= 0)
            {
                return RedirectToAction(
                    "OrganizationLogin",
                    "Account"
                );
            }


            // =====================================================
            // SET ORGANIZATION ID
            // =====================================================

            model.nOrgID = orgId.Value;


            // =====================================================
            // MODEL VALIDATION
            // =====================================================

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // =====================================================
            // CONNECTION STRING
            // =====================================================

            string connectionString =
                _configuration.GetConnectionString(
                    "DefaultConnection"
                )!;


            // =====================================================
            // EXISTING LOGO
            // =====================================================

            string logoPath =
                model.sCompanyLogo ?? string.Empty;


            // =====================================================
            // COMPANY LOGO UPLOAD
            // =====================================================

            if (CompanyLogo != null &&
                CompanyLogo.Length > 0)
            {
                // =============================================
                // wwwroot/uploads/organization
                // =============================================

                string uploadsFolder =
                    Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads",
                        "organization"
                    );


                // =============================================
                // CREATE FOLDER IF NOT EXISTS
                // =============================================

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(
                        uploadsFolder
                    );
                }


                // =============================================
                // FILE EXTENSION
                // =============================================

                string extension =
                    Path.GetExtension(
                        CompanyLogo.FileName
                    ).ToLowerInvariant();


                // =============================================
                // ALLOWED FILE TYPES
                // =============================================

                string[] allowedExtensions =
                {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };


                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "CompanyLogo",
                        "Only JPG, JPEG, PNG and WEBP files are allowed."
                    );

                    return View(model);
                }


                // =============================================
                // FILE SIZE - MAX 5 MB
                // =============================================

                if (CompanyLogo.Length >
                    5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "CompanyLogo",
                        "Company logo size cannot exceed 5 MB."
                    );

                    return View(model);
                }


                // =============================================
                // UNIQUE FILE NAME
                // =============================================

                string fileName =
                    Guid.NewGuid().ToString("N")
                    + extension;


                // =============================================
                // COMPLETE FILE PATH
                // =============================================

                string filePath =
                    Path.Combine(
                        uploadsFolder,
                        fileName
                    );


                // =============================================
                // SAVE FILE
                // =============================================

                using (FileStream stream =
                       new FileStream(
                           filePath,
                           FileMode.Create))
                {
                    CompanyLogo.CopyTo(stream);
                }


                // =============================================
                // SAVE FILE NAME
                // =============================================

                logoPath = fileName;
            }


            // =====================================================
            // DATABASE
            // =====================================================

            using (SqlConnection cn =
                   new SqlConnection(connectionString))
            {
                using (SqlCommand cmd =
                       new SqlCommand(
                           "SP_AddOrganizationProfile",
                           cn))
                {
                    cmd.CommandType =
                        CommandType.StoredProcedure;


                    // =============================================
                    // ORGANIZATION ID
                    // =============================================

                    cmd.Parameters.Add(
                        "@nOrgID",
                        SqlDbType.Int
                    ).Value =
                        model.nOrgID;


                    // =============================================
                    // DATE OF BIRTH
                    // =============================================

                    cmd.Parameters.Add(
                        "@dDateOfBirth",
                        SqlDbType.DateTime
                    ).Value =
                        model.dDateOfBirth.HasValue
                            ? model.dDateOfBirth.Value
                            : DBNull.Value;


                    // =============================================
                    // COMPANY LOGO
                    // =============================================

                    cmd.Parameters.Add(
                        "@sCompanyLogo",
                        SqlDbType.NVarChar,
                        500
                    ).Value =
                        string.IsNullOrWhiteSpace(
                            logoPath)
                            ? DBNull.Value
                            : logoPath;


                    // =============================================
                    // COMPANY ADDRESS
                    // =============================================

                    cmd.Parameters.Add(
                        "@sCompanyAddress",
                        SqlDbType.NVarChar,
                        -1
                    ).Value =
                        string.IsNullOrWhiteSpace(
                            model.sCompanyAddress)
                            ? DBNull.Value
                            : model.sCompanyAddress;


                    // =============================================
                    // ESTABLISHMENT YEAR
                    // =============================================

                    cmd.Parameters.Add(
                        "@nEstablishmentYear",
                        SqlDbType.Int
                    ).Value =
                        model.nEstablishmentYear;


                    // =============================================
                    // GST NUMBER
                    // =============================================

                    cmd.Parameters.Add(
                        "@sGSTNo",
                        SqlDbType.NVarChar,
                        50
                    ).Value =
                        string.IsNullOrWhiteSpace(
                            model.sGSTNo)
                            ? DBNull.Value
                            : model.sGSTNo;


                    // =============================================
                    // CIN NUMBER
                    // =============================================

                    cmd.Parameters.Add(
                        "@sCINNo",
                        SqlDbType.NVarChar,
                        50
                    ).Value =
                        string.IsNullOrWhiteSpace(
                            model.sCINNo)
                            ? DBNull.Value
                            : model.sCINNo;


                    // =============================================
                    // EMPLOYEE STRENGTH
                    // =============================================

                    cmd.Parameters.Add(
                        "@nEmployeeStrength",
                        SqlDbType.NVarChar,
                        100
                    ).Value =
                        string.IsNullOrWhiteSpace(
                            model.nEmployeeStrength)
                            ? DBNull.Value
                            : model.nEmployeeStrength;


                    // =============================================
                    // EXECUTE
                    // =============================================

                    cn.Open();

                    cmd.ExecuteNonQuery();
                }
            }


            // =====================================================
            // SUCCESS
            // =====================================================

            TempData["SuccessMessage"] =
                "Organization profile updated successfully.";


            // =====================================================
            // GET ORGANIZATION CODE FROM SESSION
            // =====================================================

            string? orgCode =
                HttpContext.Session.GetString("OrgCode");


            if (!string.IsNullOrWhiteSpace(orgCode))
            {
                return RedirectToAction(
                    "EditProfile"
                //new { id = orgCode }
                );
            }


            return RedirectToAction(
                "EditProfile"
            //new { id = model.nOrgID }
            );
        }


        // =========================================================
        // LOGOUT
        // =========================================================

        [HttpGet]
        public IActionResult Logout()
        {
            // Clear all session data
            HttpContext.Session.Clear();

            // Delete session cookie
            Response.Cookies.Delete(".AspNetCore.Session");

            // Prevent browser from caching the previous page
            SetNoCacheHeaders();

            // Go to Home page
            return RedirectToAction(
                "Index",
                "Home");
        }


        // =========================================================
        // NO CACHE
        // =========================================================

        private void SetNoCacheHeaders()
        {
            Response.Headers["Cache-Control"] =
                "no-cache, no-store, must-revalidate";

            Response.Headers["Pragma"] =
                "no-cache";

            Response.Headers["Expires"] =
                "0";
        }



        // =========================================================
        // CANDIDATE CREATE FEEDBACK - GET
        // =========================================================
        [HttpGet]
        [Route("Organization/CreateFeedback/{id}")]
        public IActionResult CreateFeedback(string id)
        {
            // =====================================================
            // GET CANDIDATE ID
            // =====================================================

            int? OrgID =
                HttpContext.Session.GetInt32("OrgID");

            if (OrgID == null || OrgID <= 0)
            {
                return RedirectToAction("CreateFeedback", "Organization");
            }

            CandidateFeedbackViewModel feedback = null;

            string connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection con =
                   new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"
            SELECT
                nID,
                Que1,
                Que2,
                Que3,
                Que4,
                Que5,
                nBit,
                nSABit
            FROM tblSAOrgFeedback
            WHERE nID = 1
              AND ISNULL(nBit, 1) = 1";

                using (SqlCommand cmd =
                       new SqlCommand(query, con))
                {
                    using (SqlDataReader dr =
                           cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            feedback = new CandidateFeedbackViewModel
                            {
                                nID = Convert.ToInt32(dr["nID"]),

                                Que1 = dr["Que1"] != DBNull.Value
                                    ? dr["Que1"].ToString()
                                    : "",

                                Que2 = dr["Que2"] != DBNull.Value
                                    ? dr["Que2"].ToString()
                                    : "",

                                Que3 = dr["Que3"] != DBNull.Value
                                    ? dr["Que3"].ToString()
                                    : "",

                                Que4 = dr["Que4"] != DBNull.Value
                                    ? dr["Que4"].ToString()
                                    : "",

                                Que5 = dr["Que5"] != DBNull.Value
                                    ? dr["Que5"].ToString()
                                    : "",

                                sQue1 = "",
                                sQue2 = "",
                                sQue3 = "",
                                sQue4 = "",
                                sQue5 = "",

                                nOrgID = OrgID.Value,

                                nBit = dr["nBit"] != DBNull.Value
                                    ? Convert.ToBoolean(dr["nBit"])
                                    : true,

                                nSABit = dr["nSABit"] != DBNull.Value
                                    ? Convert.ToBoolean(dr["nSABit"])
                                    : true
                            };
                        }
                    }
                }
            }

            if (feedback == null)
            {
                return NotFound("Feedback questions not found.");
            }
            ViewBag.OrgCode = id;
            return View(feedback);
        }



        // =========================================================
        // CANDIDATE FEEDBACK - CREATE - POST
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateFeedback(
            CandidateFeedbackViewModel model)
        {
            // =====================================================
            // GET CANDIDATE ID FROM SESSION
            // =====================================================

            int? OrgID =
                HttpContext.Session.GetInt32("OrgID");

            if (OrgID == null || OrgID <= 0)
            {
                TempData["Error"] =
    "Organization session expired. Please login again.";

                return RedirectToAction("CreateFeedback", "Organization");
            }

            // =====================================================
            // VALIDATION
            // =====================================================

            if (string.IsNullOrEmpty(model.sQue1))
                ModelState.AddModelError(
                    "sQue1",
                    "Please select an answer.");

            if (string.IsNullOrEmpty(model.sQue2))
                ModelState.AddModelError(
                    "sQue2",
                    "Please select a rating.");

            if (string.IsNullOrEmpty(model.sQue3))
                ModelState.AddModelError(
                    "sQue3",
                    "Please select an answer.");

            if (string.IsNullOrEmpty(model.sQue4))
                ModelState.AddModelError(
                    "sQue4",
                    "Please select an emoji.");

            if (!ModelState.IsValid)
            {
                // IMPORTANT:
                // model is now CandidateFeedbackViewModel,
                // same type required by the View.
                return View(model);
            }

            // =====================================================
            // QUESTION 1
            // =====================================================

            int sQue1;

            if (model.sQue1 == "True")
            {
                sQue1 = 1;
            }
            else if (model.sQue1 == "False")
            {
                sQue1 = 0;
            }
            else
            {
                ModelState.AddModelError(
                    "sQue1",
                    "Invalid answer.");

                return View(model);
            }

            // =====================================================
            // QUESTION 2
            // =====================================================

            if (!int.TryParse(
                model.sQue2,
                out int sQue2))
            {
                ModelState.AddModelError(
                    "sQue2",
                    "Invalid rating.");

                return View(model);
            }

            // =====================================================
            // QUESTION 3
            // =====================================================

            int sQue3;

            if (model.sQue3 == "Yes")
            {
                sQue3 = 1;
            }
            else if (model.sQue3 == "No")
            {
                sQue3 = 0;
            }
            else
            {
                ModelState.AddModelError(
                    "sQue3",
                    "Invalid answer.");

                return View(model);
            }

            // =====================================================
            // QUESTION 4
            // =====================================================

            if (!int.TryParse(
                model.sQue4,
                out int sQue4))
            {
                ModelState.AddModelError(
                    "sQue4",
                    "Invalid emoji rating.");

                return View(model);
            }

            // =====================================================
            // QUESTION 5
            // =====================================================

            string sQue5 =
                model.sQue5 ?? "";

            // =====================================================
            // DATABASE
            // =====================================================

            string connectionString =
                _configuration.GetConnectionString(
                    "DefaultConnection");

            using (SqlConnection con =
                   new SqlConnection(connectionString))
            {
                string query = @"
            INSERT INTO tblOrgFeedback
            (
                sQue1,
                sQue2,
                sQue3,
                sQue4,
                sQue5,
                nAdminID,
                RegDate,
                ModDate,
                nBit,
                nSABit
            )
            VALUES
            (
                @sQue1,
                @sQue2,
                @sQue3,
                @sQue4,
                @sQue5,
                @nAdminID,
                GETDATE(),
                GETDATE(),
                1,
                0
            )";

                using (SqlCommand cmd =
                       new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@sQue1",
                        SqlDbType.Int).Value = sQue1;

                    cmd.Parameters.Add(
                        "@sQue2",
                        SqlDbType.Int).Value = sQue2;

                    cmd.Parameters.Add(
                        "@sQue3",
                        SqlDbType.Int).Value = sQue3;

                    cmd.Parameters.Add(
                        "@sQue4",
                        SqlDbType.Int).Value = sQue4;

                    cmd.Parameters.Add(
                        "@sQue5",
                        SqlDbType.NVarChar,
                        200).Value =
                            string.IsNullOrWhiteSpace(sQue5)
                            ? DBNull.Value
                            : sQue5;

                    // Candidate ID
                    cmd.Parameters.Add(
                        "@nAdminID",
                        SqlDbType.Int).Value =
                            OrgID.Value;

                    con.Open();

                    int rows =
                        cmd.ExecuteNonQuery();

                    if (rows <= 0)
                    {
                        TempData["Error"] =
                            "Feedback was not saved.";

                        return View(model);
                    }
                }
            }

            TempData["Success"] = "Feedback submitted successfully.";

            return RedirectToAction(
                "CreateFeedback",
                "Organization");
        }

        // Create Post
        [HttpGet]
        [Route("Organization/CreatePost/{id}")]
        public IActionResult CreatePostNew(string id)
        {
            if (!IsValidOrganizationCode(id))
                return NotFound();

            ViewBag.OrgCode = id;

            return View();
        }

        [HttpPost]
        public IActionResult CreatePostNew(OrgPostM model)
        {
            try
            {
                string? connectionString =
                    _configuration.GetConnectionString(
                        "DefaultConnection"
                    );

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Database connection string not found."
                    });
                }

                using SqlConnection con =
                    new SqlConnection(connectionString);

                using SqlCommand cmd =
                    new SqlCommand(
                        "SP_CreateOrgPost",
                        con
                    );

                cmd.CommandType =
                    CommandType.StoredProcedure;

                // =====================================================
                // ROLE DETAILS
                // =====================================================

                cmd.Parameters.AddWithValue(
                    "@nPositionID",
                    model.nPositionID);

                cmd.Parameters.AddWithValue(
                    "@nRequiredTrainees",
                    model.nRequiredTrainees);

                cmd.Parameters.AddWithValue(
                    "@nGenderID",
                    model.nGenderID);

                cmd.Parameters.AddWithValue(
                    "@nMinimumQualificationID",
                    model.nMinimumQualificationID);

                // =====================================================
                // LOCATION
                // =====================================================

                cmd.Parameters.AddWithValue(
                    "@sCountryCode",
                    model.sCountryCode ?? (object)DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@sStateCode",
                    model.sStateCode ?? (object)DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@nCityID",
                    model.nCityID);

                cmd.Parameters.AddWithValue(
                    "@sWorkingHours",
                    model.sWorkingHours ?? (object)DBNull.Value);

                // =====================================================
                // WORK TERMS
                // =====================================================

                cmd.Parameters.AddWithValue(
                    "@nInternshipTypeID",
                    model.nInternshipTypeID);

                cmd.Parameters.AddWithValue(
                    "@sWorkingShift",
                    model.sWorkingShift ?? (object)DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@nInternshipFellowshipTypeID",
                    model.nInternshipFellowshipTypeID);

                cmd.Parameters.AddWithValue(
                    "@sTotalCharges",
                    model.sTotalCharges.HasValue
                        ? model.sTotalCharges.Value
                        : DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@sCurrency",
                    model.sCurrency ?? (object)DBNull.Value);

                // =====================================================
                // DURATION
                // =====================================================

                cmd.Parameters.AddWithValue(
                    "@nTrainingInvolvedID",
                    model.nTrainingInvolvedID);

                cmd.Parameters.AddWithValue(
                    "@nInternshipDurationID",
                    model.nInternshipDurationID);

                cmd.Parameters.AddWithValue(
                    "@dStartDate",
                    model.dStartDate);

                cmd.Parameters.AddWithValue(
                    "@dCompletionDate",
                    model.dCompletionDate);

                // =====================================================
                // MODE
                // =====================================================

                cmd.Parameters.AddWithValue(
                    "@nInternshipModeID",
                    model.nInternshipModeID);

                cmd.Parameters.AddWithValue(
                    "@sDivyang",
                    model.sDivyang ?? (object)DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@sLanguageKnown",
                    model.sLanguageKnown ?? (object)DBNull.Value);

                // =====================================================
                // WORKING DAYS / FACILITIES
                // =====================================================

                cmd.Parameters.AddWithValue(
                    "@sWorkingDays",
                    string.IsNullOrWhiteSpace(
                        model.sWorkingDays)
                        ? DBNull.Value
                        : model.sWorkingDays);

                cmd.Parameters.AddWithValue(
                    "@sFacilities",
                    string.IsNullOrWhiteSpace(
                        model.sFacilities)
                        ? DBNull.Value
                        : model.sFacilities);

                // =====================================================
                // SKILLS
                // =====================================================

                cmd.Parameters.AddWithValue(
                    "@sMedicalSkills",
                    string.IsNullOrWhiteSpace(
                        model.sMedicalSkills)
                        ? DBNull.Value
                        : model.sMedicalSkills);

                cmd.Parameters.AddWithValue(
                    "@sTechnicalSkills",
                    string.IsNullOrWhiteSpace(
                        model.sTechnicalSkills)
                        ? DBNull.Value
                        : model.sTechnicalSkills);

                cmd.Parameters.AddWithValue(
                    "@sNonTechnicalSkills",
                    string.IsNullOrWhiteSpace(
                        model.sNonTechnicalSkills)
                        ? DBNull.Value
                        : model.sNonTechnicalSkills);

                // =====================================================
                // ORGANIZATION
                // =====================================================

                // TEMPORARY
                // Replace this with logged-in organization ID.
                cmd.Parameters.AddWithValue(
                    "@nOrgID",
                    1);

                // =====================================================
                // EXECUTE
                // =====================================================

                con.Open();

                object? result =
                    cmd.ExecuteScalar();

                int newPostID =
                    result != null &&
                    result != DBNull.Value
                        ? Convert.ToInt32(result)
                        : 0;

                return Json(new
                {
                    success = true,
                    message =
                        "Create Post added successfully.",
                    nID = newPostID
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        // =========================================================
        // COMMON MASTER DATA METHOD
        // =========================================================

        private List<OrgPostM> GetMasterData(string procedureName)
        {
            var list = new List<OrgPostM>();

            string? connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(procedureName, con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new OrgPostM
                        {
                            nID = Convert.ToInt32(dr["nID"]),
                            sName = dr["sName"]?.ToString() ?? ""
                        });
                    }
                }
            }

            return list;
        }

        // =========================================================
        // POSITION
        // =========================================================

        [HttpGet]
        public IActionResult GetPosition()
        {
            return Json(GetMasterData("SP_GetPosition"));
        }

        // =========================================================
        // GENDER
        // =========================================================

        [HttpGet]
        public IActionResult GetGender()
        {
            return Json(GetMasterData("SP_GetGender"));
        }

        // =========================================================
        // MINIMUM QUALIFICATION
        // =========================================================

        [HttpGet]
        public IActionResult GetMinimumQualification()
        {
            return Json(GetMasterData("SP_GetMinimumQualification"));
        }

        // =========================================================
        // INTERNSHIP TYPE
        // =========================================================

        [HttpGet]
        public IActionResult GetInternshipType()
        {
            return Json(GetMasterData("SP_GetInternshipType"));
        }

        // =========================================================
        // INTERNSHIP / FELLOWSHIP TYPE
        // =========================================================

        [HttpGet]
        public IActionResult GetInternshipFellowshipType()
        {
            return Json(GetMasterData("SP_GetInternshipFellowshipType"));
        }

        // =========================================================
        // TRAINING INVOLVED
        // =========================================================

        [HttpGet]
        public IActionResult GetTrainingInvolved()
        {
            return Json(GetMasterData("SP_GetTrainingInvolved"));
        }

        // =========================================================
        // INTERNSHIP DURATION
        // =========================================================

        [HttpGet]
        public IActionResult GetInternshipDuration()
        {
            return Json(GetMasterData("SP_GetInternshipDuration"));
        }

        // =========================================================
        // INTERNSHIP MODE
        // =========================================================

        [HttpGet]
        public IActionResult GetInternshipMode()
        {
            return Json(GetMasterData("SP_GetInternshipMode"));
        }
        // =========================================================
        // GET COUNTRIES
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetCountries()
        {
            try
            {
                string url =
                    "https://api.geocoded.me/v2/countries" +
                    "?fields=id,iso2,name" +
                    "&limit=300";

                using HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(
                        "application/json"));

                HttpResponseMessage response =
                    await client.GetAsync(url);

                string json =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(
                        (int)response.StatusCode,
                        json);
                }

                var result =
                    JsonSerializer.Deserialize<CountryApiResponse>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (result?.Data == null)
                {
                    return Json(new List<Country>());
                }

                var countries =
                    result.Data
                        .OrderBy(x => x.Name)
                        .ToList();

                return Json(countries);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    "Country API Error: " + ex.Message);
            }
        }


        // =========================================================
        // GET STATES BY COUNTRY
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetStates(
            string countryCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(countryCode))
                {
                    return BadRequest(
                        "Country code is required.");
                }

                countryCode =
                    countryCode.Trim().ToUpperInvariant();

                string url =
                    "https://api.geocoded.me/v2/states" +
                    "?filter[country]=" +
                    Uri.EscapeDataString(countryCode) +
                    "&fields=id,name,countryCode,stateCode" +
                    "&limit=5000";

                using HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Accept.Clear();

                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(
                        "application/json"));

                HttpResponseMessage response =
                    await client.GetAsync(url);

                string json =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(
                        (int)response.StatusCode,
                        json);
                }

                var result =
                    JsonSerializer.Deserialize<StateApiResponse>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (result?.Data == null)
                {
                    return Json(new List<State>());
                }

                var states =
                    result.Data
                        .OrderBy(x => x.Name)
                        .ToList();

                return Json(states);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    "State API Error: " + ex.Message);
            }
        }


        // =========================================================
        // GET CITIES BY COUNTRY + STATE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetCities(
            string countryCode,
            string stateCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(countryCode))
                {
                    return BadRequest(
                        "Country code is required.");
                }

                if (string.IsNullOrWhiteSpace(stateCode))
                {
                    return BadRequest(
                        "State code is required.");
                }

                countryCode =
                    countryCode.Trim().ToUpperInvariant();

                stateCode =
                    stateCode.Trim().ToUpperInvariant();

                string url =
                    "https://api.geocoded.me/v2/cities" +
                    "?filter[country]=" +
                    Uri.EscapeDataString(countryCode) +
                    "&filter[state]=" +
                    Uri.EscapeDataString(stateCode) +
                    "&fields=id,name,countryCode,stateCode" +
                    "&limit=1000";

                Console.WriteLine(
                    "CITY URL: " + url);

                using HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Accept.Clear();

                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(
                        "application/json"));

                HttpResponseMessage response =
                    await client.GetAsync(url);

                string json =
                    await response.Content.ReadAsStringAsync();

                Console.WriteLine(
                    "CITY STATUS: " + response.StatusCode);

                Console.WriteLine(
                    "CITY RESPONSE: " + json);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(
                        (int)response.StatusCode,
                        json);
                }

                var result =
                    JsonSerializer.Deserialize<CityApiResponse>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (result?.Data == null)
                {
                    return Json(new List<City>());
                }

                var cities =
                    result.Data
                        .OrderBy(x => x.Name)
                        .ToList();

                return Json(cities);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    "City API Error: " + ex.Message);
            }
        }


        // =========================================================
        // API RESPONSE MODELS
        // =========================================================

        public class CountryApiResponse
        {
            public List<Country> Data { get; set; }
                = new List<Country>();
        }


        public class StateApiResponse
        {
            public List<State> Data { get; set; }
                = new List<State>();
        }


        public class CityApiResponse
        {
            public List<City> Data { get; set; }
                = new List<City>();
        }


        public class Country
        {
            public string Id { get; set; } = string.Empty;

            public string Iso2 { get; set; } = string.Empty;

            public string Name { get; set; } = string.Empty;
        }


        public class State
        {
            public string Id { get; set; } = string.Empty;

            public string Name { get; set; } = string.Empty;

            public string CountryCode { get; set; } = string.Empty;

            public string StateCode { get; set; } = string.Empty;
        }


        public class City
        {
            public string Id { get; set; } = string.Empty;

            public string Name { get; set; } = string.Empty;

            public string CountryCode { get; set; } = string.Empty;

            public string StateCode { get; set; } = string.Empty;
        }


        [HttpGet]
        public IActionResult GetTechnicalSkills()
        {
            var list = new List<OrgPostM>();

            string? connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SP_GetTechnicalSkills", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new OrgPostM
                        {
                            nID = Convert.ToInt32(dr["nID"]),

                            nTechnicalSkillID =
                                Convert.ToInt32(dr["nTechnicalSkillID"]),

                            sTechnicalSkillName =
                                dr["sTechnicalSkillName"]?.ToString() ?? "",

                            sTechnicalSkillRating =
                                Convert.ToInt32(dr["sTechnicalSkillRating"]),

                            IsChecked =
                                dr["IsChecked"] != DBNull.Value &&
                                Convert.ToBoolean(dr["IsChecked"])
                        });
                    }
                }
            }

            return Json(list);
        }


        [HttpGet]
        public IActionResult GetMedicalSkills()
        {
            var list = new List<OrgPostM>();

            string? connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SP_GetMedicalSkills", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new OrgPostM
                        {
                            nID = Convert.ToInt32(dr["nID"]),
                            nMedicalSkillID =
                                Convert.ToInt32(dr["nMedicalSkillID"]),

                            sMedicalSkillName =
                                dr["sMedicalSkillName"]?.ToString() ?? "",

                            sMedicalSkillRating =
                                Convert.ToInt32(dr["sMedicalSkillRating"]),

                            IsChecked =
                                Convert.ToBoolean(dr["IsChecked"])
                        });
                    }
                }
            }

            return Json(list);
        }


        // =========================================================
        // NON-TECHNICAL SKILLS
        // =========================================================

        [HttpGet]
        public IActionResult GetNonTechnicalSkills()
        {
            var list = new List<OrgPostM>();

            string? connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd =
                new SqlCommand("SP_GetNonTechnicalSkills", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        list.Add(new OrgPostM
                        {
                            nID = Convert.ToInt32(dr["nID"]),

                            nNonTechnicalSkillID =
                                Convert.ToInt32(
                                    dr["nNonTechnicalSkillID"]),

                            sNonTechnicalSkillName =
                                dr["sNonTechnicalSkillName"]?.ToString() ?? "",

                            sNonTechnicalSkillRating =
                                Convert.ToInt32(
                                    dr["sNonTechnicalSkillRating"]),

                            IsChecked =
                                Convert.ToBoolean(dr["IsChecked"])
                        });
                    }
                }
            }

            return Json(list);
        }



        [HttpPost]
        public IActionResult AddPost(OrgPostM model)
        {
            try
            {
                string? connectionString =
                    _configuration.GetConnectionString(
                        "DefaultConnection"
                    );

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return Json(new
                    {
                        success = false,
                        message =
                            "Database connection string not found."
                    });
                }

                using SqlConnection con =
                    new SqlConnection(connectionString);

                using SqlCommand cmd =
                    new SqlCommand(
                        "SP_AddPost",
                        con
                    );

                cmd.CommandType =
                    CommandType.StoredProcedure;

                // =====================================================
                // ROLE DETAILS
                // =====================================================

                cmd.Parameters.AddWithValue(
                    "@nPositionID",
                    model.nPositionID);

                cmd.Parameters.AddWithValue(
                    "@nRequiredTrainees",
                    model.nRequiredTrainees);

                cmd.Parameters.AddWithValue(
                    "@nGenderID",
                    model.nGenderID);

                cmd.Parameters.AddWithValue(
                    "@nMinimumQualificationID",
                    model.nMinimumQualificationID);

                // =====================================================
                // LOCATION
                // =====================================================

                cmd.Parameters.AddWithValue(
                    "@sCountryCode",
                    model.sCountryCode ?? (object)DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@sStateCode",
                    model.sStateCode ?? (object)DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@nCityID",
                    model.nCityID);

                cmd.Parameters.AddWithValue(
                    "@sWorkingHours",
                    model.sWorkingHours ?? (object)DBNull.Value);

                // =====================================================
                // WORK TERMS
                // =====================================================

                cmd.Parameters.AddWithValue(
                    "@nInternshipTypeID",
                    model.nInternshipTypeID);

                cmd.Parameters.AddWithValue(
                    "@sWorkingShift",
                    model.sWorkingShift ?? (object)DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@nInternshipFellowshipTypeID",
                    model.nInternshipFellowshipTypeID);

                cmd.Parameters.AddWithValue(
                    "@sTotalCharges",
                    model.sTotalCharges.HasValue
                        ? model.sTotalCharges.Value
                        : DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@sCurrency",
                    model.sCurrency ?? (object)DBNull.Value);

                // =====================================================
                // DURATION
                // =====================================================

                cmd.Parameters.AddWithValue(
                    "@nTrainingInvolvedID",
                    model.nTrainingInvolvedID);

                cmd.Parameters.AddWithValue(
                    "@nInternshipDurationID",
                    model.nInternshipDurationID);

                cmd.Parameters.AddWithValue(
                    "@dStartDate",
                    model.dStartDate);

                cmd.Parameters.AddWithValue(
                    "@dCompletionDate",
                    model.dCompletionDate);

                // =====================================================
                // MODE
                // =====================================================

                cmd.Parameters.AddWithValue(
                    "@nInternshipModeID",
                    model.nInternshipModeID);

                cmd.Parameters.AddWithValue(
                    "@sDivyang",
                    model.sDivyang ?? (object)DBNull.Value);

                cmd.Parameters.AddWithValue(
                    "@sLanguageKnown",
                    model.sLanguageKnown ?? (object)DBNull.Value);

                // =====================================================
                // WORKING DAYS / FACILITIES
                // =====================================================

                cmd.Parameters.AddWithValue(
                    "@sWorkingDays",
                    string.IsNullOrWhiteSpace(
                        model.sWorkingDays)
                        ? DBNull.Value
                        : model.sWorkingDays);

                cmd.Parameters.AddWithValue(
                    "@sFacilities",
                    string.IsNullOrWhiteSpace(
                        model.sFacilities)
                        ? DBNull.Value
                        : model.sFacilities);

                // =====================================================
                // SKILLS
                // =====================================================

                cmd.Parameters.AddWithValue(
                    "@sMedicalSkills",
                    string.IsNullOrWhiteSpace(
                        model.sMedicalSkills)
                        ? DBNull.Value
                        : model.sMedicalSkills);

                cmd.Parameters.AddWithValue(
                    "@sTechnicalSkills",
                    string.IsNullOrWhiteSpace(
                        model.sTechnicalSkills)
                        ? DBNull.Value
                        : model.sTechnicalSkills);

                cmd.Parameters.AddWithValue(
                    "@sNonTechnicalSkills",
                    string.IsNullOrWhiteSpace(
                        model.sNonTechnicalSkills)
                        ? DBNull.Value
                        : model.sNonTechnicalSkills);

                // =====================================================
                // ORGANIZATION
                // =====================================================

                // TEMPORARY
                // Replace this with logged-in organization ID.
                cmd.Parameters.AddWithValue(
                    "@nOrgID",
                    1);

                // =====================================================
                // EXECUTE
                // =====================================================

                con.Open();

                object? result =
                    cmd.ExecuteScalar();

                int newPostID =
                    result != null &&
                    result != DBNull.Value
                        ? Convert.ToInt32(result)
                        : 0;

                return Json(new
                {
                    success = true,
                    message =
                        "Post added successfully.",
                    nID = newPostID
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("Organization/PostDetails/{id}")]
        public IActionResult PostDetails(string id)
        {
            int? sessionOrgID = HttpContext.Session.GetInt32("OrgID");

            if (sessionOrgID == null)
            {
                return RedirectToAction("OrganizationLogin", "Account");
            }

            int orgID = sessionOrgID.Value;


            List<OrgPostM> posts = new List<OrgPostM>();

            using (SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand(
                    "SP_GetOrganizationPostList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@nOrgID", orgID);

                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            OrgPostM item = new OrgPostM();

                            item.nID = Convert.ToInt32(dr["nID"]);

                            //item.sName =
                            //    dr["sName"]?.ToString() ?? "";

                            item.nPositionID =
     Convert.ToInt32(dr["nPositionID"]);
                            item.sPositionName =
                                dr["sPositionName"]?.ToString() ?? "";

                            item.nRequiredTrainees =
                                Convert.ToInt32(dr["nRequiredTrainees"]);

                            item.nGenderID =
                                Convert.ToInt32(dr["nGenderID"]);
                            item.sGenderName =
                                dr["sGenderName"]?.ToString() ?? "";

                            item.nMinimumQualificationID =
                                Convert.ToInt32(dr["nMinimumQualificationID"]);
                            item.sMinimumQualificationName =
                                dr["sMinimumQualificationName"]?.ToString() ?? "";

                            item.sCountryCode =
                                dr["sCountryCode"]?.ToString() ?? "";

                            item.sStateCode =
                                dr["sStateCode"]?.ToString() ?? "";

                            item.nCityID =
                                Convert.ToInt32(dr["nCityID"]);
                            //item.sCityName =
                            //    dr["sCityName"]?.ToString() ?? "";

                            item.sWorkingHours =
                                dr["sWorkingHours"]?.ToString() ?? "";

                            item.nInternshipTypeID =
                                Convert.ToInt32(dr["nInternshipTypeID"]);
                            item.sInternshipTypeName =
                                dr["sInternshipTypeName"]?.ToString() ?? "";

                            item.sWorkingShift =
                                dr["sWorkingShift"]?.ToString() ?? "";

                            item.nInternshipFellowshipTypeID =
                                Convert.ToInt32(dr["nInternshipFellowshipTypeID"]);
                            item.sInternshipFellowshipTypeName =
                                dr["sInternshipFellowshipTypeName"]?.ToString() ?? "";

                            item.nTrainingInvolvedID =
                                Convert.ToInt32(dr["nTrainingInvolvedID"]);
                            item.sTrainingInvolvedName =
                                dr["sTrainingInvolvedName"]?.ToString() ?? "";

                            item.nInternshipDurationID =
                                Convert.ToInt32(dr["nInternshipDurationID"]);
                            item.sInternshipDurationName =
                                dr["sInternshipDurationName"]?.ToString() ?? "";

                            item.nInternshipModeID =
                                Convert.ToInt32(dr["nInternshipModeID"]);
                            item.sInternshipModeName =
                                dr["sInternshipModeName"]?.ToString() ?? "";

                            item.sDivyang =
                                dr["sDivyang"]?.ToString() ?? "";

                            item.sWorkingDays =
                                dr["sWorkingDays"]?.ToString() ?? "";

                            item.sFacilities =
                                dr["sFacilities"]?.ToString() ?? "";

                            item.dRegisterDate =
                                dr["dRegisterDate"] == DBNull.Value
                                ? null
                                : Convert.ToDateTime(
                                    dr["dRegisterDate"]);

                            item.dModDate =
                                dr["dModDate"] == DBNull.Value
                                ? null
                                : Convert.ToDateTime(
                                    dr["dModDate"]);

                            item.nBit =
                                dr["nBit"] != DBNull.Value &&
                                Convert.ToBoolean(dr["nBit"]);

                            item.nSABit =
                                dr["nSABit"] != DBNull.Value &&
                                Convert.ToBoolean(dr["nSABit"]);

                            item.nOrgID =
                                Convert.ToInt32(dr["nOrgID"]);

                            posts.Add(item);
                        }
                    }
                }
            }
            ViewBag.OrgCode = id;
            return View(posts);
        }

        [HttpPost]
        [Route("Organization/CreatePost/{id}")]
        public IActionResult DisablePost(int id)
        {
            int? orgID = HttpContext.Session.GetInt32("OrgID");


            if (orgID == null)
            {
                return RedirectToAction("OrganizationLogin", "Account");
            }

            using (SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand(
                    @"UPDATE tblPost
          SET nBit = 0,
              dModDate = GETDATE()
          WHERE nID = @nID
            AND nOrgID = @nOrgID", con))
                {
                    cmd.Parameters.AddWithValue("@nID", id);
                    cmd.Parameters.AddWithValue("@nOrgID", orgID.Value);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            ViewBag.OrgCode = id;
            return RedirectToAction("PostDetails");
        }


        [HttpPost]
        public IActionResult EnablePost(int id)
        {
            int? orgID = HttpContext.Session.GetInt32("OrgID");

            if (orgID == null)
            {
                return RedirectToAction("OrganizationLogin", "Account");
            }

            using (SqlConnection con = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")))
            {
                using (SqlCommand cmd = new SqlCommand(
                    @"UPDATE tblPost
          SET nBit = 1,
              dModDate = GETDATE()
          WHERE nID = @nID
            AND nOrgID = @nOrgID", con))
                {
                    cmd.Parameters.AddWithValue("@nID", id);
                    cmd.Parameters.AddWithValue("@nOrgID", orgID.Value);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("PostDetails");
        }

        // =========================================================
        // EDIT POST - GET
        // =========================================================

        [HttpGet]
        public IActionResult EditPost(int id)
        {
            int? sessionOrgID = HttpContext.Session.GetInt32("OrgID");

            if (sessionOrgID == null)
            {
                return RedirectToAction("OrganizationLogin", "Account");
            }

            int orgID = sessionOrgID.Value;

            OrgPostM model = new OrgPostM();

            string connectionString =
                _configuration.GetConnectionString("DefaultConnection")!;

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(@"
    SELECT
        nID,
        nOrgID,
        nPositionID,
        nRequiredTrainees,
        nGenderID,
        nMinimumQualificationID,
        sCountryCode,
        sStateCode,
        nCityID,
        sWorkingHours,
        nInternshipTypeID,
        sWorkingShift,
        nInternshipFellowshipTypeID,
        sTotalCharges,
        sCurrency,
        nTrainingInvolvedID,
        nInternshipDurationID,
        dStartDate,
        dCompletionDate,
        nInternshipModeID,
        sDivyang,
        sLanguageKnown,
        sWorkingDays,
        sFacilities,
        sMedicalSkills,
        sTechnicalSkills,
        sNonTechnicalSkills
    FROM tblPost
    WHERE nID = @nID
      AND nOrgID = @nOrgID", con))
            {
                cmd.Parameters.Add("@nID", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@nOrgID", SqlDbType.Int).Value = orgID;

                con.Open();

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.Read())
                    {
                        return NotFound("Post not found.");
                    }

                    model.nID =
                        Convert.ToInt32(dr["nID"]);

                    model.nOrgID =
                        Convert.ToInt32(dr["nOrgID"]);

                    model.nPositionID =
                        Convert.ToInt32(dr["nPositionID"]);

                    model.nRequiredTrainees =
                        Convert.ToInt32(dr["nRequiredTrainees"]);

                    model.nGenderID =
                        Convert.ToInt32(dr["nGenderID"]);

                    model.nMinimumQualificationID =
                        Convert.ToInt32(dr["nMinimumQualificationID"]);

                    model.sCountryCode =
                        dr["sCountryCode"] == DBNull.Value
                            ? ""
                            : dr["sCountryCode"].ToString()!;

                    model.sStateCode =
                        dr["sStateCode"] == DBNull.Value
                            ? ""
                            : dr["sStateCode"].ToString()!;

                    model.nCityID =
                        dr["nCityID"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(dr["nCityID"]);

                    model.sWorkingHours =
                        dr["sWorkingHours"] == DBNull.Value
                            ? ""
                            : dr["sWorkingHours"].ToString()!;

                    model.nInternshipTypeID =
                        Convert.ToInt32(dr["nInternshipTypeID"]);

                    model.sWorkingShift =
                        dr["sWorkingShift"] == DBNull.Value
                            ? ""
                            : dr["sWorkingShift"].ToString()!;

                    model.nInternshipFellowshipTypeID =
                        Convert.ToInt32(dr["nInternshipFellowshipTypeID"]);

                    if (dr["sTotalCharges"] != DBNull.Value)
                    {
                        model.sTotalCharges =
                            Convert.ToDecimal(dr["sTotalCharges"]);
                    }

                    model.sCurrency =
                        dr["sCurrency"] == DBNull.Value
                            ? ""
                            : dr["sCurrency"].ToString()!;

                    model.nTrainingInvolvedID =
                        Convert.ToInt32(dr["nTrainingInvolvedID"]);

                    model.nInternshipDurationID =
                        Convert.ToInt32(dr["nInternshipDurationID"]);

                    model.dStartDate =
                        Convert.ToDateTime(dr["dStartDate"]);

                    model.dCompletionDate =
                        Convert.ToDateTime(dr["dCompletionDate"]);

                    model.nInternshipModeID =
                        Convert.ToInt32(dr["nInternshipModeID"]);

                    model.sDivyang =
                        dr["sDivyang"] == DBNull.Value
                            ? ""
                            : dr["sDivyang"].ToString()!;

                    model.sLanguageKnown =
                        dr["sLanguageKnown"] == DBNull.Value
                            ? ""
                            : dr["sLanguageKnown"].ToString()!;

                    model.sWorkingDays =
                        dr["sWorkingDays"] == DBNull.Value
                            ? ""
                            : dr["sWorkingDays"].ToString()!;

                    model.sFacilities =
                        dr["sFacilities"] == DBNull.Value
                            ? ""
                            : dr["sFacilities"].ToString()!;

                    model.sMedicalSkills =
                        dr["sMedicalSkills"] == DBNull.Value
                            ? ""
                            : dr["sMedicalSkills"].ToString()!;

                    model.sTechnicalSkills =
                        dr["sTechnicalSkills"] == DBNull.Value
                            ? ""
                            : dr["sTechnicalSkills"].ToString()!;

                    model.sNonTechnicalSkills =
                        dr["sNonTechnicalSkills"] == DBNull.Value
                            ? ""
                            : dr["sNonTechnicalSkills"].ToString()!;
                }
            }

            // Convert comma-separated values into checkbox lists
            if (!string.IsNullOrWhiteSpace(model.sWorkingDays))
            {
                model.WorkingDays =
                    model.sWorkingDays
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToList();
            }

            if (!string.IsNullOrWhiteSpace(model.sFacilities))
            {
                model.Facilities =
                    model.sFacilities
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToList();
            }

            if (!string.IsNullOrWhiteSpace(model.sTechnicalSkills))
            {
                model.TechnicalSkillIDs =
                    model.sTechnicalSkills
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => int.Parse(x.Trim()))
                        .ToList();
            }

            if (!string.IsNullOrWhiteSpace(model.sMedicalSkills))
            {
                model.MedicalSkillIDs =
                    model.sMedicalSkills
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => int.Parse(x.Trim()))
                        .ToList();
            }

            if (!string.IsNullOrWhiteSpace(model.sNonTechnicalSkills))
            {
                model.NonTechnicalSkillIDs =
                    model.sNonTechnicalSkills
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => int.Parse(x.Trim()))
                        .ToList();
            }

            return View(model);
        }


        // =========================================================
        // EDIT POST - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPost(OrgPostM model)
        {
            int? sessionOrgID =
                HttpContext.Session.GetInt32("OrgID");

            if (sessionOrgID == null)
            {
                return RedirectToAction("OrganizationLogin", "Account");
            }

            int orgID = sessionOrgID.Value;

            model.nOrgID = orgID;

            try
            {
                string connectionString =
                    _configuration.GetConnectionString("DefaultConnection")!;

                using (SqlConnection con =
                       new SqlConnection(connectionString))
                using (SqlCommand cmd =
                       new SqlCommand(
                           "SP_UpdateOrganizationPost",
                           con))
                {
                    cmd.CommandType =
                        CommandType.StoredProcedure;

                    // =====================================================
                    // ID
                    // =====================================================

                    cmd.Parameters.AddWithValue(
                        "@nID",
                        model.nID);

                    cmd.Parameters.AddWithValue(
                        "@nOrgID",
                        orgID);

                    // =====================================================
                    // ROLE DETAILS
                    // =====================================================

                    cmd.Parameters.AddWithValue(
                        "@nPositionID",
                        model.nPositionID);

                    cmd.Parameters.AddWithValue(
                        "@nRequiredTrainees",
                        model.nRequiredTrainees);

                    cmd.Parameters.AddWithValue(
                        "@nGenderID",
                        model.nGenderID);

                    cmd.Parameters.AddWithValue(
                        "@nMinimumQualificationID",
                        model.nMinimumQualificationID);

                    // =====================================================
                    // LOCATION
                    // =====================================================

                    cmd.Parameters.AddWithValue(
                        "@sCountryCode",
                        model.sCountryCode ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@sStateCode",
                        model.sStateCode ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@nCityID",
                        model.nCityID);

                    cmd.Parameters.AddWithValue(
                        "@sWorkingHours",
                        model.sWorkingHours ?? (object)DBNull.Value);

                    // =====================================================
                    // WORK TERMS
                    // =====================================================

                    cmd.Parameters.AddWithValue(
                        "@nInternshipTypeID",
                        model.nInternshipTypeID);

                    cmd.Parameters.AddWithValue(
                        "@sWorkingShift",
                        model.sWorkingShift ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@nInternshipFellowshipTypeID",
                        model.nInternshipFellowshipTypeID);

                    cmd.Parameters.AddWithValue(
                        "@sTotalCharges",
                        model.sTotalCharges.HasValue
                            ? model.sTotalCharges.Value
                            : DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@sCurrency",
                        model.sCurrency ?? (object)DBNull.Value);

                    // =====================================================
                    // DURATION
                    // =====================================================

                    cmd.Parameters.AddWithValue(
                        "@nTrainingInvolvedID",
                        model.nTrainingInvolvedID);

                    cmd.Parameters.AddWithValue(
                        "@nInternshipDurationID",
                        model.nInternshipDurationID);

                    cmd.Parameters.AddWithValue(
                        "@dStartDate",
                        model.dStartDate);

                    cmd.Parameters.AddWithValue(
                        "@dCompletionDate",
                        model.dCompletionDate);

                    // =====================================================
                    // MODE
                    // =====================================================

                    cmd.Parameters.AddWithValue(
                        "@nInternshipModeID",
                        model.nInternshipModeID);

                    cmd.Parameters.AddWithValue(
                        "@sDivyang",
                        model.sDivyang ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@sLanguageKnown",
                        model.sLanguageKnown ?? (object)DBNull.Value);

                    // =====================================================
                    // WORKING DAYS
                    // =====================================================

                    string workingDays =
                        model.WorkingDays != null &&
                        model.WorkingDays.Count > 0
                            ? string.Join(",", model.WorkingDays)
                            : model.sWorkingDays ?? "";

                    cmd.Parameters.AddWithValue(
                        "@sWorkingDays",
                        string.IsNullOrWhiteSpace(workingDays)
                            ? DBNull.Value
                            : workingDays);

                    // =====================================================
                    // FACILITIES
                    // =====================================================

                    string facilities =
                        model.Facilities != null &&
                        model.Facilities.Count > 0
                            ? string.Join(",", model.Facilities)
                            : model.sFacilities ?? "";

                    cmd.Parameters.AddWithValue(
                        "@sFacilities",
                        string.IsNullOrWhiteSpace(facilities)
                            ? DBNull.Value
                            : facilities);

                    // =====================================================
                    // SKILLS
                    // =====================================================

                    string technicalSkills =
                        model.TechnicalSkillIDs != null &&
                        model.TechnicalSkillIDs.Count > 0
                            ? string.Join(",", model.TechnicalSkillIDs)
                            : model.sTechnicalSkills ?? "";

                    string medicalSkills =
                        model.MedicalSkillIDs != null &&
                        model.MedicalSkillIDs.Count > 0
                            ? string.Join(",", model.MedicalSkillIDs)
                            : model.sMedicalSkills ?? "";

                    string nonTechnicalSkills =
                        model.NonTechnicalSkillIDs != null &&
                        model.NonTechnicalSkillIDs.Count > 0
                            ? string.Join(",", model.NonTechnicalSkillIDs)
                            : model.sNonTechnicalSkills ?? "";

                    cmd.Parameters.AddWithValue(
                        "@sMedicalSkills",
                        string.IsNullOrWhiteSpace(medicalSkills)
                            ? DBNull.Value
                            : medicalSkills);

                    cmd.Parameters.AddWithValue(
                        "@sTechnicalSkills",
                        string.IsNullOrWhiteSpace(technicalSkills)
                            ? DBNull.Value
                            : technicalSkills);

                    cmd.Parameters.AddWithValue(
                        "@sNonTechnicalSkills",
                        string.IsNullOrWhiteSpace(nonTechnicalSkills)
                            ? DBNull.Value
                            : nonTechnicalSkills);

                    // =====================================================
                    // UPDATE
                    // =====================================================

                    con.Open();

                    int rows =
                        cmd.ExecuteNonQuery();

                    if (rows <= 0)
                    {
                        TempData["Error"] =
                            "Post was not updated.";

                        return View(model);
                    }
                }

                TempData["Success"] =
                    "Post updated successfully.";

                return RedirectToAction("PostDetails");
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;

                return View(model);
            }
        }

    }

}