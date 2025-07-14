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
        private readonly SalesforceService salesforceService;
        private readonly UserService userService;

        public UserDashboardController(TemplateService templateService, TagService tagService, FormService formService, AuditLogService auditLogService, SalesforceService salesforceService, UserService userService)
        {
            this.templateService = templateService;
            this.tagService = tagService;
            this.formService = formService;
            this.auditLogService = auditLogService;
            this.salesforceService = salesforceService;
            this.userService = userService;
        }

        [AuthenticatedAdminorUser]
        [HttpGet]
        public IActionResult UserTemplates(int page = 1)
        {
            var templateIdstring = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(templateIdstring) || !int.TryParse(templateIdstring, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var pageSize = 10;
            var allTemplates = templateService.GetUserTemplates(userId, userId).ToList();
            var totalCount = allTemplates.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
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
                return RedirectToAction("Login", "Auth");
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
        public IActionResult TemplateFormResponses(int templateId)
        {
            var templateIdstring = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(templateIdstring) || !int.TryParse(templateIdstring, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!templateService.CanUserManageTemplate(templateId, userId))
            {
                return Forbid();
            }

            var template = templateService.GetTemplateDetailedById(templateId, userId);
            if (template == null)
            {
                return NotFound();
            }

            var forms = formService.GetTemplateForms(templateId, userId);
            ViewBag.Template = template;
            ViewBag.Forms = forms;
            return View();
        }

        [AuthenticatedAdminorUser  ]
        [HttpGet]
        public IActionResult UserSubmittedForms()
        {
            var templateIdstring = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(templateIdstring) || !int.TryParse(templateIdstring, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var forms = formService.GetUserForms(userId);
            ViewBag.Forms = forms;
            return View();
        }

        [AuthenticatedAdminorUser]
        [HttpGet]
        public IActionResult SyncToSalesforce()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return RedirectToAction("Login", "Auth");

            var user = userService.GetById(userId);
            var accountModel = new SalesforceAccountViewModel();
            var contactModel = new SalesforceContactViewModel
            {
                ContactFirstName = user?.UserName,
                ContactEmail = user?.UserEmail
            };
            return View((accountModel, contactModel));
        }

        [AuthenticatedAdminorUser]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSalesforceAccount(SalesforceAccountViewModel model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return RedirectToAction("Login", "Auth");

            var contactModel = new SalesforceContactViewModel();
            if (!ModelState.IsValid)
                return View("SyncToSalesforce", (model, contactModel));

            try
            {
                var token = await salesforceService.AuthenticateAsync();
                var accountDto = new SalesforceAccountDTO
                {
                    Name = model.AccountName,
                    Phone = model.AccountPhone,
                    Website = model.AccountWebsite
                };
                var accountId = await salesforceService.CreateAccountAsync(accountDto, token);
                model.ResultMessage = $"Successfully created Salesforce account";
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message;
                if (ex.InnerException != null)
                    errorMsg += " | Inner: " + ex.InnerException.Message;
                model.ResultMessage = $"Error creating Salesforce account: {errorMsg}";
            }
            return View("SyncToSalesforce", (model, contactModel));
        }

        [AuthenticatedAdminorUser]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSalesforceContact(SalesforceContactViewModel model)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return RedirectToAction("Login", "Auth");

            var accountModel = new SalesforceAccountViewModel();
            if (!ModelState.IsValid)
                return View("SyncToSalesforce", (accountModel, model));

            try
            {
                var token = await salesforceService.AuthenticateAsync();
                var contactDto = new SalesforceContactDTO
                {
                    FirstName = model.ContactFirstName,
                    LastName = model.ContactLastName,
                    Email = model.ContactEmail,
                    AccountId = null
                };
                var contactId = await salesforceService.CreateContactAsync(contactDto, token);
                model.ResultMessage = $"Successfully created Salesforce contact";
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message;
                if (ex.InnerException != null)
                    errorMsg += " | Inner: " + ex.InnerException.Message;
                model.ResultMessage = $"Error creating Salesforce contact: {errorMsg}";
            }
            return View("SyncToSalesforce", (accountModel, model));
        }
    }
}
