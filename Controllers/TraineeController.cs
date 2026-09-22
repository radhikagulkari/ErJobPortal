using ErJobPortal.Models;
using ErJobPortal.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ErJobPortal.Controllers
{
    public class TraineeController : Controller
    {
        private readonly AccountRepository _traineeRepository;

        public TraineeController(AccountRepository traineeRepository)
        {
            _traineeRepository = traineeRepository;
        }

        //[HttpGet]
        //public IActionResult Index()
        //{
        //    List<TraineeM> trainees =
        //        _traineeRepository.GetAllTrainees();

        //    return View(trainees);
        //}
    }
}