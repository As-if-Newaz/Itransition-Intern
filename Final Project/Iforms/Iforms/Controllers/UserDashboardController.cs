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
        private readonly FormService formService;
        private readonly AuditLogService auditLogService;

        public UserDashboardController(TemplateService templateService, TagService tagService, FormService formService, AuditLogService auditLogService)
        {
            this.templateService = templateService;
            this.tagService = tagService;
            this.formService = formService;
            this.auditLogService = auditLogService;
        }

        [AuthenticatedAdminorUser]
        [HttpGet]
        public IActionResult UserTemplates(int page = 1)
        {
            var templateIdstring = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(templateIdstring) || !int.TryParse(templateIdstring, out var userId))
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

        [AuthenticatedAdminorUser]
        [HttpGet]
        public IActionResult UserTemplateResponses(int page = 1)
        {
            var templateIdstring = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(templateIdstring) || !int.TryParse(templateIdstring, out var userId))
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

        [AuthenticatedAdminorUser]
        [HttpGet]
        public IActionResult TemplateFormResponses(int templateId, int page = 1)
        {
            var templateIdstring = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(templateIdstring) || !int.TryParse(templateIdstring, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if user can manage this template
            if (!templateService.CanUserManageTemplate(templateId, userId))
            {
                return Forbid();
            }

            var template = templateService.GetTemplateDetailedById(templateId, userId);
            if (template == null)
            {
                return NotFound();
            }

            var pageSize = 10;
            var allForms = formService.GetTemplateForms(templateId, userId);
            var totalCount = allForms.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            // Ensure page is within valid range
            page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));
            
            var forms = allForms
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Template = template;
            ViewBag.Forms = forms;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;
            ViewBag.HasPreviousPage = page > 1;
            ViewBag.HasNextPage = page < totalPages;

            return View();
        }

        [AuthenticatedAdminorUser  ]
        [HttpGet]
        public IActionResult UserSubmittedForms(int page = 1)
        {
            var templateIdstring = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(templateIdstring) || !int.TryParse(templateIdstring, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var pageSize = 10;
            var allForms = formService.GetUserForms(userId);
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
