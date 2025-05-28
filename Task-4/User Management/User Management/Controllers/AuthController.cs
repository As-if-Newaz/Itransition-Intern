using Microsoft.AspNetCore.Mvc;
using UserManagement.BLL.DTOs;
using UserManagement.BLL.Services;
using User_Management.Auth;

namespace User_Management.Controllers
{
    public class AuthController : Controller
    {
        private AuthService authService;

        public AuthController(AuthService authService)
        {
            this.authService = authService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("login")]
        [HttpGet]

        public IActionResult Login()
        {
            return View(new UserLoginDTO());
        }

        [Route("login")]
        [HttpPost]

        public IActionResult Login(UserLoginDTO userLoginDTO)
        {
            if (ModelState.IsValid)
            {
                string errorMsg;
                var user = authService.Authenticate(userLoginDTO, out errorMsg);
                if (user == null)
                {
                    TempData["LoginMessage"] = errorMsg;
                    return View(userLoginDTO);
                }

                TempData["LoginMessage"] = "Login Successful!";
                HttpContext.Session.SetString("userId", user.UserId.ToString());
                return RedirectToAction("Dashboard", "Dashboard");
            }
            return View(userLoginDTO);
        }
        [Logged]
        [Route("logout")]
        [HttpPost]
        
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("userId");
            TempData["LogoutMessage"] = "Logged out successfully!";
            return RedirectToAction("Login", "Auth");
        }
    }
}
