using Azure;
using Iforms.BLL.DTOs;
using Iforms.BLL.Services;
using Iforms.DAL.Entity_Framework.Table_Models;
using Iforms.MVC.Authentication;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;

namespace Iforms.MVC.Controllers
{
    [Route("AdminDashboard")]
    [AuthenticatedAdmin]
    public class AdminDashboardController : Controller
    {
        private UserService userServices;
        private AuditLogService auditLogService;
        private TemplateService templateService;
        private FormService formService;    


        public AdminDashboardController(UserService userServices, AuditLogService auditLogService, TemplateService templateService, FormService formService)
        {
            this.userServices = userServices;
            this.auditLogService = auditLogService;
            this.templateService = templateService;
            this.formService = formService;
        }
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        [HttpGet]
        public IActionResult Index()
        {         
            return View();
        }

        [HttpGet("UserManagement")]
        public IActionResult UserManagement()
        {
            var users = userServices.GetAll()
                .OrderByDescending(u => u.CreatedAt)
                .ToList();
            return View(users);
        }

        
        [HttpPost("UserManagement/Block")]
        public JsonResult Block([FromBody] List<int> userIds)
        {
            if (userIds == null || userIds.Count == 0)
                return Json(new { success = false, message = "No users selected." });
            var result = userServices.BlockUsers(userIds);
            if (result)
            {
                auditLogService.RecordLog(int.Parse(HttpContext.Request.Cookies["LoggedId"]), "Blocked Users", string.Join(", ", userIds));
                return Json(new { success = true, message = "Users blocked successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to block users." });
            }
        }

        
        [HttpPost("UserManagement/Unblock")]
        public JsonResult Unblock([FromBody] List<int> userIds)
        {
            if (userIds == null || userIds.Count == 0)
                return Json(new { success = false, message = "No users selected." });
            var result = userServices.UnblockUsers(userIds);
            if (result)
            {
                auditLogService.RecordLog(int.Parse(HttpContext.Request.Cookies["LoggedId"]), "Unblocked Users", string.Join(", ", userIds));
                return Json(new { success = true, message = "Users unblocked successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to unblock users." });
            }
        }

        [HttpPost("UserManagement/Activate")]
        public JsonResult Activate([FromBody] List<int> userIds)
        {
            if (userIds == null || userIds.Count == 0)
                return Json(new { success = false, message = "No users selected." });
            var result = userServices.ActivateUsers(userIds);
            if (result)
            {
                auditLogService.RecordLog(int.Parse(HttpContext.Request.Cookies["LoggedId"]), "Activated Users", string.Join(", ", userIds));
                return Json(new { success = true, message = "Users Activated successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to Activate users." });
            }
        }
        [HttpPost("UserManagement/Inactivate")]
        public JsonResult Inactivate([FromBody] List<int> userIds)
        {
            if (userIds == null || userIds.Count == 0)
                return Json(new { success = false, message = "No users selected." });
            var result = userServices.InactivateUsers(userIds);
            if (result)
            {
                auditLogService.RecordLog(int.Parse(HttpContext.Request.Cookies["LoggedId"]), "Inactivated Users", string.Join(", ", userIds));
                return Json(new { success = true, message = "Users Inactivated successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to Inactivate users." });
            }
        }
            [HttpPost("UserManagement/AddAdminAccess")]
            public JsonResult GiveAdminAccess([FromBody] List<int> userIds)
            {
                if (userIds == null || userIds.Count == 0)
                    return Json(new { success = false, message = "No users selected." });
                var result = userServices.UpdateUserRole(userIds, UserRole.Admin);
                if (result)
                {
                    auditLogService.RecordLog(int.Parse(HttpContext.Request.Cookies["LoggedId"]), "Added Admin Access", string.Join(", ", userIds));
                    return Json(new { success = true, message = "Admin access granted successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to grant admin access." });
                }
            }

        [HttpPost("UserManagement/RemoveAdminAccess")]
        public JsonResult RemoveAdminAccess([FromBody] List<int> userIds)
        {
            if (userIds == null || userIds.Count == 0)
                return Json(new { success = false, message = "No users selected." });
            var result = userServices.UpdateUserRole(userIds, UserRole.User);
            if (result)
            {
                auditLogService.RecordLog(int.Parse(HttpContext.Request.Cookies["LoggedId"]), "Removed Admin Access", string.Join(", ", userIds));
                return Json(new { success = true, message = "Admin access removed successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to remove admin access." });
            }
        }




            [HttpPost("UserManagement/Delete")]
        public JsonResult Delete([FromBody] List<int> userIds)
        {
            if (userIds == null || userIds.Count == 0)
                return Json(new { success = false, message = "No users selected." });
            var result = userServices.DeleteUsers(userIds);
            if (result)
            {
                auditLogService.RecordLog(int.Parse(HttpContext.Request.Cookies["LoggedId"]), "Deleted Users", string.Join(", ", userIds));
                return Json(new { success = true, message = "Users deleted successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to delete users." });
            }
        }
        
        //[HttpGet("UserManagement/Edit/{id}")]
        //public IActionResult Edit(int id)
        //{
        //    var user = userServices.GetById(id);
        //    if (user == null)
        //    {
        //        TempData["ErrorMsg"] = "User not found.";
        //        return RedirectToAction("Dashboard");
        //    }
        //    return View(user);
        //}

        //[HttpPost("UserManagement/Update")]
        //public JsonResult Update([FromBody] UserDTO userDto)
        //{
        //    if (userDto == null || userDto.Id <= 0)
        //        return Json(new { success = false, message = "Invalid user data." });
        //    var result = userServices.UpdateUser(userDto);
        //    if (result)
        //    {
        //        auditLogService.RecordLog(int.Parse(HttpContext.Request.Cookies["LoggedId"]), "Updated User", userDto.Id.ToString());
        //        return Json(new { success = true, message = "User updated successfully." });
        //    }
        //    else
        //    {
        //        return Json(new { success = false, message = "Failed to update user" });
        //    }
        //}
        [AuthenticatedAdmin]
        [HttpGet("AdminDashboard/AllTemplates")]
        public IActionResult AllTemplates()
        {
            var currentUserId = GetCurrentUserId();
            var templates = templateService.GetAllTemplates(currentUserId.Value);
            var templateDTOs = templates.GroupBy(t => t.Id).Select(g => g.First()).ToList();
            var createdByNames = templateDTOs.ToDictionary(t => t.Id, t => t.CreatedBy?.UserName ?? "Unknown");
            // Get comments and likes count
            var commentsDict = templateDTOs.ToDictionary(
                t => t.Id,
                t => t.Comments != null
                    ? t.Comments.Select(c => new Iforms.BLL.DTOs.CommentDTO
                    {
                        Id = c.Id,
                        Content = c.Content,
                        CreatedAt = c.CreatedAt,
                        TemplateId = c.TemplateId,
                        CreatedById = c.CreatedById,
                        CreatedByUserName = c.CreatedBy != null ? c.CreatedBy.UserName : null
                    }).ToList()
                    : new List<Iforms.BLL.DTOs.CommentDTO>()
            );
            var likesDict = templateDTOs.ToDictionary(t => t.Id, t => t.Likes?.Count ?? 0);
            var viewModel = new Iforms.MVC.Models.BrowseTemplatesViewModel
            {
                Templates = templateDTOs,
                CreatedByNames = createdByNames,
                Comments = commentsDict,
                LikesCount = likesDict
            };
            ViewBag.CurrentUserId = currentUserId;
            return View(viewModel);
        }

        [AuthenticatedAdmin]
        [HttpGet("AdminDashboard/AllForms")]
        public IActionResult AllForms(int page = 1)
        {
            var pageSize = 10;
            var allForms = formService.GetAllForms()?.ToList() ?? new List<FormDTO>();
            var totalCount = allForms.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

            var forms = allForms
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Forms = forms;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;

            return View();
        }


    }
}
