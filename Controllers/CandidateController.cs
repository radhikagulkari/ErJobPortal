using ErJobPortal.Models;
using ErJobPortal.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace ErJobPortal.Controllers
{
    public class CandidateController : Controller
    {
        private readonly CandidateProfileRepository _repo;
        private readonly IConfiguration _configuration;
        private readonly AccountRepository _repository;

        public CandidateController(IConfiguration configuration, AccountRepository repository, CandidateProfileRepository repo)
        {
            _configuration = configuration;
            _repository = repository;
            _repo = repo;
        }


        private string? GetCandidateCode()
        {
            int? candidateId = HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null || candidateId <= 0)
                return null;

            var candidate =
                _repository.GetCandidateRegistrationDetails(candidateId.Value);

            if (candidate == null ||
                candidate.Value.DOB == null ||
                candidate.Value.RegDate == null)
            {
                return null;
            }

            string candidateCode =
                "CD" +
                candidate.Value.DOB.Value.ToString("ddMMyy") +
                candidate.Value.RegDate.Value.ToString("MMdd") +
                candidate.Value.CandidateID.ToString("D2");

            return candidateCode;
        }

        // =========================================================
        // DASHBOARD
        // =========================================================

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Dashboard(string? id)
        {
            int? candidateId =
                HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null)
            {
                return RedirectToAction(
                    "CandidateLogin",
                    "Account");
            }

            // =========================================================
            // GET CANDIDATE REGISTRATION DETAILS
            // =========================================================

            var candidate =
                _repository.GetCandidateRegistrationDetails(
                    candidateId.Value);

            if (candidate == null)
            {
                return NotFound("Candidate registration not found.");
            }

            // =========================================================
            // CHECK DOB AND REGISTRATION DATE
            // =========================================================

            if (candidate.Value.DOB == null ||
                candidate.Value.RegDate == null)
            {
                return BadRequest(
                    "Candidate DOB or Registration Date is missing.");
            }

            // =========================================================
            // CREATE CANDIDATE CODE
            // =========================================================
            //
            // CD
            // + DOB ddMMyy
            // + Registration Date MMdd
            // + Candidate ID 2 digits
            //
            // Example:
            // DOB      = 08/05/1999
            // RegDate  = 26/07/2026
            // nID      = 1
            //
            // CD080599072601
            // =========================================================

            string candidateCode =
                "CD" +
                candidate.Value.DOB.Value.ToString("ddMMyy") +
                candidate.Value.RegDate.Value.ToString("MMdd") +
                candidate.Value.CandidateID.ToString("D2");

            // =========================================================
            // IF NORMAL URL IS OPENED
            // REDIRECT TO CODE URL
            // =========================================================

            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(
                    "Dashboard",
                    "Candidate",
                    new { id = candidateCode });
            }

            // =========================================================
            // OPTIONAL: CHECK THAT URL CODE BELONGS TO LOGGED-IN
            // CANDIDATE
            // =========================================================

            if (id != candidateCode)
            {
                return NotFound();
            }

            // =========================================================
            // EXISTING DASHBOARD CODE
            // =========================================================

            ViewBag.CandidateID =
                candidateId;

            ViewBag.CandidateName =
                HttpContext.Session.GetString(
                    "CandidateName");

            ViewBag.CandidateEmail =
                HttpContext.Session.GetString(
                    "CandidateEmail");

            ViewBag.CandidateCode =
                candidateCode;

            // =========================================================
            // ORGANIZATION COUNT
            // =========================================================

            int organizationRegistrationCount = 0;

            string connectionString =
                _configuration.GetConnectionString(
                    "DefaultConnection");

            using (SqlConnection con =
                   new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"
     SELECT COUNT(nID)
     FROM tblOrgRegistration;";

                using (SqlCommand cmd =
                       new SqlCommand(query, con))
                {
                    organizationRegistrationCount =
                        Convert.ToInt32(
                            cmd.ExecuteScalar());
                }
            }

            // =========================================================
            // ORGANIZATION LIST
            // =========================================================

            List<OrganizationUser> organizations =
                _repository.GetAllOrganizationList();

            ViewBag.OrganizationRegistrationCount =
                organizationRegistrationCount;

            ViewBag.Organizations =
                organizations;

            return View();
        }


        // ==========================================
        // ORGANIZATION LIST
        // ==========================================
        [HttpGet]
        [Route("Candidate/OrgList/{id?}")]
        public IActionResult OrgList(string? id)
        {
            int? candidateId =
                HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null || candidateId <= 0)
            {
                return RedirectToAction(
                    "CandidateLogin",
                    "Account");
            }

            string? candidateCode = GetCandidateCode();

            if (string.IsNullOrEmpty(candidateCode))
            {
                return NotFound("Candidate code could not be generated.");
            }

            // If URL does not contain candidate code, add it
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(
                    "OrgList",
                    "Candidate",
                    new { id = candidateCode });
            }

            // Prevent another/wrong candidate code
            if (id != candidateCode)
            {
                return NotFound();
            }

            List<OrganizationUser> organization =
                _repository.GetAllOrganizationList();

            ViewBag.CandidateCode = candidateCode;
            ViewBag.CandidateID = candidateId.Value;

            return View(organization);
        }

        [HttpGet]
        [Route("Candidate/EditProfile/{id?}")]
        public IActionResult EditProfile(string? id)
        {
            // =========================================================
            // GET CANDIDATE ID FROM SESSION
            // =========================================================
            int? candidateId =
                HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null || candidateId <= 0)
            {
                return RedirectToAction(
                    "CandidateLogin",
                    "Account");
            }

            // =========================================================
            // GET CANDIDATE CODE
            // =========================================================
            string? candidateCode = GetCandidateCode();

            if (string.IsNullOrEmpty(candidateCode))
            {
                return NotFound(
                    "Candidate code could not be generated.");
            }

            // =========================================================
            // IF URL DOES NOT HAVE ID
            // REDIRECT TO CODE URL
            // =========================================================
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(
                    "EditProfile",
                    "Candidate",
                    new { id = candidateCode });
            }

            // =========================================================
            // VALIDATE URL ID
            // =========================================================
            if (id != candidateCode)
            {
                return NotFound();
            }

            // =========================================================
            // SEND CODE TO VIEW / LAYOUT
            // =========================================================
            ViewBag.CandidateCode = candidateCode;
            ViewBag.CandidateID = candidateId.Value;

            return View();
        }

        // =========================================================
        // GET PROFILE
        // =========================================================
        [HttpGet]
        public IActionResult Profile(string id)
        {
            // =========================================================
            // GET CANDIDATE ID FROM SESSION
            // =========================================================
            int? candidateId =
                HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null || candidateId <= 0)
            {
                return RedirectToAction(
                    "CandidateLogin",
                    "Account");
            }

            // =========================================================
            // GET CANDIDATE CODE
            // =========================================================
            string? candidateCode = GetCandidateCode();

            if (string.IsNullOrEmpty(candidateCode))
            {
                return NotFound(
                    "Candidate code could not be generated.");
            }

            // =========================================================
            // IF URL DOES NOT CONTAIN ID
            // REDIRECT TO CODE URL
            // =========================================================
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(
                    "Profile",
                    "Candidate",
                    new { id = candidateCode });
            }

            // =========================================================
            // VALIDATE CANDIDATE CODE
            // =========================================================
            if (id != candidateCode)
            {
                return NotFound();
            }

            // =========================================================
            // GET PROFILE
            // =========================================================
            CandidateProfileModel? profile =
                _repo.GetProfile(candidateId.Value);

            if (profile == null)
            {
                profile = new CandidateProfileModel
                {
                    CandidateID = candidateId.Value
                };
            }

            // =========================================================
            // LOAD DROPDOWNS
            // =========================================================
            CandidateProfileViewModel vm =
                LoadProfileDropdowns(profile);

            // =========================================================
            // SEND CODE TO VIEW / LAYOUT
            // =========================================================
            ViewBag.CandidateCode = candidateCode;

            return View(vm);
        }


        // =========================================================
        // LOAD ALL DROPDOWNS
        // =========================================================

        private CandidateProfileViewModel LoadProfileDropdowns(
    CandidateProfileModel profile)
        {
            return new CandidateProfileViewModel
            {
                Profile = profile,

                Divisions =
                    _repo.GetDivisions(),

                Streams =
                    _repo.GetStreams(),

                GraduationStatuses =
                    _repo.GetGraduationStatuses(),

                InternshipFellowshipType =
                    _repo.GetInternshipFellowshipType(),

                InternshipTitles =
                    _repo.GetInternshipTitles(),

                InternshipDurations =
                    _repo.GetInternshipDurations(),

                InternshipStatuses =
                    _repo.GetInternshipStatuses(),

                Relationships =
                    _repo.GetRelationships()
            };
        }

        // =====================================================
        // UPDATE ADDRESS
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateAddress(CandidateProfileViewModel model)
        {
            int? candidateId =
                HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null)
            {
                return RedirectToAction(
                    "CandidateLogin",
                    "Account"
                );
            }

            _repo.UpdateAddress(
                candidateId.Value,
                model.Profile.CountryID,
                model.Profile.StateID,
                model.Profile.CityID,
                model.Profile.Pincode
            );
            TempData["Success"] = "Address updated successfully.";
            return RedirectToAction("Profile");
        }

        // =====================================================
        // UPDATE EDUCATION
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateEducation([Bind(Prefix = "Profile")] CandidateProfileModel model)
        {
            int? candidateId = HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null)
            {
                return RedirectToAction("CandidateLogin", "Account");
            }

            model.CandidateID = candidateId.Value;

            _repo.UpdateEducation(model);

            TempData["Success"] = "Education updated successfully.";

            return RedirectToAction("Profile");
        }


        // =====================================================
        // UPDATE INTERNSHIP / FELLOWSHIP PREFERENCE
        // =====================================================

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult UpdateInternshipPreference(CandidateProfileModel model)
        //{
        //    int? candidateId = HttpContext.Session.GetInt32("CandidateID");

        //    if (candidateId == null)
        //    {
        //        return RedirectToAction("CandidateLogin", "Account");
        //    }

        //    _repo.UpdateInternshipPreference(
        //        candidateId.Value,
        //        model.Internship_FellowshipType,
        //        model.Preferred_Country,
        //        model.Preferred_State,
        //        model.Preferred_City
        //    );

        //    TempData["Success"] = "Internship / Fellowship Preference updated successfully.";

        //    return RedirectToAction("Profile");
        //}

        // =====================================================
        // UPDATE DOCUMENTS
        // =====================================================

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult UpdateDocuments(CandidateProfileModel model)
        //{
        //    int? candidateId = HttpContext.Session.GetInt32("CandidateID");

        //    if (candidateId == null)
        //    {
        //        return RedirectToAction("CandidateLogin", "Account");
        //    }

        //    _repo.UpdateDocuments(
        //        candidateId.Value,
        //        model.sResume,
        //        model.sPhoto,
        //        model.sSignature,
        //        model.sDivyang,
        //        model.sHobbies
        //    );

        //    TempData["Success"] = "Documents updated successfully.";

        //    return RedirectToAction("Profile");
        //}


        // =========================================================
        // UPDATE INTERNSHIP DETAILS
        // =========================================================

    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public IActionResult UpdateInternshipPreference(
    //[Bind(Prefix = "Profile")] CandidateProfileModel model)
    //    {
    //        int? candidateId =
    //            HttpContext.Session.GetInt32("CandidateID");

    //        if (candidateId == null)
    //        {
    //            return RedirectToAction(
    //                "CandidateLogin",
    //                "Account"
    //            );
    //        }

    //        model.CandidateID = candidateId.Value;

    //        _repo.UpdateInternshipPreference(model);

    //        TempData["Success"] =
    //            "Internship / Fellowship Preference updated successfully.";

    //        return RedirectToAction("Profile");
    //    }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult UpdateLanguages([Bind(Prefix = "Profile")] CandidateProfileModel model)
        //{
        //    int? candidateId = HttpContext.Session.GetInt32("CandidateID");

        //    if (candidateId == null)
        //    {
        //        return RedirectToAction("CandidateLogin", "Account");
        //    }

        //    model.CandidateID = candidateId.Value;

        //    _repo.UpdateLanguages(model);

        //    TempData["Success"] = "Languages updated successfully.";

        //    return RedirectToAction("Profile");
        //}


        // =========================================================
        // UPDATE REFERENCES
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateReferences([Bind(Prefix = "Profile")] CandidateProfileModel model)
        {
            int? candidateId = HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null)
            {
                return RedirectToAction("CandidateLogin", "Account");
            }

            model.CandidateID = candidateId.Value;

            _repo.UpdateReferences(model);

            TempData["Success"] = "References updated successfully.";

            return RedirectToAction("Profile");
        }


        // =========================================================
        // UPDATE ACHIEVEMENTS
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateAchievements([Bind(Prefix = "Profile")] CandidateProfileModel model)
        {
            int? candidateId = HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null)
            {
                return RedirectToAction("CandidateLogin", "Account");
            }

            model.CandidateID = candidateId.Value;

            _repo.UpdateAchievements(model);

            TempData["Success"] = "Achievements updated successfully.";

            return RedirectToAction("Profile");
        }


        // =========================================================
        // UPDATE LINKS
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateLinks([Bind(Prefix = "Profile")] CandidateProfileModel model)
        {
            int? candidateId = HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null)
            {
                return RedirectToAction("CandidateLogin", "Account");
            }

            model.CandidateID = candidateId.Value;

            _repo.UpdateLinks(model);

            TempData["Success"] = "Links updated successfully.";

            return RedirectToAction("Profile");
        }


        // =========================================================
        // UPDATE OBJECTIVE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateObjective(CandidateProfileModel model)
        {
            int? candidateId = HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null)
            {
                return RedirectToAction("CandidateLogin", "Account");
            }

            model.CandidateID = candidateId.Value;

            _repo.UpdateObjective(model);

            TempData["Success"] = "Career objective updated successfully.";

            return RedirectToAction("Profile");
        }


        // =========================================================
        // UPDATE SKILLS
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateSkills([Bind(Prefix = "Profile")] CandidateProfileModel model)
        {
            int? candidateId = HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null)
            {
                return RedirectToAction("CandidateLogin", "Account");
            }

            model.CandidateID = candidateId.Value;

            _repo.UpdateSkills(model);

            TempData["Success"] = "Skills updated successfully.";

            return RedirectToAction("Profile");
        }


        // =========================================================
        // UPDATE DOCUMENTS / HOBBIES
        // =========================================================

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult UpdateDocuments([Bind(Prefix = "Profile")] CandidateProfileModel model)
        //{
        //    int? candidateId = HttpContext.Session.GetInt32("CandidateID");

        //    if (candidateId == null)
        //    {
        //        return RedirectToAction("CandidateLogin", "Account");
        //    }

        //    model.CandidateID = candidateId.Value;

        //    _repo.UpdateDocuments(model);

        //    TempData["Success"] = "Documents and personal details updated successfully.";

        //    return RedirectToAction("Profile");
        //}

        //// =========================================================
        //// UPDATE INTERNSHIP / FELLOWSHIP PREFERENCE
        //// =========================================================

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult UpdateInternshipPreference([Bind(Prefix = "Profile")] CandidateProfileViewModel model)
        //{
        //    int? candidateId = HttpContext.Session.GetInt32("CandidateID");

        //    if (candidateId == null)
        //    {
        //        return RedirectToAction("CandidateLogin", "Account");
        //    }

        //    // Never trust CandidateID from form
        //    model.Profile.CandidateID = candidateId.Value;

        //    _repo.UpdateInternshipPreference(model.Profile);

        //    TempData["Success"] =
        //        "Internship Preference Updated Successfully.";

        //    return RedirectToAction("Profile");
        //}

        // =========================================================
        // SEARCH JOBS
        // =========================================================
        [HttpGet]
        [Route("Candidate/SearchJobs/{id?}")]
        public IActionResult SearchJobs(string? id)
        {
            int? candidateId =
                HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null || candidateId <= 0)
            {
                return RedirectToAction(
                    "CandidateLogin",
                    "Account");
            }

            string? candidateCode = GetCandidateCode();

            if (string.IsNullOrEmpty(candidateCode))
            {
                return NotFound("Candidate code could not be generated.");
            }

            // If ID is missing, add it to URL
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(
                    "SearchJobs",
                    "Candidate",
                    new { id = candidateCode });
            }

            // Validate ID
            if (id != candidateCode)
            {
                return NotFound();
            }

            ViewBag.CandidateCode = candidateCode;
            ViewBag.CandidateID = candidateId.Value;

            return View();
        }


        // =========================================================
        // MY APPLICATIONS
        // =========================================================
        [HttpGet]
        [Route("Candidate/MyApplications/{id?}")]
        public IActionResult MyApplications(string? id)
        {
            int? candidateId =
                HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null || candidateId <= 0)
            {
                return RedirectToAction(
                    "CandidateLogin",
                    "Account");
            }

            string? candidateCode = GetCandidateCode();

            if (string.IsNullOrEmpty(candidateCode))
            {
                return NotFound("Candidate code could not be generated.");
            }

            // If ID is missing, add it to URL
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(
                    "MyApplications",
                    "Candidate",
                    new { id = candidateCode });
            }

            // Validate ID
            if (id != candidateCode)
            {
                return NotFound();
            }

            ViewBag.CandidateCode = candidateCode;
            ViewBag.CandidateID = candidateId.Value;

            return View();
        }


        // =========================================================
        // CHANGE PASSWORD
        // =========================================================
        [HttpGet]
        [Route("Candidate/ChangePassword/{id?}")]
        public IActionResult ChangePassword(string? id)
        {
            int? candidateId =
                HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null || candidateId <= 0)
            {
                return RedirectToAction(
                    "CandidateLogin",
                    "Account");
            }

            string? candidateCode = GetCandidateCode();

            if (string.IsNullOrEmpty(candidateCode))
            {
                return NotFound("Candidate code could not be generated.");
            }

            // If ID is missing, add it to URL
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(
                    "ChangePassword",
                    "Candidate",
                    new { id = candidateCode });
            }

            // Validate ID
            if (id != candidateCode)
            {
                return NotFound();
            }

            ViewBag.CandidateCode = candidateCode;
            ViewBag.CandidateID = candidateId.Value;

            return View();
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
            return RedirectToAction("Index", "Home");
        }


        // =========================================================
        // NO CACHE
        // =========================================================

        private void SetNoCacheHeaders()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
        }



        // =========================================================
        // CANDIDATE CREATE FEEDBACK - GET
        // =========================================================
        [HttpGet]
        public IActionResult CreateFeedback()
        {
            // =====================================================
            // GET CANDIDATE ID
            // =====================================================

            int? candidateId =
                HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null || candidateId <= 0)
            {
                return RedirectToAction(
                    "CandidateLogin",
                    "Account");
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
            FROM tblSATRFeedback
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

                                nCandidateID = candidateId.Value,

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

            int? candidateId =
                HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null || candidateId <= 0)
            {
                TempData["Error"] =
                    "Candidate session expired. Please login again.";

                return RedirectToAction(
                    "CandidateLogin",
                    "Account");
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
            INSERT INTO tblCandidateFeedback
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
                            candidateId.Value;

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

            TempData["Success"] =
                "Feedback submitted successfully.";

            return RedirectToAction(
                "CreateFeedback");
        }



        // shrirang 27/08/26

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult UpdateDocuments([Bind(Prefix = "Profile")] CandidateProfileModel model, IFormFile? ResumeFile, IFormFile? PhotoFile, IFormFile? SignatureFile)
        //{
        //    int? candidateId =
        //        HttpContext.Session.GetInt32("CandidateID");

        //    if (candidateId == null || candidateId <= 0)
        //    {
        //        return RedirectToAction(
        //            "CandidateLogin",
        //            "Account");
        //    }

        //    model.CandidateID = candidateId.Value;

        //    // Get existing profile
        //    CandidateProfileModel? existingProfile =
        //        _repo.GetProfile(candidateId.Value);

        //    if (existingProfile != null)
        //    {
        //        // Keep old files if user does not select a new file
        //        model.sResume = existingProfile.sResume;
        //        model.sPhoto = existingProfile.sPhoto;
        //        model.sSignature = existingProfile.sSignature;
        //    }

        //    // Upload folder
        //    string uploadFolder = Path.Combine(
        //        Directory.GetCurrentDirectory(),
        //        "wwwroot",
        //        "uploads",
        //        "candidates");

        //    if (!Directory.Exists(uploadFolder))
        //    {
        //        Directory.CreateDirectory(uploadFolder);
        //    }


        //    // =====================================================
        //    // RESUME
        //    // =====================================================

        //    if (ResumeFile != null && ResumeFile.Length > 0)
        //    {
        //        string extension =
        //            Path.GetExtension(ResumeFile.FileName)
        //                .ToLowerInvariant();

        //        if (extension != ".pdf" &&
        //            extension != ".doc" &&
        //            extension != ".docx")
        //        {
        //            TempData["Error"] =
        //                "Resume must be PDF, DOC or DOCX.";

        //            return RedirectToAction("Profile");
        //        }

        //        string fileName =
        //            $"{candidateId}_Resume{extension}";

        //        string filePath =
        //            Path.Combine(uploadFolder, fileName);

        //        using (FileStream stream =
        //               new FileStream(filePath, FileMode.Create))
        //        {
        //            ResumeFile.CopyTo(stream);
        //        }

        //        model.sResume =
        //            $"/uploads/candidates/{fileName}";
        //    }


        //    // =====================================================
        //    // PHOTO
        //    // =====================================================

        //    if (PhotoFile != null && PhotoFile.Length > 0)
        //    {
        //        string extension =
        //            Path.GetExtension(PhotoFile.FileName)
        //                .ToLowerInvariant();

        //        if (extension != ".jpg" &&
        //            extension != ".jpeg" &&
        //            extension != ".png")
        //        {
        //            TempData["Error"] =
        //                "Photo must be JPG, JPEG or PNG.";

        //            return RedirectToAction("Profile");
        //        }

        //        string fileName =
        //            $"{candidateId}_Photo{extension}";

        //        string filePath =
        //            Path.Combine(uploadFolder, fileName);

        //        using (FileStream stream =
        //               new FileStream(filePath, FileMode.Create))
        //        {
        //            PhotoFile.CopyTo(stream);
        //        }

        //        model.sPhoto =
        //            $"/uploads/candidates/{fileName}";
        //    }


        //    // =====================================================
        //    // SIGNATURE
        //    // =====================================================

        //    if (SignatureFile != null && SignatureFile.Length > 0)
        //    {
        //        string extension =
        //            Path.GetExtension(SignatureFile.FileName)
        //                .ToLowerInvariant();

        //        if (extension != ".jpg" &&
        //            extension != ".jpeg" &&
        //            extension != ".png")
        //        {
        //            TempData["Error"] =
        //                "Signature must be JPG, JPEG or PNG.";

        //            return RedirectToAction("Profile");
        //        }

        //        string fileName =
        //            $"{candidateId}_Signature{extension}";

        //        string filePath =
        //            Path.Combine(uploadFolder, fileName);

        //        using (FileStream stream =
        //               new FileStream(filePath, FileMode.Create))
        //        {
        //            SignatureFile.CopyTo(stream);
        //        }

        //        model.sSignature =
        //            $"/uploads/candidates/{fileName}";
        //    }


        //    // =====================================================
        //    // UPDATE EVERYTHING TOGETHER
        //    // =====================================================

        //    _repo.UpdateDocuments(model);

        //    TempData["Success"] =
        //        "Personal details updated successfully.";

        //    return RedirectToAction("Profile");
        //}

        // =========================================================
        // UPDATE INTERNSHIP DETAILS
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateInternshipDetails(
            [Bind(Prefix = "Profile")] CandidateProfileModel model)
        {
            // =====================================================
            // GET CANDIDATE ID FROM SESSION
            // =====================================================

            int? candidateId =
                HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null || candidateId <= 0)
            {
                return RedirectToAction(
                    "CandidateLogin",
                    "Account");
            }


            // =====================================================
            // NEVER TRUST CANDIDATE ID FROM FORM
            // =====================================================

            model.CandidateID =
                candidateId.Value;


            // =====================================================
            // UPDATE INTERNSHIP DETAILS
            // =====================================================

            _repo.UpdateInternshipDetails(model);


            // =====================================================
            // SUCCESS MESSAGE
            // =====================================================

            TempData["Success"] =
                "Internship details updated successfully.";


            // =====================================================
            // REDIRECT
            // =====================================================

            return RedirectToAction("Profile");
        }

        // =========================================================
        // UPDATE LANGUAGES
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateLanguages([Bind(Prefix = "Profile")] CandidateProfileModel model)
        {
            // =====================================================
            // GET CANDIDATE ID FROM SESSION
            // =====================================================

            int? candidateId =
                HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null || candidateId <= 0)
            {
                return RedirectToAction(
                    "CandidateLogin",
                    "Account");
            }


            // =====================================================
            // NEVER TRUST CANDIDATE ID FROM FORM
            // =====================================================

            model.CandidateID = candidateId.Value;


            // =====================================================
            // UPDATE LANGUAGES
            // =====================================================

            _repo.UpdateLanguages(model);


            // =====================================================
            // SUCCESS MESSAGE
            // =====================================================

            TempData["Success"] =
                "Languages updated successfully.";


            // =====================================================
            // REDIRECT
            // =====================================================

            return RedirectToAction("Profile");
        }


        // =========================================================
        // UPDATE SOCIAL LINKS
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateSocialLinks([Bind(Prefix = "Profile")] CandidateProfileModel model)
        {
            // Get Candidate ID from session
            int? candidateId = HttpContext.Session.GetInt32("CandidateID");

            // Check login
            if (candidateId == null || candidateId <= 0)
            {
                return RedirectToAction("CandidateLogin", "Account");
            }

            // Never trust CandidateID coming from the form
            model.CandidateID = candidateId.Value;

            // Update GitHub and LinkedIn
            _repo.UpdateSocialLinks(model);

            // Success message
            TempData["Success"] = "Social links updated successfully.";

            // Return to Profile
            return RedirectToAction("Profile");
        }

        // =========================================================
        // VIEW PROFILE (READ-ONLY RESUME VIEW) RADHIKA
        // =========================================================
        // shrirang 19/09/26

        // =========================================================
        // VIEW PROFILE
        // =========================================================

        [HttpGet]
        [ResponseCache(
            NoStore = true,
            Location = ResponseCacheLocation.None)]
        public IActionResult ViewProfile()
        {
            // -----------------------------------------------------
            // GET LOGGED-IN CANDIDATE
            // -----------------------------------------------------

            int? candidateId =
                HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null || candidateId <= 0)
            {
                return RedirectToAction(
                    "CandidateLogin",
                    "Account");
            }

            // -----------------------------------------------------
            // ALWAYS LOAD FRESH DATA FROM DATABASE
            // -----------------------------------------------------

            CandidateProfileModel? profile =
                _repo.GetProfile(candidateId.Value);

            // -----------------------------------------------------
            // IF PROFILE DOES NOT EXIST
            // -----------------------------------------------------

            if (profile == null)
            {
                profile = new CandidateProfileModel
                {
                    CandidateID = candidateId.Value
                };
            }

            // -----------------------------------------------------
            // LOAD PROFILE + ALL LOOKUP LISTS
            // -----------------------------------------------------

            CandidateProfileViewModel vm =
                LoadProfileDropdowns(profile);

            // -----------------------------------------------------
            // CANDIDATE INFORMATION
            // -----------------------------------------------------

            ViewBag.CandidateID =
                candidateId.Value;

            ViewBag.CandidateName =
                HttpContext.Session.GetString("CandidateName");

            ViewBag.CandidateEmail =
                HttpContext.Session.GetString("CandidateEmail");

            // -----------------------------------------------------
            // PREVENT BROWSER CACHE
            // -----------------------------------------------------

            Response.Headers["Cache-Control"] =
                "no-cache, no-store, must-revalidate";

            Response.Headers["Pragma"] =
                "no-cache";

            Response.Headers["Expires"] =
                "0";

            return View(vm);
        }

        [HttpGet]
        public IActionResult InternshipDetails(string id)
        {
            // id = CD080900080701

            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Dashboard");
            }

            // Your code to get internship details using id

            return View();
        }

        // shrirang 14/09/26
        // shrirang 12/09/26


        // =========================================================
        // UPDATE INTERNSHIP / FELLOWSHIP PREFERENCE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateInternshipPreference(
            [Bind(Prefix = "Profile")] CandidateProfileModel model)
        {
            // =====================================================
            // GET CANDIDATE ID FROM SESSION
            // =====================================================

            int? candidateId =
                HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null || candidateId <= 0)
            {
                return RedirectToAction(
                    "CandidateLogin",
                    "Account");
            }

            // =====================================================
            // NEVER TRUST CANDIDATE ID FROM FORM
            // =====================================================

            model.CandidateID = candidateId.Value;

            // =====================================================
            // VALIDATE INTERNSHIP / FELLOWSHIP TYPE
            // =====================================================

            if (model.Internship_FellowshipType == null)
            {
                ModelState.AddModelError(
                    "Internship_FellowshipType",
                    "Please select Internship / Fellowship Type.");

                // Reload dropdowns because the view needs them
                CandidateProfileViewModel vm =
                    LoadProfileDropdowns(model);

                return View("Profile", vm);
            }

            // =====================================================
            // UPDATE DATABASE
            // =====================================================

            _repo.UpdateInternshipPreference(model);

            // =====================================================
            // SUCCESS MESSAGE
            // =====================================================

            TempData["Success"] =
                "Internship / Fellowship Preference updated successfully.";

            // =====================================================
            // REDIRECT
            // =====================================================

            return RedirectToAction("Profile");
        }

        // =========================================================
        // UPDATE DOCUMENTS / PERSONAL DETAILS
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateDocuments(
            [Bind(Prefix = "Profile")] CandidateProfileModel model,
            IFormFile? ResumeFile,
            IFormFile? PhotoFile,
            IFormFile? SignatureFile)
        {
            // =====================================================
            // GET CANDIDATE ID FROM SESSION
            // =====================================================

            int? candidateId =
                HttpContext.Session.GetInt32("CandidateID");

            if (candidateId == null || candidateId <= 0)
            {
                return RedirectToAction(
                    "CandidateLogin",
                    "Account");
            }

            // Never trust CandidateID from browser
            model.CandidateID = candidateId.Value;

            // =====================================================
            // GET EXISTING PROFILE
            // =====================================================

            CandidateProfileModel? existingProfile =
                _repo.GetProfile(candidateId.Value);

            if (existingProfile != null)
            {
                // Preserve existing documents
                // if no new file is selected.

                if (string.IsNullOrEmpty(model.sResume))
                {
                    model.sResume = existingProfile.sResume;
                }

                if (string.IsNullOrEmpty(model.sPhoto))
                {
                    model.sPhoto = existingProfile.sPhoto;
                }

                if (string.IsNullOrEmpty(model.sSignature))
                {
                    model.sSignature = existingProfile.sSignature;
                }
            }

            // =====================================================
            // UPLOAD DIRECTORY
            // =====================================================

            string uploadFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "candidates");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            // =====================================================
            // RESUME
            // =====================================================

            if (ResumeFile != null &&
                ResumeFile.Length > 0)
            {
                string extension =
                    Path.GetExtension(
                        ResumeFile.FileName)
                        .ToLowerInvariant();

                if (extension != ".pdf" &&
                    extension != ".doc" &&
                    extension != ".docx")
                {
                    TempData["Error"] =
                        "Resume must be PDF, DOC or DOCX.";

                    return RedirectToAction("Profile");
                }

                string fileName =
                    $"{candidateId.Value}_Resume{extension}";

                string filePath =
                    Path.Combine(
                        uploadFolder,
                        fileName);

                using (FileStream stream =
                       new FileStream(
                           filePath,
                           FileMode.Create))
                {
                    ResumeFile.CopyTo(stream);
                }

                model.sResume =
                    $"/uploads/candidates/{fileName}";
            }

            // =====================================================
            // PHOTO
            // =====================================================

            if (PhotoFile != null &&
                PhotoFile.Length > 0)
            {
                string extension =
                    Path.GetExtension(
                        PhotoFile.FileName)
                        .ToLowerInvariant();

                if (extension != ".jpg" &&
                    extension != ".jpeg" &&
                    extension != ".png")
                {
                    TempData["Error"] =
                        "Photo must be JPG, JPEG or PNG.";

                    return RedirectToAction("Profile");
                }

                string fileName =
                    $"{candidateId.Value}_Photo{extension}";

                string filePath =
                    Path.Combine(
                        uploadFolder,
                        fileName);

                using (FileStream stream =
                       new FileStream(
                           filePath,
                           FileMode.Create))
                {
                    PhotoFile.CopyTo(stream);
                }

                model.sPhoto =
                    $"/uploads/candidates/{fileName}";
            }

            // =====================================================
            // SIGNATURE
            // =====================================================

            if (SignatureFile != null &&
                SignatureFile.Length > 0)
            {
                string extension =
                    Path.GetExtension(
                        SignatureFile.FileName)
                        .ToLowerInvariant();

                if (extension != ".jpg" &&
                    extension != ".jpeg" &&
                    extension != ".png")
                {
                    TempData["Error"] =
                        "Signature must be JPG, JPEG or PNG.";

                    return RedirectToAction("Profile");
                }

                string fileName =
                    $"{candidateId.Value}_Signature{extension}";

                string filePath =
                    Path.Combine(
                        uploadFolder,
                        fileName);

                using (FileStream stream =
                       new FileStream(
                           filePath,
                           FileMode.Create))
                {
                    SignatureFile.CopyTo(stream);
                }

                model.sSignature =
                    $"/uploads/candidates/{fileName}";
            }

            // =====================================================
            // DEBUG / VALIDATION
            // =====================================================

            // At this point model should contain:
            //
            // model.CandidateID
            // model.sResume
            // model.sPhoto
            // model.sSignature
            // model.sDivyang
            // model.sHobbies

            // =====================================================
            // UPDATE DATABASE
            // =====================================================

            _repo.UpdateDocuments(model);

            TempData["Success"] =
                "Documents and personal details updated successfully.";

            return RedirectToAction("Profile");
        }

        public void UpdateDocuments(CandidateProfileModel model)
        {
            string connectionString =
                _configuration.GetConnectionString(
                    "DefaultConnection");

            using (SqlConnection con =
                   new SqlConnection(connectionString))
            {
                string query = @"
            UPDATE tblCandidateProfile
            SET
                sResume = @sResume,
                sPhoto = @sPhoto,
                sSignature = @sSignature,
                sDivyang = @sDivyang,
                sHobbies = @sHobbies,
                ModDate = GETDATE()
            WHERE CandidateID = @CandidateID";

                using (SqlCommand cmd =
                       new SqlCommand(query, con))
                {
                    cmd.Parameters.Add(
                        "@CandidateID",
                        SqlDbType.Int).Value =
                            model.CandidateID;

                    cmd.Parameters.Add(
                        "@sResume",
                        SqlDbType.NVarChar,
                        500).Value =
                            (object?)model.sResume ??
                            DBNull.Value;

                    cmd.Parameters.Add(
                        "@sPhoto",
                        SqlDbType.NVarChar,
                        500).Value =
                            (object?)model.sPhoto ??
                            DBNull.Value;

                    cmd.Parameters.Add(
                        "@sSignature",
                        SqlDbType.NVarChar,
                        500).Value =
                            (object?)model.sSignature ??
                            DBNull.Value;

                    cmd.Parameters.Add(
                        "@sDivyang",
                        SqlDbType.NVarChar,
                        50).Value =
                            (object?)model.sDivyang ??
                            DBNull.Value;

                    cmd.Parameters.Add(
                        "@sHobbies",
                        SqlDbType.NVarChar,
                        -1).Value =
                            (object?)model.sHobbies ??
                            DBNull.Value;

                    con.Open();

                    int rows = cmd.ExecuteNonQuery();

                    if (rows == 0)
                    {
                        throw new Exception(
                            "No candidate record found for CandidateID: "
                            + model.CandidateID);
                    }
                }
            }
        }


    }
}