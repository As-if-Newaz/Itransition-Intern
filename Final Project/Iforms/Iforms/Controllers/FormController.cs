using Iforms.BLL.DTOs;
using Iforms.BLL.Services;
using Iforms.DAL.Entity_Framework.Table_Models;
using Iforms.MVC.Authentication;
using Iforms.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Server;
using System.Linq;
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
                FormId = 0, 
                Template = template,
                Questions = questions.ToList(),
                Answers = questions.Select(q => new AnswerDTO
                {
                    QuestionId = q.Id,
                    FormId = 0 
                }).ToList()
            };
            return View(model);
        }

        [AuthenticatedAdminorUser]
        [HttpPost("Form/Submit")]
        public IActionResult Submit(FillFormModel model)
        {
            var validationResult = ValidateFormSubmission(model);
            if (validationResult != null)
                return validationResult;

            var currentUserId = GetAuthenticatedUserId();
            ProcessFileUploads(model);
            FilterEmptyAnswers(model);

            try
            {
                var formDto = CreateFormDto(model);
                var form = formService.Create(formDto, currentUserId);
                return RedirectToAction("Details", new { id = form.Id });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private IActionResult? ValidateFormSubmission(FillFormModel model)
        {
            if (model.Template == null)
                return BadRequest("Template is null");

            if (model.Answers == null || !model.Answers.Any())
                return BadRequest("No answers provided");

            var currentUserId = GetAuthenticatedUserId();
            if (!templateService.CanUserAccessTemplate(model.Template.Id, currentUserId))
                return Forbid();

            return null;
        }

        private void FilterEmptyAnswers(FillFormModel model)
        {
            if (model.Answers == null) return;

            var questions = questionService.GetByTemplateId(model.Template.Id).ToList();
            model.Answers = model.Answers
                .Where(answer => IsAnswerNotEmpty(answer, questions))
                .ToList();
        }

        private bool IsAnswerNotEmpty(AnswerDTO answer, List<QuestionDTO> questions)
        {
            var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
            if (question == null) return false;

            return question.QuestionType == Enums.QuestionType.FileUpload || 
                   !string.IsNullOrWhiteSpace(answer.Text) ||
                   !string.IsNullOrWhiteSpace(answer.LongText) ||
                   answer.Number.HasValue ||
                   answer.Checkbox.HasValue ||
                   !string.IsNullOrWhiteSpace(answer.FileUrl) ||
                   answer.Date.HasValue;
        }

        private void ProcessFileUploads(FillFormModel model)
        {
            var questions = questionService.GetByTemplateId(model.Template.Id).ToList();
            
            for (int i = 0; i < model.Answers.Count; i++)
            {
                var answer = model.Answers[i];
                var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                
                if (question?.QuestionType == Enums.QuestionType.FileUpload)
                {
                    ProcessFileUpload(answer, i);
                }
            }
        }

        private void ProcessFileUpload(AnswerDTO answer, int answerIndex)
        {
            var file = Request.Form.Files[$"AnswerFile_{answerIndex}"];
            if (file != null && file.Length > 0)
            {
                var url = ImageService.UploadAnswerImage(file);
                answer.FileUrl = url;
            }
        }

        private FormDTO CreateFormDto(FillFormModel model)
        {
            return new FormDTO
            {
                TemplateId = model.Template.Id,
                Answers = model.Answers.Select(answer => new AnswerDTO
                {
                    QuestionId = answer.QuestionId,
                    Text = answer.Text,
                    LongText = answer.LongText,
                    Number = answer.Number,
                    Checkbox = answer.Checkbox,
                    FileUrl = answer.FileUrl,
                    Date = answer.Date,
                }).ToList()
            };
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
            var validationResult = ValidateFormSubmission(model);
            if (validationResult != null)
                return validationResult;

            var currentUserId = GetAuthenticatedUserId();

            ProcessEditFileUploads(model);

            try
            {
                var formDto = CreateEditFormDto(model);
                var success = formService.Update(formDto, currentUserId);
                
                if (!success)
                    return Forbid();
                    
                return RedirectToAction("Details", new { id = model.FormId });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void ProcessEditFileUploads(FillFormModel model)
        {
            var questions = questionService.GetByTemplateId(model.Template.Id).ToList();

            for (int i = 0; i < model.Answers.Count; i++)
            {
                var answer = model.Answers[i];
                var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                
                if (question?.QuestionType == Enums.QuestionType.FileUpload)
                {
                    ProcessEditFileUpload(answer, i);
                }
            }
        }

        private void ProcessEditFileUpload(AnswerDTO answer, int answerIndex)
        {
            // Handle file deletion if requested
            if (answer.RemoveFile && !string.IsNullOrEmpty(answer.FileUrl))
            {
                ImageService.DeleteImage(answer.FileUrl);
                answer.FileUrl = null;
            }

            // Handle new file upload using existing method
            ProcessFileUpload(answer, answerIndex);
        }

        private FormDTO CreateEditFormDto(FillFormModel model)
        {
            var formDto = CreateFormDto(model);
            formDto.Id = model.FormId; // Add the form ID for editing
            return formDto;
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
            var answer = formService.GetAnswerById(id);
            if (answer == null)
                return NotFound(new { success = false, message = "Answer not found." });

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
        public JsonResult DeleteForms([FromBody] List<int> formIds)
        {
            if (formIds == null || formIds.Count == 0)
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
