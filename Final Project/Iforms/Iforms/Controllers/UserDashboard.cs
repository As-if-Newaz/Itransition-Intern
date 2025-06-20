using Microsoft.AspNetCore.Mvc;

namespace Iforms.MVC.Controllers
{
    public class UserDashboard : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
