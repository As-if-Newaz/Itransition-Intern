using Iforms.BLL.Services;
using Iforms.BLL.DTOs;
using Iforms.MVC.Authentication;
using Iforms.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Iforms.MVC.Controllers
{
    public class UserDashboardController : Controller
    {
        private readonly TemplateService templateService;
        private readonly TagService tagService;
        public UserDashboardController(TemplateService templateService, TagService tagService)
        {
            this.templateService = templateService;
            this.tagService = tagService;
        }

        [AuthenticatedUser]
        [HttpGet]
        public IActionResult UserTemplates(int page = 1)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var pageSize = 10;
            var allTemplates = templateService.GetUserTemplates(userId, userId).ToList();
            var totalCount = allTemplates.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));
            
            var templates = allTemplates
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new UserTemplatesViewModel
            {
                Templates = templates,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };

            return View(viewModel);
        }
    }
}
