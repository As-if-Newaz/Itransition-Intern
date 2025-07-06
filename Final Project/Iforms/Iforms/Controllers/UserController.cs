using Iforms.BLL.DTOs;
using Iforms.BLL.Services;
using Iforms.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;

namespace Iforms.MVC.Controllers
{
    public class UserController : Controller
    {
        private UserService userServices;
        private EmailService emailService;
        public UserController(UserService userServices , EmailService emailService)
        {
            this.userServices = userServices;
            this.emailService = emailService;
        }

        [Route("register")]
        [HttpGet]
        public IActionResult Register()
        {
            return View(new UserDTO());
        }
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register(UserDTO userDTO, string confirmPassword)
        {
            if (userDTO.PasswordHash != confirmPassword)
            {
                TempData["ErrorMsg"] = "Passwords do not match.";
                return View(userDTO);
            }
            if (ModelState.IsValid)
            {
                string errorMsg = string.Empty;
                var result = userServices.Register(userDTO, out errorMsg);
                if (result)
                {
                    string subject = "Verification Code for Iforms";
                    string body = $"Your verification code for Iforms registration is: {userDTO.EmailVerificationCode}";
                    await emailService.SendEmailAsync(userDTO.UserEmail, subject, body);
                    TempData["SuccessMsg"] = "Registered Successfully! Please check your email for the verification code.";
                    return RedirectToAction("VerifyEmail", "User", new { email = userDTO.UserEmail });
                }
                else
                {
                    TempData["ErrorMsg"] = errorMsg;
                    return View(userDTO);
                }
            }
            return View(userDTO);
        }

        [HttpGet]
        [Route("verify-email")]
        public IActionResult VerifyEmail(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [Route("verify-email")]
        public IActionResult VerifyEmail(string email, string code)
        {
            var user = userServices.GetUserByEmail(email);
            if (user == null)
            {
                TempData["ErrorMsg"] = "User not found.";
                return View();
            }
            if (user.EmailVerificationCode != code)
            {
                TempData["ErrorMsg"] = "Invalid verification code.";
                ViewBag.Email = email;
                return View();
            }
            if (user.EmailVerificationExpiry < DateTime.UtcNow)
            {
                TempData["ErrorMsg"] = "Verification code expired.";
                ViewBag.Email = email;
                return View();
            }
            user.UserStatus = UserStatus.Active;
            user.EmailVerificationCode = null;
            user.EmailVerificationExpiry = null;
            userServices.UpdateUser(user);
            TempData["SuccessMsg"] = "Email verified! You can now log in.";
            return RedirectToAction("Login", "Auth");
        }

        [HttpPost]
        public IActionResult UpdateThemePreference([FromBody] ThemePreference model)
        {
            if (model == null || model.UserId <= 0)
                return Json(new { success = false, message = "Invalid data" });

            try
            {
                var user = userServices.GetById(model.UserId);
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                var theme = model.Theme == "Dark" ? Theme.Dark : Theme.Light;
                var preferences = new UserPreferencesDTO
                {
                    UserId = model.UserId,
                    PreferredLanguage = user.PreferredLanguage ?? Language.English,
                    PreferredTheme = theme
                };

                var result = userServices.UpdatePreferences(model.UserId, preferences);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating theme preference" });
            }
        }
    }
}
