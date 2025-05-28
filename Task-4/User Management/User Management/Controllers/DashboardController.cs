using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using User_Management.Auth;
using User_Management.Models;
using UserManagement.BLL.DTOs;
using UserManagement.BLL.Services;

namespace User_Management.Controllers
{
    public class DashboardController : Controller
    {
        private UserServices userServices;

        public DashboardController(UserServices userServices)
        {
            this.userServices = userServices;
        }

        [Logged]
        [Route("Dashboard")]
        [HttpGet]
        public IActionResult Dashboard()
        {
            var users = userServices.GetAll();
            return View(users);

        }


    }
}
