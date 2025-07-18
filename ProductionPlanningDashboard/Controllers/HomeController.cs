using Microsoft.AspNetCore.Mvc;

namespace ProductionPlanningDashboard.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Dashboard");
        }
    }
}