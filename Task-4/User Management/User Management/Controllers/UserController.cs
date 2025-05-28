using Microsoft.AspNetCore.Mvc;
using UserManagement.BLL.DTOs;
using UserManagement.BLL.Services;

namespace User_Management.Controllers
{
    public class UserController : Controller
    {
        private UserServices userServices;

        public UserController(UserServices userServices)
        {
            this.userServices = userServices;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("register")]
        [HttpGet]
        public IActionResult Register()
        {
            return View(new UserDTO());
        }
        [Route("register")]
        [HttpPost]
        public IActionResult Register(UserDTO userDTO)
        {

            if (ModelState.IsValid)
            {
                string errorMsg = string.Empty;
                var result = userServices.Register(userDTO, out errorMsg);
                if (result)
                {

                    TempData["RegisterMessage"] = "Registered Successfully!";
                    return View(userDTO);
                }
                else
                {
                    TempData["RegisterMessage"] = errorMsg;
                }
            }
            return View(userDTO);
        }
    }
}

