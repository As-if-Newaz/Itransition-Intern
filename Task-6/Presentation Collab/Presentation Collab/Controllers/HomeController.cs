using Microsoft.AspNetCore.Mvc;

namespace Presentation_Collab.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDBContext context;

        public HomeController(ApplicationDBContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            //var presentations = context.Presentations
            //    .OrderByDescending(p => p.CreatedAt)
            //    .ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Join(string nickname)
        {
            if (string.IsNullOrEmpty(nickname))
            {
                return RedirectToAction("Index");
            }

            HttpContext.Session.SetString("UserNickname", nickname);
            return RedirectToAction("Index", "Presentation");
        }
    }
}
