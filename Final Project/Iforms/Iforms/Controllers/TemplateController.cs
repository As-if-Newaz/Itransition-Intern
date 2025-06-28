using Iforms.BLL.DTOs;
using Iforms.BLL.Services;
using Iforms.MVC.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;

namespace Iforms.MVC.Controllers
{
    public class TemplateController : Controller
    {
        private readonly TemplateService templateService;
        private readonly QuestionService questionService;
        private readonly CommentService commentService;
        private readonly UserService userService;


        public TemplateController(
            TemplateService templateService,
            QuestionService questionService,
            CommentService commentService,
            UserService userService)
        {
            this.templateService = templateService;
            this.questionService = questionService;
            this.commentService = commentService;
            this.userService = userService;
        }

        public IActionResult Details(int id)
        {
            var currentUserId = GetCurrentUserId();
            var template = templateService.GetTemplateDetailedById(id, currentUserId);

            if (template == null)
                return NotFound();

            if (!templateService.CanUserAccessTemplate(id, currentUserId))
                return Forbid();

            var questions = questionService.GetByTemplateId(id);
            var comments = commentService.GetTemplateComments(id);
            return View(template);
        }

        [AuthenticatedUser]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new TemplateExtendedDTO());
        }

        [AuthenticatedUser]
        [HttpPost]
        public IActionResult Create(TemplateExtendedDTO model)
        {
            // Debug information
            Console.WriteLine($"Title: {model.Title}");
            Console.WriteLine($"Description: {model.Description}");
            Console.WriteLine($"ImageUrl: {model.ImageUrl}");
            Console.WriteLine($"IsPublic: {model.IsPublic}");
            Console.WriteLine($"Topic: {model.Topic?.Id} - {model.Topic?.TopicType}");
            Console.WriteLine($"Questions Count: {model.Questions?.Count ?? 0}");
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine($"Model validation errors: {string.Join(", ", errors)}");
                return View(model);
            }

            var currentUserId = GetCurrentUserId()!.Value;
            
            // Set required fields
            model.CreatedAt = DateTime.UtcNow;
            model.CreatedById = currentUserId;

            try
            {
                var template = templateService.Create(model, currentUserId);
                if (template != null)
                {
                    return RedirectToAction("Details", new { id = template.Id });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating template: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while creating the template. Please try again.");
                return View(model);
            }
            
            return RedirectToAction("Index", "UserDashboard");
        }

        [AuthenticatedUser]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var currentUserId = GetCurrentUserId()!.Value;

            if (!templateService.CanUserManageTemplate(id, currentUserId))
                return Forbid();

            var template = templateService.GetTemplateDetailedById(id, currentUserId);
            if (template == null)
                return NotFound();

            return View(template);
        }

        [AuthenticatedUser]
        [HttpPost]
        public IActionResult Edit(TemplateExtendedDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var currentUserId = GetCurrentUserId()!.Value;

            var template = templateService.Update(model.Id, model, currentUserId);
            if (template == null)
                return NotFound();
            return RedirectToAction("Details", new { id = model.Id });
        }

        [AuthenticatedUser]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            var currentUserId = GetCurrentUserId()!.Value;
            var success = templateService.Delete(id, currentUserId);

            if (!success)
                return NotFound();

            return RedirectToAction("Index", "UserDashboard");
        }

        [AuthenticatedUser]
        [HttpPost]
        public IActionResult ToggleLike(int id)
        {
            var currentUserId = GetCurrentUserId()!.Value;
            var isLiked = templateService.ToggleLike(id, currentUserId);

            return Json(new { isLiked });
        }

        [AuthenticatedUser]
        [HttpPost]
        public IActionResult AddComment(int templateId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return BadRequest();

            var currentUserId = GetCurrentUserId()!.Value;
            var createDto = new CommentDTO
            {
                TemplateId = templateId,
                Content = content
            };

            var comment = commentService.Create(createDto);
            return Json(comment);
        }

        [AuthenticatedUser]
        //public IActionResult Results(int id)
        //{
        //    var currentUserId = GetCurrentUserId()!.Value;

        //    if (!templateService.CanUserManageTemplate(id, currentUserId))
        //        return Forbid();

        //    var results = formService.GetTemplateResults(id, currentUserId);
        //    var template = templateService.GetById(id, currentUserId);

        //    var model = new TemplateResultsViewModel
        //    {
        //        Template = template!,
        //        Results = results
        //    };

        //    return View(model);
        //}

        [HttpGet]
        public IActionResult SearchUsers(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var users = userService.SearchUsers(term);
            return Json(users.Select(u => new { id = u.Id, name = u.UserName, email = u.UserEmail }));
        }

        [HttpGet]
        public IActionResult SearchTopics(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var topics = templateService.SearchTopics(term);
            return Json(topics.Select(t => new { id = t.Id, topicType = t.TopicType }));
        }

        [HttpPost]
        public IActionResult CreateTopic([FromBody] TopicDTO topicDto)
        {
            if (string.IsNullOrWhiteSpace(topicDto.TopicType))
                return BadRequest("Topic type is required");

            var success = templateService.AddNewTopic(topicDto);
            if (success)
            {
                return Json(new { success = true, message = "Topic created successfully" });
            }
            
            return BadRequest("Failed to create topic");
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
