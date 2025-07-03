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
        private readonly AuditLogService auditLogService;
        private readonly IHubContext<TemplateHub> hubContext;


        public TemplateController(
            TemplateService templateService,
            QuestionService questionService,
            CommentService commentService,
            UserService userService,
            IHubContext<TemplateHub> hubContext,
            AuditLogService auditLogService)
        {
            this.templateService = templateService;
            this.questionService = questionService;
            this.commentService = commentService;
            this.userService = userService;
            this.hubContext = hubContext;
            this.auditLogService = auditLogService;

        }

        //public IActionResult Details(int id)
        //{
        //    var currentUserId = GetCurrentUserId();
        //    var template = templateService.GetTemplateDetailedById(id, currentUserId);

        //    if (template == null)
        //        return NotFound();

        //    if (!templateService.CanUserAccessTemplate(id, currentUserId))
        //        return Forbid();

        //    var questions = questionService.GetByTemplateId(id);
        //    var comments = commentService.GetTemplateComments(id);
        //    return View(template);
        //}

        [AuthenticatedAdminorUser]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new TemplateDTO());
        }

        [AuthenticatedAdminorUser]
        [HttpPost]
        public IActionResult Create(TemplateDTO model)
        {
            var topicIdFromForm = Request.Form["Topic.Id"].ToString();
            var topicTypeFromForm = Request.Form["Topic.TopicType"].ToString();
            if ((model.Topic?.Id == 0 || string.IsNullOrEmpty(model.Topic?.TopicType)) && 
                (!string.IsNullOrEmpty(topicIdFromForm) || !string.IsNullOrEmpty(topicTypeFromForm)))
            {
                if (model.Topic == null)
                    model.Topic = new TopicDTO();
                if (int.TryParse(topicIdFromForm, out var topicId))
                    model.Topic.Id = topicId;
                if (!string.IsNullOrEmpty(topicTypeFromForm))
                    model.Topic.TopicType = topicTypeFromForm;
            }
            var templateImage = Request.Form.Files["TemplateImage"];
            if (templateImage != null && templateImage.Length > 0)
            {
                var imageUrl = ImageService.UploadTemplateImage(templateImage);
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    model.ImageUrl = imageUrl;
                }
                else
                {
                    ModelState.AddModelError("", "There was an error uploading the image. Please try again.");
                    return View(model);
                }
            }
            var templateTagsInput = Request.Form["TemplateTagsInput"].ToString();
            if (!string.IsNullOrWhiteSpace(templateTagsInput) && (model.TemplateTags == null || !model.TemplateTags.Any()))
            {
                var tagNames = templateTagsInput.Split(',')
                    .Select(t => t.Trim())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();
                model.TemplateTags = tagNames.Select(tagName => new TagDTO { Name = tagName }).ToList();
            }
            var selectedUserIdsInput = Request.Form["SelectedUserIds"].ToString();
            if (!string.IsNullOrWhiteSpace(selectedUserIdsInput))
            {
                var userIds = selectedUserIdsInput.Split(',')
                    .Select(id => id.Trim())
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(int.Parse)
                    .ToList();
                model.TemplateAccesses = userIds
                    .Select(userId => userService.GetById(userId))
                    .Where(u => u != null)
                    .ToList();
            }
            else
            {
                model.TemplateAccesses = new List<UserDTO>();
            }
            var questions = new List<QuestionDTO>();
            var questionIndex = 0;
            while (Request.Form.ContainsKey($"Questions[{questionIndex}].QuestionTitle"))
            {
                var questionIdStr = Request.Form[$"Questions[{questionIndex}].Id"].ToString();
                var questionTitle = Request.Form[$"Questions[{questionIndex}].QuestionTitle"].ToString();
                var questionTypeStr = Request.Form[$"Questions[{questionIndex}].QuestionType"].ToString();
                var questionOrder = int.Parse(Request.Form[$"Questions[{questionIndex}].QuestionOrder"].ToString());
                var optionsJson = Request.Form[$"Questions[{questionIndex}].Options"].ToString();
                var isMandatoryStr = Request.Form[$"Questions[{questionIndex}].IsMandatory"].ToString();
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
                            new List<string>(),
                        IsMandatory = isMandatoryStr == "true" || isMandatoryStr == "True"
                    };
                    questions.Add(questionDto);
                }
                questionIndex++;
            }
            model.Questions = questions;
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var currentUserId = GetCurrentUserId()!.Value;
            model.CreatedAt = DateTime.UtcNow;
            model.CreatedById = currentUserId;
            var currentUser = userService.GetById(currentUserId);
            if (currentUser != null)
            {
                model.CreatedBy = currentUser;
            }
            try
            {
                var template = templateService.Create(model, currentUserId);
                if (template != null)
                {
                    return RedirectToAction("UserTemplates", "UserDashboard");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while creating the template. Please try again.");
                return View(model);
            }
            return RedirectToAction("Index", "UserDashboard");
        }

        [AuthenticatedAdminorUser]
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

        [AuthenticatedAdminorUser]
        [HttpPost]
        public IActionResult Edit(TemplateDTO model)
        {
            var templateImage = Request.Form.Files["TemplateImage"];
            if (templateImage != null && templateImage.Length > 0)
            {
                var newImageUrl = ImageService.UploadTemplateImage(templateImage, model.ImageUrl);
                if (!string.IsNullOrEmpty(newImageUrl))
                {
                    model.ImageUrl = newImageUrl;
                }
                else
                {
                    ModelState.AddModelError("", "There was an error uploading the new image. Please try again.");
                    return View(model);
                }
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
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUserId = GetCurrentUserId()!.Value;

            var template = templateService.Update(model.Id, model, currentUserId);
            if (template == null)
                return NotFound();
            return RedirectToAction("Edit", new { id = model.Id });
        }


        [AuthenticatedAdminorUser]
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

        [AuthenticatedAdminorUser]
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

        [AuthenticatedAdminorUser]
        [HttpPost]
        public IActionResult DeleteComment(int id)
        {
            var currentUserId = GetCurrentUserId()!.Value;
            var success = commentService.Delete(id, currentUserId);
            if (!success)
                return Forbid();
            return Ok();
        }

        [AuthenticatedAdminorUser]
        [HttpPost]
        public IActionResult DeleteImage(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            if (!templateService.CanUserManageTemplate(id, currentUserId.Value))
            {
                return Forbid();
            }

            var template = templateService.GetTemplateDetailedById(id, currentUserId.Value);
            if (template == null || string.IsNullOrEmpty(template.ImageUrl))
            {
                return Json(new { success = false, message = "Image not found or already deleted." });
            }
            var imageUrlToDelete = template.ImageUrl;

            var deleteSuccess = ImageService.DeleteImage(imageUrlToDelete);

            if (!deleteSuccess)
            {
                return Json(new { success = false, message = "Failed to delete image from storage. Please try again." });
            }

            var updateSuccess = templateService.UpdateImageUrl(id, null, currentUserId.Value);
            if (updateSuccess)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Image was deleted from storage, but the template could not be updated." });
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
                ? templateService.GetAccessibleTemplates(currentUserId.Value)
                : templateService.GetPublicTemplates();
            var templateDTOs = accessibleTemplates.GroupBy(t => t.Id).Select(g => g.First()).ToList();
            var createdByNames = templateDTOs.ToDictionary(t => t.Id, t => t.CreatedBy?.UserName ?? "Unknown");
            // Get comments and likes count
            var commentsDict =templateDTOs.ToDictionary(
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
            var likesDict = templateDTOs.ToDictionary(t => t.Id, t => t.Likes?.Count ?? 0);
            var viewModel = new Iforms.MVC.Models.BrowseTemplatesViewModel {
                Templates = templateDTOs,
                CreatedByNames = createdByNames,
                Comments = commentsDict,
                LikesCount = likesDict
            };
            ViewBag.CurrentUserId = currentUserId;
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Search(string q, int? topicId = null, string tags = null, int page = 1)
        {
            var currentUserId = GetCurrentUserId();
            var searchDto = new TemplateSearchDTO
            {
                SearchTerm = q,
                Page = page,
                PageSize = 10,
                SortBy = "createdAt",
                SortDescending = true,
                Tags = !string.IsNullOrWhiteSpace(tags) ? tags.Split(',').Select(t => t.Trim()).ToList() : new List<string>()
            };
            if (topicId.HasValue)
            {
                searchDto.Topic = new TopicDTO { Id = topicId.Value };
            }
            var results = templateService.Search(searchDto, currentUserId);
            // Get CreatedBy names
            var createdByNames = results.ToDictionary(t => t.Id, t => userService.GetById(t.CreatedById)?.UserName ?? "Unknown");
            // Get comments and likes count
            var commentsDict = results.ToDictionary(
                t => t.Id,
                t => commentService.GetTemplateComments(t.Id).ToList()
            );
            var likesDict = results.ToDictionary(t => t.Id, t => t.IsLikedByCurrentUser ? 1 : 0); // You may want to get actual like counts if needed
            var viewModel = new Iforms.MVC.Models.BrowseTemplatesViewModel
            {
                Templates = results,
                CreatedByNames = createdByNames,
                Comments = commentsDict,
                LikesCount = likesDict
            };
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.SearchTerm = q;
            return View("BrowseTemplates", viewModel);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        [AuthenticatedAdminorUser]
        [HttpPost("Template/DeleteTemplates")]
        public JsonResult DeleteTemplates([FromBody] int[] templateIds)
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
    }
}
