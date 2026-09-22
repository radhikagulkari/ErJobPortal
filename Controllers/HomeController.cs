using ErJobPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using ErJobPortal.Data;
using ErJobPortal.Services;

namespace ErJobPortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly EmailService _emailService;
        private readonly ILogger<HomeController> _logger;
        private readonly DbConnection _dbConnection;

        public HomeController(
    ILogger<HomeController> logger,
    DbConnection dbConnection,
    EmailService emailService)
        {
            _logger = logger;
            _dbConnection = dbConnection;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
        public IActionResult Organization()
        {
            return View();
        }
        public IActionResult TeamInvolved()
        {
            return View();
        }
        public IActionResult Terms_Cond()
        {
            return View();
        }
        public IActionResult Org_blog()
        {
            return View();
        }
        public IActionResult Benefits()
        {
            return View();
        }
        public IActionResult Tips()
        {
            return View();
        }
        public IActionResult TopCarriers()
        {
            return View();
        }
        public IActionResult Transformation()
        {
            return View();
        }
        public IActionResult Embracing_change()
        {
            return View();
        }
        public IActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactUs(ViewModelTrainee rg)
        {
            try
            {
                // ==========================================
                // VALIDATION
                // ==========================================

                if (rg == null)
                {
                    TempData["ContactUs"] =
                        "Please enter valid contact details.";

                    return RedirectToAction("Contact");
                }

                if (string.IsNullOrWhiteSpace(rg.FullName) ||
                    string.IsNullOrWhiteSpace(rg.Email) ||
                    string.IsNullOrWhiteSpace(rg.Subject) ||
                    string.IsNullOrWhiteSpace(rg.Description))
                {
                    TempData["ContactUs"] =
                        "Please fill all required fields.";

                    return RedirectToAction("Contact");
                }

                // ==========================================
                // TRIM VALUES
                // ==========================================

                rg.FullName = rg.FullName.Trim();
                rg.Email = rg.Email.Trim();
                rg.MobileNo = rg.MobileNo?.Trim();
                rg.Subject = rg.Subject.Trim();
                rg.Description = rg.Description.Trim();

                // ==========================================
                // SAVE CONTACT DATA
                // ==========================================

                using (SqlConnection con =
                    _dbConnection.GetConnection())
                {
                    await con.OpenAsync();

                    string query = @"
                INSERT INTO tblContactUs
                (
                    FullName,
                    Email,
                    MobileNo,
                    Subject,
                    Description
                )
                VALUES
                (
                    @FullName,
                    @Email,
                    @MobileNo,
                    @Subject,
                    @Description
                )";

                    using (SqlCommand cmd =
                        new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue(
                            "@FullName",
                            (object?)rg.FullName ?? DBNull.Value);

                        cmd.Parameters.AddWithValue(
                            "@Email",
                            (object?)rg.Email ?? DBNull.Value);

                        cmd.Parameters.AddWithValue(
                            "@MobileNo",
                            (object?)rg.MobileNo ?? DBNull.Value);

                        cmd.Parameters.AddWithValue(
                            "@Subject",
                            (object?)rg.Subject ?? DBNull.Value);

                        cmd.Parameters.AddWithValue(
                            "@Description",
                            (object?)rg.Description ?? DBNull.Value);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                // ==========================================
                // SEND THANK-YOU EMAIL
                // TO USER-ENTERED EMAIL ADDRESS
                // ==========================================

                try
                {
                    await _emailService.SendContactThankYouEmailAsync(
                        rg.FullName,
                        rg.Email,
                        rg.Subject,
                        rg.Description);

                    _logger.LogInformation(
                        "Contact thank-you email sent successfully to {Email}",
                        rg.Email);
                }
                catch (Exception emailEx)
                {
                    // Contact data is already saved.
                    // Log email failure separately.

                    _logger.LogError(
                        emailEx,
                        "Contact saved but thank-you email failed. Recipient: {Email}",
                        rg.Email);

                    TempData["ContactUs"] =
                        "Your message was saved, but the confirmation email " +
                        "could not be sent. Please try again.";

                    return RedirectToAction("Contact");
                }

                // ==========================================
                // SUCCESS
                // ==========================================

                ModelState.Clear();

                TempData["ContactUs"] =
                    "Thank you for contacting us! " +
                    "Your message has been sent successfully.";

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while saving Contact Us form.");

                TempData["ContactUs"] =
                    "Something went wrong. Please try again.";

                return RedirectToAction("Contact");
            }
        }

        public IActionResult Interview_Preparation()
        {
            return View();
        }

        public IActionResult IndustryNews()
        {
            return View();
        }

        public IActionResult MarketInsight()
        {
            return View();
        }

        public IActionResult EmergingTechnologies()
        {
            return View();
        }

        public IActionResult CareerTrends()
        {
            return View();
        }

        public IActionResult ExpertOpinion()
        {
            return View();
        }

        public IActionResult SectorSpecificNews()
        {
            return View();
        }

        public ActionResult SuccessStories()
        {

            return View();
        }

        public ActionResult CoverLetter()
        {
            return View();
        }
        public ActionResult FAQ()
        {
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
