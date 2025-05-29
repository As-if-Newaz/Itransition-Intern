using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using User_Management.Auth;
using User_Management.Models;
using UserManagement.BLL.DTOs;
using UserManagement.BLL.Services;
using System.Linq;

namespace User_Management.Controllers
{
    public class DashboardController : Controller
    {
        private UserServices userServices;
        private ActivityService activityService;

        public DashboardController(UserServices userServices, ActivityService activityService)
        {
            this.userServices = userServices;
            this.activityService = activityService;
        }

        [Logged]
        [Route("Dashboard")]
        [HttpGet]
        public IActionResult Dashboard()
        {
            var users = userServices.GetAllWithActivity(activityService)
                .OrderByDescending(u => u.LastLogin)
                .ToList();
            return View(users);
        }

        [HttpPost]
        [Route("Dashboard/Block")]
        public JsonResult Block([FromBody] int[] userIds)
        {
            if (userIds == null || userIds.Length == 0)
                return Json(new { success = false, message = "No users selected." });
            var result = userServices.BlockUser(userIds);
            return Json(new { success = result, message = result ? "Users blocked successfully." : "Failed to block users." });
        }

        [HttpPost]
        [Route("Dashboard/Unblock")]
        public JsonResult Unblock([FromBody] int[] userIds)
        {
            if (userIds == null || userIds.Length == 0)
                return Json(new { success = false, message = "No users selected." });
            var result = userServices.UnblockUser(userIds);
            return Json(new { success = result, message = result ? "Users unblocked successfully." : "Failed to unblock users." });
        }

        [HttpPost]
        [Route("Dashboard/Delete")]
        public JsonResult Delete([FromBody] int[] userIds)
        {
            if (userIds == null || userIds.Length == 0)
                return Json(new { success = false, message = "No users selected." });
            var result = userServices.Delete(userIds);
            return Json(new { success = result, message = result ? "Users deleted successfully." : "Failed to delete users." });
        }
    }
}
