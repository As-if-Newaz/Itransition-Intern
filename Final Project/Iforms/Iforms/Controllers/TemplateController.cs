using Iforms.BLL.DTOs;
using Iforms.BLL.Services;
using Iforms.MVC.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;
using Microsoft.AspNetCore.SignalR;

namespace Iforms.MVC.Controllers
{
    public class TemplateController : Controller
    {
        private readonly TemplateService templateService;
        private readonly QuestionService questionService;
        private readonly CommentService commentService;
        private readonly UserService userService;
        private readonly IHubContext<TemplateHub> hubContext;


        public TemplateController(
            TemplateService templateService,
            QuestionService questionService,
            CommentService commentService,
            UserService userService,
            IHubContext<TemplateHub> hubContext)
        {
            this.templateService = templateService;
            this.questionService = questionService;
            this.commentService = commentService;
            this.userService = userService;
            this.hubContext = hubContext;
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
            Console.WriteLine($"Template Tags Count: {model.TemplateTags?.Count ?? 0}");
            if (model.TemplateTags?.Any() == true)
            {
                Console.WriteLine($"Template Tags: {string.Join(", ", model.TemplateTags.Select(t => t.Name))}");
            }
            
            // Process template tags from form input if needed
            var templateTagsInput = Request.Form["TemplateTagsInput"].ToString();
            if (!string.IsNullOrWhiteSpace(templateTagsInput) && (model.TemplateTags == null || !model.TemplateTags.Any()))
            {
                var tagNames = templateTagsInput.Split(',')
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();
                
                model.TemplateTags = tagNames.Select(tagName => new TagDTO { Name = tagName }).ToList();
                Console.WriteLine($"Processed template tags from input: {string.Join(", ", tagNames)}");
            }
            
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

            // Debug logging for questions and options
            if (template.Questions != null && template.Questions.Any())
            {
                Console.WriteLine($"Edit: Template {id} has {template.Questions.Count} questions");
                foreach (var question in template.Questions)
                {
                    Console.WriteLine($"Question {question.Id}: {question.QuestionTitle}, Type: {question.QuestionType}");
                    if (question.Options != null && question.Options.Any())
                    {
                        Console.WriteLine($"  Options: {string.Join(", ", question.Options)}");
                    }
                    else
                    {
                        Console.WriteLine($"  No options found");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Edit: Template {id} has no questions");
            }

            // Debug logging for shared users
            Console.WriteLine($"TemplateAccesses count: {template.TemplateAccesses?.Count}");
            if (template.TemplateAccesses != null)
            {
                foreach (var user in template.TemplateAccesses)
                {
                    Console.WriteLine($"User: {user.Id}, {user.UserName}, {user.UserEmail}");
                }
            }

            return View(template);
        }

        [AuthenticatedUser]
        [HttpPost]
        public IActionResult Edit(TemplateExtendedDTO model)
        {
            // Debug information
            Console.WriteLine($"Title: {model.Title}");
            Console.WriteLine($"Description: {model.Description}");
            Console.WriteLine($"ImageUrl: {model.ImageUrl}");
            Console.WriteLine($"IsPublic: {model.IsPublic}");
            Console.WriteLine($"Topic: {model.Topic?.Id} - {model.Topic?.TopicType}");
            Console.WriteLine($"Questions Count: {model.Questions?.Count ?? 0}");
            Console.WriteLine($"Template Tags Count: {model.TemplateTags?.Count ?? 0}");
            if (model.TemplateTags?.Any() == true)
            {
                Console.WriteLine($"Template Tags: {string.Join(", ", model.TemplateTags.Select(t => t.Name))}");
            }
            
            // Process template tags from form input if needed
            var templateTagsInput = Request.Form["TemplateTagsInput"].ToString();
            if (!string.IsNullOrWhiteSpace(templateTagsInput) && (model.TemplateTags == null || !model.TemplateTags.Any()))
            {
                var tagNames = templateTagsInput.Split(',')
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();
                
                model.TemplateTags = tagNames.Select(tagName => new TagDTO { Name = tagName }).ToList();
                Console.WriteLine($"Processed template tags from input: {string.Join(", ", tagNames)}");
            }
            
            // Process selected user IDs from form input
            var selectedUserIdsInput = Request.Form["SelectedUserIds"].ToString();
            if (!string.IsNullOrWhiteSpace(selectedUserIdsInput))
            {
                var userIds = selectedUserIdsInput.Split(',')
                    .Select(id => id.Trim())
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(int.Parse)
                    .ToList();
                
                // Create minimal UserDTO objects with only the Id property
                model.TemplateAccesses = userIds.Select(userId => new UserDTO 
                { 
                    Id = userId,
                    UserName = "", // Will be ignored by the service
                    UserEmail = "", // Will be ignored by the service
                    PasswordHash = "", // Will be ignored by the service
                    UserRole = UserRole.User, // Default value
                    UserStatus = UserStatus.Active, // Default value
                    CreatedAt = DateTime.UtcNow // Default value
                }).ToList();
                Console.WriteLine($"Processed selected user IDs: {string.Join(", ", userIds)}");
            }
            else
            {
                model.TemplateAccesses = new List<UserDTO>();
            }
            
            // Process questions from form input
            var questions = new List<QuestionDTO>();
            var questionIndex = 0;
            while (Request.Form.ContainsKey($"Questions[{questionIndex}].QuestionTitle"))
            {
                var questionIdStr = Request.Form[$"Questions[{questionIndex}].Id"].ToString();
                var questionTitle = Request.Form[$"Questions[{questionIndex}].QuestionTitle"].ToString();
                var questionTypeStr = Request.Form[$"Questions[{questionIndex}].QuestionType"].ToString();
                var questionOrder = int.Parse(Request.Form[$"Questions[{questionIndex}].QuestionOrder"].ToString());
                var optionsJson = Request.Form[$"Questions[{questionIndex}].Options"].ToString();
                
                // Parse question type string to enum
                if (Enum.TryParse<Iforms.DAL.Entity_Framework.Table_Models.Enums.QuestionType>(questionTypeStr, out var questionType))
                {
                    var questionDto = new QuestionDTO
                    {
                        Id = int.TryParse(questionIdStr, out var id) ? id : 0,
                        QuestionTitle = questionTitle,
                        QuestionDescription = questionTitle, // Using title as description for now
                        QuestionType = questionType,
                        QuestionOrder = questionOrder,
                        TemplateId = model.Id,
                        Options = !string.IsNullOrEmpty(optionsJson) ? 
                            System.Text.Json.JsonSerializer.Deserialize<List<string>>(optionsJson) ?? new List<string>() : 
                            new List<string>()
                    };
                    questions.Add(questionDto);
                }
                questionIndex++;
            }
            model.Questions = questions;
            Console.WriteLine($"Processed {questions.Count} questions");
            
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine($"Model validation errors: {string.Join(", ", errors)}");
                return View(model);
            }

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
        public async Task<IActionResult> ToggleLike(int id)
        {
            var currentUserId = GetCurrentUserId()!.Value;
            var isLiked = templateService.ToggleLike(id, currentUserId);

            // Get updated likes count
            var template = templateService.GetTemplateDetailedById(id, currentUserId);
            var likesCount = template?.Likes?.Count ?? 0;

            // Broadcast the like update to all clients
            await hubContext.Clients.All.SendAsync("ReceiveLike", id, likesCount, isLiked, currentUserId);

            return Json(new { isLiked });
        }

        [AuthenticatedUser]
        [HttpPost]
        public async Task<IActionResult> AddComment(int templateId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return BadRequest();

            var currentUserId = GetCurrentUserId()!.Value;
            var createDto = new CommentDTO
            {
                TemplateId = templateId,
                Content = content,
                CreatedById = currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            var commentCreated = commentService.Create(createDto);

            // Fetch the comment with user info from DB and return as DTO
            var commentWithUser = commentService.GetTemplateComments(templateId)
                .LastOrDefault(c => c.CreatedById == currentUserId && c.Content == content);

            // Ensure CreatedByUserName is set
            if (commentWithUser != null && string.IsNullOrEmpty(commentWithUser.CreatedByUserName))
            {
                var user = userService.GetById(currentUserId);
                commentWithUser.CreatedByUserName = user?.UserName ?? "User";
            }

            // Broadcast the new comment to all clients
            await hubContext.Clients.All.SendAsync("ReceiveComment", templateId, commentWithUser);

            return Json(commentWithUser);
        }

        [AuthenticatedUser]
        [HttpPost]
        public IActionResult DeleteComment(int id)
        {
            var currentUserId = GetCurrentUserId()!.Value;
            var success = commentService.Delete(id, currentUserId);
            if (!success)
                return Forbid();
            return Ok();
        }

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

        [HttpGet]
        public IActionResult GetAllTopics()
        {
            var topics = templateService.GetAllTopics();
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

        [HttpGet]
        public IActionResult BrowseTemplates()
        {
            var currentUserId = GetCurrentUserId();
            var accessibleTemplates = currentUserId.HasValue
                ? templateService.DA.TemplateData().GetAccessibleTemplates(currentUserId.Value)
                : templateService.DA.TemplateData().GetPublicTemplates();
            var allTemplates = accessibleTemplates.GroupBy(t => t.Id).Select(g => g.First()).ToList();

            // Debug output
            Console.WriteLine($"[BrowseTemplates] CurrentUserId: {currentUserId}");
            foreach (var t in allTemplates)
            {
                Console.WriteLine($"[BrowseTemplates] TemplateId: {t.Id}, Title: {t.Title}, IsPublic: {t.IsPublic}, CreatedById: {t.CreatedById}");
            }

            var templateDTOs = allTemplates.Select(t => new Iforms.BLL.DTOs.TemplateDTO {
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
                    ? t.Comments.Select(c => new Iforms.BLL.DTOs.CommentDTO {
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
            var viewModel = new Iforms.MVC.Models.BrowseTemplatesViewModel {
                Templates = templateDTOs,
                CreatedByNames = createdByNames,
                Comments = commentsDict,
                LikesCount = likesDict
            };
            ViewBag.CurrentUserId = currentUserId;
            return View(viewModel);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
