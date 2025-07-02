using Iforms.BLL.DTOs;
using Iforms.BLL.Services;
using Iforms.DAL.Entity_Framework.Table_Models;
using Iforms.MVC.Authentication;
using Iforms.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Iforms.MVC.Controllers
{
    public class FormController : Controller
    {
        private readonly FormService formService;
        private readonly TemplateService templateService;
        private readonly QuestionService questionService;
        private readonly ImageService imageService;
        private readonly AuditLogService auditLogService;

        public FormController(
            FormService formService,
            TemplateService templateService,
            QuestionService questionService,
            ImageService imageService,
            AuditLogService auditLogService)
        {
            this.formService = formService;
            this.templateService = templateService;
            this.questionService = questionService;
            this.imageService = imageService;
            this.auditLogService = auditLogService;
        }
        [HttpGet]
        public IActionResult DebugTemplates()
        {
            try
            {
                // Get all templates
                var allTemplates = templateService.DA.TemplateData().GetAll();
                var publicTemplates = templateService.DA.TemplateData().GetPublicTemplates();
                
                var result = new
                {
                    TotalTemplates = allTemplates.Count(),
                    PublicTemplates = publicTemplates.Count(),
                    TemplateList = allTemplates.Select(t => new
                    {
                        Id = t.Id,
                        Title = t.Title,
                        IsPublic = t.IsPublic,
                        CreatedById = t.CreatedById
                    }).ToList()
                };
                
                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message, StackTrace = ex.StackTrace });
            }
        }

        [AuthenticatedAdminorUser]
        [HttpGet("Form/Fill/{templateId}")]
        public IActionResult Fill(int templateId)
        {
            var currentUserId = GetCurrentUserId();
            if (!templateService.CanUserAccessTemplate(templateId, currentUserId))
            {
                return Forbid();
            }
            var template = templateService.GetTemplateDetailedById(templateId, currentUserId);
            if (template == null)
            {
                return NotFound();
            }
            var questions = questionService.GetByTemplateId(templateId);
            var model = new FillFormModel
            {
                Template = template,
                Questions = questions.ToList()
            };
            return View(model);
        }

        [AuthenticatedAdminorUser]
        [HttpPost("Form/Submit")]
        public IActionResult Submit(FillFormModel model)
        {
            var currentUserId = GetAuthenticatedUserId();
            if (model.Template == null)
            {
                return BadRequest("Template is null");
            }
            if (!templateService.CanUserAccessTemplate(model.Template.Id, currentUserId))
            {
                return Forbid();
            }
            if (model.Answers == null || !model.Answers.Any())
            {
                return BadRequest("No answers provided");
            }
            // Get the list of questions for the template
            var questions = questionService.GetByTemplateId(model.Template.Id).ToList();
            // Handle file uploads for FileUpload questions
            for (int i = 0; i < model.Answers.Count; i++)
            {
                var answer = model.Answers[i];
                var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                if (question != null && question.QuestionType.ToString() == "FileUpload")
                {
                    // The file input name is Answers[i].FileUrl
                    var file = Request.Form.Files[$"Answers[{i}].FileUrl"];
                    if (file != null && file.Length > 0)
                    {
                        var url = ImageService.UploadAnswerImage(file);
                        answer.FileUrl = url;
                    }
                }
            }
            var createDto = new FormDTO
            {
                TemplateId = model.Template.Id,
                Answers = model.Answers.Select(a => new AnswerDTO
                {
                    QuestionId = a.QuestionId,
                    Text = a.Text,
                    Number = a.Number,
                    SignleChoice = a.SignleChoice,
                    FileUrl = a.FileUrl,
                    Date = a.Date,
                }).ToList()
            };
            var form = formService.Create(createDto, currentUserId);
            return RedirectToAction("Details", new { id = form.Id });
        }

        [AuthenticatedAdminorUser]
        public IActionResult Details(int id)
        {
            var currentUserId = GetAuthenticatedUserId();
            var form = formService.GetById(id, currentUserId);

            if (form == null)
                return NotFound();

            return View(form);
        }

        [AuthenticatedAdminorUser]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(FillFormModel model)
        {
            var currentUserId = GetAuthenticatedUserId();
            if (model.Template == null || model.Answers == null)
                return BadRequest();

            // Get the list of questions for the template
            var questions = questionService.GetByTemplateId(model.Template.Id).ToList();

            // Handle file uploads for FileUpload questions
            for (int i = 0; i < model.Answers.Count; i++)
            {
                var answer = model.Answers[i];
                var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                if (question != null && question.QuestionType.ToString() == "FileUpload")
                {
                    var file = Request.Form.Files[$"Answers[{i}].FileUrl"];
                    if (file != null && file.Length > 0)
                    {
                        var url = ImageService.UploadAnswerImage(file);
                        answer.FileUrl = url;
                    }
                }
            }

            var formDto = new FormDTO
            {
                Id = model.FormId,
                TemplateId = model.Template.Id,
                Answers = model.Answers
            };

            var success = formService.Update(formDto, currentUserId);
            if (!success)
                return Forbid();
            return RedirectToAction("Details", new { id = model.FormId });
        }

        [AuthenticatedAdminorUser]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var currentUserId = GetAuthenticatedUserId();
            var form = formService.GetById(id, currentUserId);
            if (form == null)
                return NotFound();

            var model = new FillFormModel
            {
                FormId = form.Id,
                Template = form.Template,
                Answers = form.Answers ?? new List<AnswerDTO>(),
                Questions = form.Template?.Questions ?? new List<QuestionDTO>()
            };
            return View(model);
        }

        [AuthenticatedAdminorUser]
        [HttpPost]
        public IActionResult UploadAnswerImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest(new { success = false, message = "No image uploaded." });
            try
            {
                var url = ImageService.UploadAnswerImage(image);
                if (string.IsNullOrEmpty(url))
                    return StatusCode(500, new { success = false, message = "Image upload failed." });
                return Json(new { success = true, imageUrl = url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [AuthenticatedAdminorUser]
        [HttpPost]
        public IActionResult DeleteAnswerImage(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized();

            // Find the answer by id (use questionService or formService as appropriate)
            var answer = formService.GetAnswerById(id);
            if (answer == null)
                return NotFound(new { success = false, message = "Answer not found." });

            // Check if the user can manage the form this answer belongs to
            var form = formService.GetAnswerById(answer.FormId);
            if (form == null || !formService.CanUserManageForm(form.Id, currentUserId.Value))
                return Forbid();

            var imageUrl = answer.FileUrl;
            if (string.IsNullOrEmpty(imageUrl))
                return BadRequest(new { success = false, message = "No image URL to delete." });

            var deleteSuccess = ImageService.DeleteImage(imageUrl);
            if (!deleteSuccess)
            {
                return Json(new { success = false, message = "Failed to delete image from storage. Please try again." });
            }

            // Optionally clear the FileUrl in the answer (if you want to persist this change)
            answer.FileUrl = null;
            formService.UpdateAnswer(answer);

            return Json(new { success = true });
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("User is not authenticated");
            return userId;
        }

        [AuthenticatedAdminorUser]
        [HttpPost("Form/DeleteForms")]
        public JsonResult DeleteForms([FromBody] int[] formIds)
        {
            if (formIds == null || formIds.Length == 0)
                return Json(new { success = false, message = "No forms selected." });
            var result = formService.DeleteForms(formIds);
            if (result)
            {
                auditLogService.RecordLog(int.Parse(HttpContext.Request.Cookies["LoggedId"]), "Deleted Forms", string.Join(", ", formIds));
                return Json(new { success = true, message = "Forms deleted successfully." });
            }
            else
            {
                return Json(new { success = false, message = "Failed to delete Forms" });
            }
        }
    }
}
