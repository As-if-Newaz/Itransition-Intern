using Iforms.BLL.DTOs;
using Iforms.BLL.Services;
using Iforms.DAL.Entity_Framework.Table_Models;
using Microsoft.AspNetCore.Mvc;

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
    }
}
