using Microsoft.AspNetCore.Mvc;
using UserManagement.BLL.DTOs;
using UserManagement.BLL.Services;
using User_Management.Auth;

namespace User_Management.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService authService;
        private readonly JwtService jwtService;
        private readonly UserServices userServices;

        public AuthController(AuthService authService, JwtService jwtService, UserServices userServices)
        {
            this.authService = authService;
            this.jwtService = jwtService;
            this.userServices = userServices;   
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("login")]
        [HttpGet]
        public IActionResult Login(string errorMessage = null)
        {
            if (!string.IsNullOrEmpty(errorMessage))
            {
                TempData["ErrorMsg"] = errorMessage;
            }
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
                    TempData["ErrorMsg"] = errorMsg;
                    return View(userLoginDTO);
                }
                var token = jwtService.GenerateToken(user);
                var name = userServices.GetById(user.UserId)?.UserName ?? "User";
                HttpContext.Response.Cookies.Append("name", name);
                Response.Cookies.Append("JWTToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddHours(3)
                });

                TempData["SuccessMsg"] = "Login Successful!";
                return RedirectToAction("Dashboard", "Dashboard");
            }
            return View(userLoginDTO);
        }
        [AuthenticatedUser]
        [Route("logout")]
        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("JWTToken");
            TempData["SuccessMsg"] = "Logged out successfully!";
            return RedirectToAction("Login", "Auth");
        }
    }
}
