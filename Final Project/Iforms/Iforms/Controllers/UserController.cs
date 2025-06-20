using Iforms.BLL.DTOs;
using Iforms.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace Iforms.MVC.Controllers
{
    public class UserController : Controller
    {
        private UserService userServices;
        public UserController(UserService userServices)
        {
            this.userServices = userServices;
        }

        [Route("register")]
        [HttpGet]
        public IActionResult Register()
        {
            return View(new UserDTO());
        }
        [Route("register")]
        [HttpPost]
        public IActionResult Register(UserDTO userDTO, string confirmPassword)
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
                    TempData["SuccessMsg"] = "Registered Successfully!";
                    return RedirectToAction("Login", "Auth");
                }
                else
                {
                    TempData["ErrorMsg"] = errorMsg;
                    return View(userDTO);
                }
            }
            return View(userDTO);
        }
    }
}
