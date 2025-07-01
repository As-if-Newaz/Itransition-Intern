using Iforms.BLL.DTOs;
using Iforms.BLL.Services;
using Iforms.MVC.Authentication;
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


        public AdminDashboardController(UserService userServices, AuditLogService auditLogService, TemplateService templateService)
        {
            this.userServices = userServices;
            this.auditLogService = auditLogService;
            this.templateService = templateService;
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
        public JsonResult Block([FromBody] int[] userIds)
        {
            if (userIds == null || userIds.Length == 0)
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
        public JsonResult Unblock([FromBody] int[] userIds)
        {
            if (userIds == null || userIds.Length == 0)
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
        public JsonResult Activate([FromBody] int[] userIds)
        {
            if (userIds == null || userIds.Length == 0)
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
        public JsonResult Inactivate([FromBody] int[] userIds)
        {
            if (userIds == null || userIds.Length == 0)
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


        [HttpPost("UserManagement/Delete")]
        public JsonResult Delete([FromBody] int[] userIds)
        {
            if (userIds == null || userIds.Length == 0)
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
        
        [HttpGet("UserManagement/Edit/{id}")]
        public IActionResult Edit(int id)
        {
            var user = userServices.GetById(id);
            if (user == null)
            {
                TempData["ErrorMsg"] = "User not found.";
                return RedirectToAction("Dashboard");
            }
            return View(user);
        }

        [HttpPost("UserManagement/Update")]
        public JsonResult Update([FromBody] UserDTO userDto)
        {
            if (userDto == null || userDto.Id <= 0)
                return Json(new { success = false, message = "Invalid user data." });
            var result = userServices.UpdateUser(userDto);
            if (result)
            {
                auditLogService.RecordLog(int.Parse(HttpContext.Request.Cookies["LoggedId"]), "Updated User", userDto.Id.ToString());
                return Json(new { success = true, message = "User updated successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to update user" });
            }
        }

        [HttpPost("UserManagement/UpdateRole")]
        public JsonResult UpdateRole(int userId, UserRole role)
        {
            var result = userServices.UpdateUserRole(userId, role);
            if (result)
            {
                return Json(new { success = true, message = "User role updated successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to update user role." });
            }
        }
        [AuthenticatedAdmin]
        [HttpGet("AdminDashboard/AllTemplates")]
        public IActionResult AllTemplates()
        {
            var currentUserId = GetCurrentUserId();
            var templates = templateService.GetAllTemplates();

            // Debug output
            Console.WriteLine($"[BrowseTemplates] CurrentUserId: {currentUserId}");
            foreach (var t in allTemplates)
            {
                Console.WriteLine($"[BrowseTemplates] TemplateId: {t.Id}, Title: {t.Title}, IsPublic: {t.IsPublic}, CreatedById: {t.CreatedById}");
            }

            var templateDTOs = allTemplates.Select(t => new Iforms.BLL.DTOs.TemplateDTO
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                IsPublic = t.IsPublic,
                CreatedAt = t.CreatedAt,
                CreatedById = t.CreatedById,
                IsLikedByCurrentUser = t.Likes != null && currentUserId.HasValue && t.Likes.Any(l => l.UserId == currentUserId.Value)
            }).ToList();
            // Get CreatedBy names
            var createdByNames = allTemplates.ToDictionary(t => t.Id, t => t.CreatedBy?.UserName ?? "Unknown");
            // Get comments and likes count
            var commentsDict = allTemplates.ToDictionary(
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
            var likesDict = allTemplates.ToDictionary(t => t.Id, t => t.Likes?.Count ?? 0);
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
        [HttpPost("AdminDashboard/DeletefromAllTemplates")]
        public JsonResult DeletefromAllTemplates([FromBody] int[] templateIds)
        {
            if (templateIds == null || templateIds.Length == 0)
                return Json(new { success = false, message = "No Templates selected." });
            var result = templateService.DeleteTemplates(templateIds);
            if (result)
            {
                auditLogService.RecordLog(int.Parse(HttpContext.Request.Cookies["LoggedId"]), "Deleted Templates", string.Join(", ", templateIds));
                return Json(new { success = true, message = "Templates successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to delete Templates" });
            }
        }

        [AuthenticatedAdmin]
        [HttpGet("AdminDashboard/AllForms")]
        public IActionResult AllForms()
        {
            var forms = formService.GetAllForms();
            return View(forms);
        }


    }
}
