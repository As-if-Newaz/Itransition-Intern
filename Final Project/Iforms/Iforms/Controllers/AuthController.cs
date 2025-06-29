using Iforms.BLL.DTOs;
using Iforms.BLL.Services;
using Iforms.DAL.Entity_Framework.Table_Models;
using Iforms.MVC.Authentication;
using Microsoft.AspNetCore.Mvc;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;

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
                HttpContext.Response.Cookies.Append("Role", user.UserRole.ToString());
                HttpContext.Response.Cookies.Append("Theme", user.PreferredTheme.ToString());
                HttpContext.Response.Cookies.Append("Language" , user.PreferredLanguage.ToString());
                Response.Cookies.Append("JWTToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.Now.AddHours(3)
                });
                if(user.UserStatus == UserStatus.Inactive)
                {
                    TempData["ErrorMsg"] = "Your account is inactive. Please verify your email.";
                    return RedirectToAction("VerifyEmail", "User");
                }
                else if(user.UserStatus == UserStatus.Blocked)
                {
                    TempData["ErrorMsg"] = "Your account is blocked. Please contact support.";
                    return View(userLoginDTO);
                }
                if (user.UserRole == UserRole.Admin)
                {
                    auditLogService.RecordLog(user.Id, "Admin Login", "Admin logged in successfully");
                    //TempData["SuccessMsg"] = "Logged in successfully!";
                    return RedirectToAction("Index", "Home");
                }
                else if(user.UserRole == UserRole.User)
                {
                    auditLogService.RecordLog(user.Id, "User Login", "User logged in successfully");
                    //TempData["SuccessMsg"] = "Logged in successfully!";
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(userLoginDTO);
        }
        [AuthenticatedAdminorUser]
        [Route("logout")]
        [HttpPost]
        public IActionResult Logout()
        {
            auditLogService.RecordLog(int.Parse(HttpContext.Request.Cookies["LoggedId"]), "Logout", "User logged out successfully");
            Response.Cookies.Delete("JWTToken");
            Response.Cookies.Delete("LoggedId");
            Response.Cookies.Delete("name");
            Response.Cookies.Delete("Role");
            Response.Cookies.Delete("Theme");
            Response.Cookies.Delete("Language");
            TempData["SuccessMsg"] = "Logged out successfully!";
            return RedirectToAction("Index", "Home");
        }
    }
}
