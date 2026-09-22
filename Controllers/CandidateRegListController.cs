using ErJobPortal.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ErJobPortal.Controllers
{
    public class CandidateRegListController : Controller
    {
        private readonly AccountRepository _repository;

        public CandidateRegListController(AccountRepository repository)
        {
            _repository = repository;
        }

        // Candidate Registration List
        public IActionResult CandidateRegList()
        {
            var dt = _repository.GetAllCandidate();
            return View(dt);
        }

        // Enable / Disable Candidate
        [HttpPost]
        public IActionResult UpdateCandidateStatus(int id, bool status)
        {
            _repository.UpdateCandidateStatus(id, status);
            return RedirectToAction("CandidateRegList");
        }
    }
}
