using Iforms.BLL.DTOs;
using Iforms.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Iforms.MVC.Controllers
{
    public class QuestionController : Controller
    {
        private readonly QuestionService questionService;
        private readonly TemplateService templateService;

        public QuestionController(QuestionService questionService, TemplateService templateService)
        {
            this.questionService = questionService;
            this.templateService = templateService;
        }

        [HttpGet]
        public IActionResult Create(int templateId)
        {
            var currentUserId = GetCurrentUserId();

            if (!templateService.CanUserManageTemplate(templateId, currentUserId))
                return Forbid();

            return View(new QuestionDTO());
        }

        [HttpPost]
        public IActionResult Create(QuestionDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var currentUserId = GetCurrentUserId();
            questionService.Create(model.TemplateId, model, currentUserId);
            return RedirectToAction("Details", "Template", new { id = model.TemplateId });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var question = questionService.GetById(id);
            if (question == null)
                return NotFound();

            var currentUserId = GetCurrentUserId();
            if (!templateService.CanUserManageTemplate(question.TemplateId, currentUserId))
                return Forbid();

            return View(question);
        }

        [HttpPost]
        public IActionResult Update(QuestionDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var currentUserId = GetCurrentUserId();

            questionService.Update(model.Id, model, currentUserId);
            return RedirectToAction("Details", "Template", new { id = model.TemplateId });
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var question = questionService.GetById(id);
            if (question == null)
                return NotFound();

            var currentUserId = GetCurrentUserId();
            questionService.Delete(id, currentUserId);

            return RedirectToAction("Details", "Template", new { id = question.TemplateId });
        }

        [HttpPost]
        public IActionResult Reorder([FromBody] TemplateExtendedDTO reorderDto)
        {
            var currentUserId = GetCurrentUserId();
            questionService.ReorderQuestions(reorderDto, currentUserId);

            return Ok();
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }
    }
}
