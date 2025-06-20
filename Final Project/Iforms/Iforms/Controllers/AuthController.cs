using Iforms.BLL.DTOs;
using Iforms.BLL.Services;
using Iforms.DAL.Entity_Framework.Table_Models;
using Iforms.MVC.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Iforms.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService authService;
        private readonly JwtService jwtService;
        private readonly UserService userServices;
        private readonly AuditLogService auditLogService;

        public AuthController(AuthService authService, JwtService jwtService, UserService userServices , AuditLogService auditLogService)
        {
            this.authService = authService;
            this.jwtService = jwtService;
            this.userServices = userServices;
            this.auditLogService = auditLogService;
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
                //var name = userServices.GetById(user.Id)?.UserName ?? "User";
                HttpContext.Response.Cookies.Append("LoggedId", user.Id.ToString());
                HttpContext.Response.Cookies.Append("name", user.UserName);
                Response.Cookies.Append("JWTToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddHours(3)
                });
                if(user.UserRole == UserRole.Admin)
                {
                    auditLogService.RecordLog(user.Id, "Admin Login", "Admin logged in successfully");
                    //TempData["SuccessMsg"] = "Logged in successfully!";
                    return RedirectToAction("Index", "AdminDashboard");
                }
                else if(user.UserRole == UserRole.User)
                {
                    auditLogService.RecordLog(user.Id, "User Login", "User logged in successfully");
                    //TempData["SuccessMsg"] = "Logged in successfully!";
                    return RedirectToAction("Index", "UserDashboard");
                }
            }
            return View(userLoginDTO);
        }

        [AuthenticatedUser]
        [Route("logout")]
        [HttpPost]
        public IActionResult Logout()
        {
            auditLogService.RecordLog(int.Parse(HttpContext.Request.Cookies["LoggedId"]), "Logout", "User logged out successfully");
            Response.Cookies.Delete("JWTToken");
            TempData["SuccessMsg"] = "Logged out successfully!";
            return RedirectToAction("Login", "Auth");
        }
    }
}
