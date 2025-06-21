using Microsoft.AspNetCore.Mvc;

namespace Iforms.MVC.Controllers
{
    public class PublicDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
