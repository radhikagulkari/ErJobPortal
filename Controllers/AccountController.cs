using ErJobPortal.Models;
using ErJobPortal.Repositories;
using Microsoft.AspNetCore.Mvc;
using ErJobPortal.Services;


namespace ErJobPortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountRepository _accountRepository;
        private readonly EmailService _emailService;

        public AccountController(AccountRepository accountRepository, EmailService emailService)
        {
            _accountRepository = accountRepository;
            _emailService = emailService;
        }

        // Candidate Registration
        [HttpGet]
        public IActionResult CandidateRegister()
        {
            return View();
        }

        // ==========================================
        // GET DEPARTMENTS
        // ==========================================

        [HttpGet]
        public IActionResult GetCandidateDepartments()
        {
            var departments = _accountRepository.GetDepartments();
            return Json(departments);
        }


        // ==========================================
        // GET BRANCHES BY DEPARTMENT
        // ==========================================

        [HttpGet]
        public IActionResult GetCandidateBranches(int departmentId)
        {
            var branches = _accountRepository.GetBranches(departmentId);
            return Json(branches);
        }

        // ==========================================
        // COLLEGE
        // ==========================================

        [HttpGet]
        public IActionResult GetCandidateColleges()
        {
            var colleges = _accountRepository.GetColleges();
            return Json(colleges);
        }


        // ==========================================
        // COLLEGE CODE BY COLLEGE
        // ==========================================

        [HttpGet]
        public IActionResult GetCandidateCollegeCodes(int collegeId)
        {
            var codes = _accountRepository.GetCollegeCodes(collegeId);
            return Json(codes);
        }

        // ==============================
        // CANDIDATE REGISTER - POST
        // ==============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CandidateRegister(CandidateRegister model)
        {
            // STATIC OTP FOR NOW
            //if (model.sOTP != "123456")
            //{
            //    ModelState.AddModelError(
            //        "sOTP",
            //        "Invalid OTP. Please enter 123456."
            //    );
            //}

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {

                // ==========================================
                // PROFILE IMAGE UPLOAD
                // ==========================================

                if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
                {
                    // ==========================================
                    // ALLOWED IMAGE TYPES
                    // ==========================================

                    string[] allowedExtensions =
                    {
    ".png",
    ".jpg",
    ".jpeg"
};

                    string extension = Path.GetExtension(
                            model.ProfileImageFile.FileName
                        ).ToLowerInvariant();

                    // ==========================================
                    // CHECK IMAGE EXTENSION
                    // ==========================================

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError(
                            "ProfileImageFile",
                            "Only PNG, JPG and JPEG images are allowed."
                        );

                        return View(model);
                    }

                    // ==========================================
                    // MAXIMUM 5 MB
                    // ==========================================

                    if (model.ProfileImageFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError(
                            "ProfileImageFile",
                            "Image size must be less than 5 MB."
                        );

                        return View(model);
                    }

                    // ==========================================
                    // CREATE UPLOAD FOLDER
                    // ==========================================

                    string uploadFolder = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads",
                        "candidates"
                    );

                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    // ==========================================
                    // GENERATE IMAGE NAME: 1, 2, 3, 4...
                    // ==========================================

                    int nextNumber = 1;

                    while (Directory.GetFiles(
                        uploadFolder,
                        nextNumber + ".*"
                    ).Length > 0)
                    {
                        nextNumber++;
                    }

                    // Example: 1.jpg, 2.png, 3.jpeg
                    string imageName =
                        nextNumber + extension;

                    // ==========================================
                    // SAVE IMAGE
                    // ==========================================

                    string filePath = Path.Combine(
                        uploadFolder,
                        imageName
                    );

                    using (FileStream stream =
                           new FileStream(
                               filePath,
                               FileMode.Create))
                    {
                        model.ProfileImageFile.CopyTo(stream);
                    }

                    // ==========================================
                    // ONLY IMAGE NAME GOES TO DATABASE
                    // ==========================================

                    model.sProfileImage = imageName;
                }
                else
                {
                    model.sProfileImage = "";
                }

                int result = _accountRepository.Register(model);

                if (result > 0)
                {
                    TempData["Success"] =
                        "Registration successful. Please login.";

                    return RedirectToAction("CandidateLogin");
                }

                ModelState.AddModelError(
                    "",
                    "Registration failed."
                );

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.Message
                );

                return View(model);
            }
        }



        // Organization Registration
        [HttpGet]
        public IActionResult OrganizationRegister()
        {
            OrganizationRegister model = new OrganizationRegister();
            return View();
        }

        // ==========================================
        // GET DEPARTMENTS BY COLLEGE
        // ==========================================

        [HttpGet]
        public IActionResult GetDepartments()
        {
            var departments =
                _accountRepository.GetDepartments();

            return Json(departments);
        }

        [HttpGet]
        public IActionResult GetBranches(int departmentId)
        {
            var branches =
                _accountRepository.GetBranches(departmentId);

            return Json(branches);
        }

        #region "College"

        [HttpGet]
        public IActionResult GetColleges()
        {
            var colleges = _accountRepository.GetColleges();

            return Json(colleges);
        }

        #endregion


        #region "College Code"

        [HttpGet]
        public IActionResult GetCollegeCodes(int collegeId)
        {
            var codes = _accountRepository.GetCollegeCodes(collegeId);

            return Json(codes);
        }

        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OrganizationRegister(OrganizationRegister model)
        {
            try
            {
                // STATIC OTP FOR NOW
                if (model.sOTP != "123456")
                {
                    ModelState.AddModelError(
                        "sOTP",
                        "Invalid OTP. Please enter 123456."
                    );
                }

                // Check validation errors
                if (!ModelState.IsValid)
                {
                    foreach (var item in ModelState)
                    {
                        foreach (var error in item.Value.Errors)
                        {
                            Console.WriteLine(
                                $"Field: {item.Key}, Error: {error.ErrorMessage}"
                            );
                        }
                    }

                    return View(model);
                }

                int result = _accountRepository.RegisterOrganization(model);

                if (result > 0)
                {
                    TempData["Success"] =
                        "Organization Registration Successfully.";

                    return RedirectToAction("OrganizationRegister");
                }

                TempData["Error"] =
                    "Organization Registration Failed.";

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Organization Register Error:");
                Console.WriteLine(ex.ToString());

                ModelState.AddModelError(
                    "",
                    ex.Message
                );

                return View(model);
            }
        }

        [HttpGet]
        public IActionResult OrganizationLogin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult OrganizationLogin(OrganizationLogin model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            OrganizationUser? user = _accountRepository.LoginOrganization(model);

            if (user != null)
            {
                HttpContext.Session.SetInt32("OrgID", user.nID);
                HttpContext.Session.SetString("OrgName", user.sOrgName ?? "");
                HttpContext.Session.SetString("OrgEmail", user.sEmail ?? "");
                return RedirectToAction("Dashboard", "Organization");
            }

            TempData["Error"] = "Invalid Email or Password";
            return View(model);
        }

        // candidate login

        [HttpGet]
        public IActionResult CandidateLogin()
        {
            return View();
        }


        [HttpPost]
        public IActionResult CandidateLogin(CandidateLogin model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            CandidateUser? user =
                _accountRepository.LoginCandidate(model);

            if (user != null)
            {
                HttpContext.Session.SetInt32(
                    "CandidateID",
                    user.nID);

                HttpContext.Session.SetString(
                    "CandidateName",
                    (user.sFName + " " + user.sLName).Trim());

                HttpContext.Session.SetString(
                    "CandidateEmail",
                    user.sEmail ?? "");

                return RedirectToAction(
                    "Dashboard",
                    "Candidate");
            }

            TempData["Error"] =
                "Invalid Email or Password";

            return View(model);
        }

        // ================= LOGOUT =================
        [HttpGet]
        public IActionResult Logout()
        {
            // Remove login session
            HttpContext.Session.Clear();

            // Remove session cookie
            Response.Cookies.Delete(".AspNetCore.Session");

            // Prevent cached pages
            Response.Headers["Cache-Control"] =
                "no-cache, no-store, must-revalidate";

            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            // Go to Home/Index
            return RedirectToAction("Index", "Home");
        }

        // shrirang 15/09/26

        // ==========================================================
        // Candidate Forgot Password
        // ==========================================================

        [HttpGet]
        public IActionResult CandidateForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CandidateForgotPassword(
            CandidateForgotPassword model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                string email = model.sEmail.Trim();

                CandidateLogin? candidate =
                    _accountRepository.GetCandidateLoginDetails(email);

                if (candidate == null)
                {
                    ModelState.AddModelError(
                        "sEmail",
                        "No candidate account was found with this email address."
                    );

                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(candidate.sPassword))
                {
                    ModelState.AddModelError(
                        "",
                        "Password information is not available for this account."
                    );

                    return View(model);
                }

                await _emailService.SendCandidateLoginDetailsAsync(
                    candidate.sEmail,
                    candidate.sPassword
                );

                TempData["Success"] =
                    "Your login details have been sent to your registered email address.";

                return RedirectToAction("CandidateLogin");
            }
            catch (Exception ex)
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Candidate Forgot Password Error");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("========================================");

                ModelState.AddModelError(
                    "",
                    "Email Error: " + ex.Message
                );

                return View(model);
            }

           
        }

        
// ==========================================================
// Candidate Reset Password
// ==========================================================

[HttpGet]
public IActionResult CandidateResetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CandidateResetPassword(
            CandidateResetPassword model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                string email = model.sEmail.Trim();

                // Check whether candidate exists
                CandidateLogin? candidate =
                    _accountRepository.GetCandidateLoginDetails(email);

                if (candidate == null)
                {
                    ModelState.AddModelError(
                        "sEmail",
                        "No candidate account was found with this email address."
                    );

                    return View(model);
                }

                // Update password
                bool updated =
                    _accountRepository.ResetCandidatePassword(
                        email,
                        model.NewPassword
                    );

                if (!updated)
                {
                    ModelState.AddModelError(
                        "",
                        "Unable to reset password. Please try again."
                    );

                    return View(model);
                }

                TempData["Success"] =
                    "Your password has been reset successfully. Please login with your new password.";

                return RedirectToAction("CandidateLogin");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "Candidate Reset Password Error: "
                    + ex.ToString()
                );

                ModelState.AddModelError(
                    "",
                    "Unable to reset password. Please try again later."
                );

                return View(model);
            }
        }

      
// ==========================================================
// ORGANIZATION RESET PASSWORD
// ==========================================================

[HttpGet]
public IActionResult OrganizationResetPassword()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OrganizationResetPassword(
            OrganizationResetPassword model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                string email = model.sEmail.Trim();

                // Check whether organization email exists
                OrganizationLogin? organization =
                    _accountRepository.GetOrganizationLoginDetails(email);

                if (organization == null)
                {
                    ModelState.AddModelError(
                        "sEmail",
                        "No organization account was found with this email address."
                    );

                    return View(model);
                }

                // Update password
                bool updated =
                    _accountRepository.ResetOrganizationPassword(
                        email,
                        model.NewPassword
                    );

                if (!updated)
                {
                    ModelState.AddModelError(
                        "",
                        "Unable to reset password. Please try again."
                    );

                    return View(model);
                }

                TempData["Success"] =
                    "Your password has been reset successfully. Please login with your new password.";

                return RedirectToAction("OrganizationLogin");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "Organization Reset Password Error: "
                    + ex.ToString()
                );

                ModelState.AddModelError(
                    "",
                    "Unable to reset password. Please try again later."
                );

                return View(model);
            }
        }

        // ==========================================================
        // Organization Forgot Password
        // ==========================================================

        [HttpGet]
        public IActionResult OrganizationForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrganizationForgotPassword(
            OrganizationForgotPassword model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                string email = model.sEmail.Trim();

                // Check whether organization exists
                OrganizationLogin? organization =
                    _accountRepository.GetOrganizationLoginDetails(email);

                if (organization == null)
                {
                    ModelState.AddModelError(
                        "sEmail",
                        "No organization account was found with this email address."
                    );

                    return View(model);
                }

                if (string.IsNullOrWhiteSpace(organization.sPassword))
                {
                    ModelState.AddModelError(
                        "",
                        "Password information is not available for this account."
                    );

                    return View(model);
                }

                // Send login details to registered email
                await _emailService.SendOrganizationLoginDetailsAsync(
                    organization.sEmail,
                    organization.sPassword
                );

                TempData["Success"] =
                    "Your login details have been sent to your registered email address.";

                return RedirectToAction("OrganizationLogin");
            }
            catch (Exception ex)
            {
                Console.WriteLine("========================================");
                Console.WriteLine("Organization Forgot Password Error");
                Console.WriteLine(ex.ToString());
                Console.WriteLine("========================================");

                ModelState.AddModelError(
                    "",
                    "Email Error: " + ex.Message
                );

                return View(model);
            }
        }




    }
}