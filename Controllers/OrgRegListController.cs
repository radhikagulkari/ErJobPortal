using ErJobPortal.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ErJobPortal.Controllers
{
    public class OrgRegListController : Controller
    {
        private readonly AccountRepository _repository;

        public OrgRegListController(AccountRepository repository)
        {
            _repository = repository;
        }

        // Organization Registration List
        public IActionResult OrgRegList()
        {
            var dt = _repository.GetAllOrganization();
            return View(dt);
        }

        // Enable / Disable Organization
        [HttpPost]
        public IActionResult UpdateOrganizationStatus(int id, bool status)
        {
            _repository.UpdateOrganizationStatus(id, status);
            return RedirectToAction("OrgRegList");
        }
    }
}
