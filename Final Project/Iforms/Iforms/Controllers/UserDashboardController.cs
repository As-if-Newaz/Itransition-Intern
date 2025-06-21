using Microsoft.AspNetCore.Mvc;

namespace Iforms.MVC.Controllers
{
    public class UserDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
