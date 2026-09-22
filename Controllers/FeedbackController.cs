using ErJobPortal.Models;
using ErJobPortal.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ErJobPortal.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly AccountRepository _feedbackRepository;

        public FeedbackController(AccountRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }

        // =========================
        // Feedback List
        // =========================
        [HttpGet]
        public IActionResult Index()
        {
            List<FeedbackM> feedbackList =
                _feedbackRepository.GetAllFeedback();

            return View(feedbackList);
        }

        // =========================
        // Feedback Create - GET
        // =========================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // =========================
        // Feedback Create - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FeedbackM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int? adminID = HttpContext.Session.GetInt32("AdminID");

            if (adminID != null)
            {
                model.nAdminID = adminID.Value;
            }

            model.RegDate = DateTime.Now;
            model.ModDate = DateTime.Now;
            model.nBit = true;
            model.nSABit = false;

            int result = _feedbackRepository.InsertFeedback(model);

            if (result > 0)
            {
                TempData["Success"] =
                    "Feedback submitted successfully.";

                return RedirectToAction("Create");
            }

            TempData["Error"] =
                "Feedback submission failed.";

            return View(model);
        }

        // =========================
        // Edit - GET
        // =========================
        [HttpGet]
        public IActionResult Edit(int id)
        {
            FeedbackM? feedback =
                _feedbackRepository.GetFeedbackById(id);

            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // =========================
        // Edit - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(FeedbackM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.ModDate = DateTime.Now;

            int result =
                _feedbackRepository.UpdateFeedback(model);

            if (result > 0)
            {
                TempData["Success"] =
                    "Feedback updated successfully.";

                return RedirectToAction("Index");
            }

            TempData["Error"] =
                "Feedback update failed.";

            return View(model);
        }

        // =========================
        // Delete
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            int result =
                _feedbackRepository.DeleteFeedback(id);

            if (result > 0)
            {
                TempData["Success"] =
                    "Feedback deleted successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Feedback delete failed.";
            }

            return RedirectToAction("Index");
        }
    }
}