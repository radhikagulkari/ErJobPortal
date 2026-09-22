using ErJobPortal.Models;
using ErJobPortal.Repositories;
using ErJobPortal.Services;
using Microsoft.AspNetCore.Mvc;

namespace ErJobPortal.Controllers
{
    public class SALoginController : Controller
    {
        private readonly AccountRepository _repository;
        private readonly EmailService _emailService;

        public SALoginController(AccountRepository repository, EmailService emailService)
        {
            _repository = repository;
            _emailService = emailService;
        }

        // ==========================================================
        // SUPER ADMIN LOGIN - GET
        // ==========================================================

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        // ==========================================================
        // SUPER ADMIN LOGIN - POST
        // ==========================================================

        [HttpPost]
        public IActionResult Index(SALoginModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user =
                _repository.Login(
                    model.sEmail,
                    model.sPassword);

            if (user != null)
            {
                // Store Super Admin details in session

                HttpContext.Session.SetString(
                    "SAID",
                    user.nID.ToString()
                );

                HttpContext.Session.SetString(
                    "SAName",
                    user.sFName
                );

                HttpContext.Session.SetString(
                    "SARole",
                    user.sRole
                );

                // Redirect to Super Admin Dashboard

                return RedirectToAction(
                    "Dashboard",
                    "SuperAdmin",
                    new { id = user.nID }
                );
            }

            ViewBag.Error =
                "Invalid Email or Password";

            return View(model);
        }


        // ==========================================================
        // SUPER ADMIN RESET PASSWORD - GET
        // ==========================================================

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }


        // ==========================================================
        // SUPER ADMIN RESET PASSWORD - POST
        // ==========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(
            SuperAdminResetPassword model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                string email =
                    model.sEmail.Trim();

                // Check Super Admin email

                SALoginModel? admin =
                    _repository.GetSuperAdminLoginDetails(
                        email);

                if (admin == null)
                {
                    ModelState.AddModelError(
                        "sEmail",
                        "No Super Admin account was found with this email address."
                    );

                    return View(model);
                }


                // Reset password

                bool updated =
                    _repository.ResetSuperAdminPassword(
                        email,
                        model.NewPassword);


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

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "Super Admin Reset Password Error: "
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
        // SUPER ADMIN FORGOT PASSWORD - GET
        // ==========================================================

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        // ==========================================================
        // SUPER ADMIN FORGOT PASSWORD - POST
        // ==========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
            SAForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                string email =
                    model.sEmail.Trim();


                // ==================================================
                // GET SUPER ADMIN LOGIN DETAILS
                // ==================================================

                SALoginModel? admin =
                    _repository.GetSuperAdminLoginDetails(
                        email);


                if (admin == null)
                {
                    ModelState.AddModelError(
                        "sEmail",
                        "No Super Admin account was found with this email address."
                    );

                    return View(model);
                }


                // ==================================================
                // CHECK PASSWORD
                // ==================================================

                if (string.IsNullOrWhiteSpace(
                    admin.sPassword))
                {
                    ModelState.AddModelError(
                        "",
                        "Password information is not available for this account."
                    );

                    return View(model);
                }


                // ==================================================
                // SEND LOGIN DETAILS
                // ==================================================

                await _emailService.SendSuperAdminLoginDetailsAsync(
                    admin.sEmail,
                    admin.sPassword
                );


                // ==================================================
                // SUCCESS
                // ==================================================

                TempData["Success"] =
                    "Your login details have been sent to your registered email address.";


                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "========================================");

                Console.WriteLine(
                    "Super Admin Forgot Password Error");

                Console.WriteLine(
                    ex.ToString());

                Console.WriteLine(
                    "========================================");


                ModelState.AddModelError(
                    "",
                    "Unable to send login details. Please try again later."
                );

                return View(model);
            }
        }
    }
}