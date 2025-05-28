using Microsoft.AspNetCore.Mvc;
using UserManagement.BLL.DTOs;
using UserManagement.BLL.Services;
using User_Management.Auth;

namespace User_Management.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;
        private readonly JwtService _jwtService;

        public AuthController(AuthService authService, JwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
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
                var user = _authService.Authenticate(userLoginDTO, out errorMsg);
                if (user == null)
                {
                    TempData["LoginMessage"] = errorMsg;
                    return View(userLoginDTO);
                }

                // Generate JWT token
                var token = _jwtService.GenerateToken(user);
                
                // Store token in cookie
                Response.Cookies.Append("JWTToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddHours(3)
                });

                TempData["LoginMessage"] = "Login Successful!";
                return RedirectToAction("Dashboard", "Dashboard");
            }
            return View(userLoginDTO);
        }

        [Logged]
        [Route("logout")]
        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("JWTToken");
            TempData["LogoutMessage"] = "Logged out successfully!";
            return RedirectToAction("Login", "Auth");
        }
    }
}
