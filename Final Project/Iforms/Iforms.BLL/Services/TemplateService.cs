using AutoMapper;
using Iforms.BLL.DTOs;
using Iforms.DAL;
using Iforms.DAL.Entity_Framework.Table_Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;

namespace Iforms.BLL.Services
{
    public class TemplateService
    {
        public readonly DataAccess DA;

        public TemplateService(DataAccess dataAccess)
        {
            DA = dataAccess;
        }

        static Mapper GetMapper()
        {
            return new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<TemplateProfile>()));
        }

        public Template? Create(TemplateDTO createTemplateDto, int createdById)
        {
            var data = GetMapper().Map<Template>(createTemplateDto);
            
            var result = DA.TemplateData().Create(data);
            
            if (result == null) throw new InvalidOperationException("Failed to create template");
            
            if (createTemplateDto.TemplateTags?.Any() == true)
            {
                AddTagsToTemplate(result.Id, createTemplateDto.TemplateTags);
            }
            
            if (!createTemplateDto.IsPublic && createTemplateDto.TemplateAccesses?.Any() == true)
            {
                AddAccessibleUsers(result.Id, createTemplateDto.TemplateAccesses);
            }
            
            if (createTemplateDto.Questions?.Any() == true)
            {
                CreateQuestionsForTemplate(result.Id, createTemplateDto.Questions, createdById);
            }
            
            return result;
        }

        public TemplateDTO? GetTemplateById(int id, int? currentUserId = null)
        {
            var template = DA.TemplateData().Get(id);
            if (template == null) return null;
            var templateDto = GetMapper().Map<TemplateDTO>(template);
            templateDto.IsLikedByCurrentUser = currentUserId.HasValue && template.Likes.Any(l => l.UserId == currentUserId.Value);
            return templateDto;
        }

        public TemplateDTO? GetTemplateDetailedById(int id, int? currentUserId = null)
        {
            var template = DA.TemplateData().GetTemplateWithDetails(id);
            if (template == null) return null;
            var templateDto = GetMapper().Map<TemplateDTO>(template);
            templateDto.IsLikedByCurrentUser = currentUserId.HasValue && template.Likes.Any(l => l.UserId == currentUserId.Value);
            templateDto.TemplateAccesses = template.TemplateAccesses?.Where(ta => ta.User != null)
                .Select(ta => new UserDTO
                {
                    Id = ta.User.Id,
                    UserName = ta.User.UserName,
                    UserEmail = ta.User.UserEmail,
                    UserRole = (Iforms.DAL.Entity_Framework.Table_Models.Enums.UserRole)ta.User.UserRole,
                    UserStatus = (Iforms.DAL.Entity_Framework.Table_Models.Enums.UserStatus)ta.User.UserStatus,
                    CreatedAt = ta.User.CreatedAt
                }).ToList() ?? new List<UserDTO>();
            return templateDto;
        }

        public bool DeleteTemplates(int[] templateIds)
        {
            return templateIds.All(templateId =>
            {
                var template = DA.TemplateData().Get(templateId);
                return template != null && DA.TemplateData().Delete(template);
            });
        }

        public bool Update(int id, TemplateDTO updateTemplateDto, int currentUserId)
        {
            if (!DA.TemplateData().CanUserManageTemplate(id, currentUserId)) return false;
            var existingTemplate = DA.TemplateData().Get(id);
            if (existingTemplate == null) return false;
            existingTemplate.Title = updateTemplateDto.Title;
            existingTemplate.Description = updateTemplateDto.Description;
            existingTemplate.ImageUrl = updateTemplateDto.ImageUrl;
            existingTemplate.IsPublic = updateTemplateDto.IsPublic;
            existingTemplate.TopicId = updateTemplateDto.Topic.Id;
            if (!DA.TemplateData().Update(existingTemplate)) return false;
            UpdateTemplateTags(id, updateTemplateDto.TemplateTags);
            UpdateAccessibleUsers(id, updateTemplateDto.TemplateAccesses, updateTemplateDto.IsPublic);
            if (updateTemplateDto.Questions?.Any() == true)
                UpdateQuestionsForTemplate(id, updateTemplateDto.Questions, currentUserId);
            return true;
        }

        public bool CanUserAccessTemplate(int templateId, int? userId) => DA.TemplateData().CanUserAccessTemplate(templateId, userId);
        public bool CanUserManageTemplate(int templateId, int userId) => DA.TemplateData().CanUserManageTemplate(templateId, userId);

        public IEnumerable<TemplateDTO> GetUserTemplates(int userId, int? currentUserId = null) =>
            MapTemplatesWithLikes(DA.TemplateData().GetUserTemplates(userId), currentUserId);
        public IEnumerable<TemplateDTO> GetLatestTemplates(int count = 10, int? currentUserId = null) =>
            MapTemplatesWithLikes(DA.TemplateData().GetLatestTemplates(count), currentUserId);
        public IEnumerable<TemplateDTO> GetMostPopularTemplates(int count = 5, int? currentUserId = null) =>
            MapTemplatesWithLikes(DA.TemplateData().GetMostPopularTemplates(count), currentUserId);

        private IEnumerable<TemplateDTO> MapTemplatesWithLikes(IEnumerable<Template> templates, int? currentUserId) =>
            templates.Select(t =>
            {
                var dto = GetMapper().Map<TemplateDTO>(t);
                dto.IsLikedByCurrentUser = currentUserId.HasValue && t.Likes.Any(l => l.UserId == currentUserId.Value);
                return dto;
            });

        private void AddTagsToTemplate(int templateId, List<TagDTO> tags)
        {
            foreach (var tago in tags)
            {
                var tag = DA.TagData().GetOrCreateTag(tago.Name);
                if (tag != null)
                {
                    var templateTag = new TemplateTagDTO { TemplateId = templateId, TagId = tag.Id };
                    var data = GetMapper().Map<TemplateTag>(templateTag);
                    DA.TemplateTagData().Create(data);
                }
            }
        }

        private void UpdateTemplateTags(int templateId, List<TagDTO> tags)
        {
            foreach (var templateTag in DA.TemplateTagData().Find(tt => tt.TemplateId == templateId))
                DA.TemplateTagData().Delete(templateTag);
            AddTagsToTemplate(templateId, tags);
        }

        private void AddAccessibleUsers(int templateId, List<UserDTO> users)
        {
            foreach (var user in users)
            {
                var templateAccess = new TemplateAccessDTO { TemplateId = templateId, UserId = user.Id };
                var data = GetMapper().Map<TemplateAccess>(templateAccess);
                DA.TemplateAccessData().Create(data);
            }
        }

        private void UpdateAccessibleUsers(int templateId, List<UserDTO> users, bool isPublic)
        {
            foreach (var access in DA.TemplateAccessData().Find(ta => ta.TemplateId == templateId))
                DA.TemplateAccessData().Delete(access);
            if (!isPublic && users.Any()) AddAccessibleUsers(templateId, users);
        }

        private void CreateQuestionsForTemplate(int templateId, List<QuestionDTO> questions, int createdById)
        {
            foreach (var questionDto in questions)
            {
                questionDto.TemplateId = templateId;
                var question = GetMapper().Map<Question>(questionDto);
                DA.QuestionData().Create(question);
            }
        }

        public List<TemplateDTO> Search(TemplateSearchDTO searchDto, int? currentUserId = null)
        {
            var templates = DA.TemplateData().SearchTemplates(searchDto.SearchTerm ?? "", GetMapper().Map<Topic>(searchDto.Topic), searchDto.Tags);
            if (currentUserId.HasValue)
            {
                var user = DA.UserData().Get(currentUserId.Value);
                if (user?.UserRole != UserRole.Admin)
                {
                    templates = templates.Where(t => t.IsPublic || t.CreatedById == currentUserId.Value || t.TemplateAccesses.Any(ta => ta.UserId == currentUserId.Value));
                }
            }
            else
            {
                templates = templates.Where(t => t.IsPublic);
            }
            templates = searchDto.SortBy.ToLower() switch
            {
                "title" => searchDto.SortDescending ? templates.OrderByDescending(t => t.Title) : templates.OrderBy(t => t.Title),
                "createdat" => searchDto.SortDescending ? templates.OrderByDescending(t => t.CreatedAt) : templates.OrderBy(t => t.CreatedAt),
                "popularity" => searchDto.SortDescending ? templates.OrderByDescending(t => t.Forms.Count) : templates.OrderBy(t => t.Forms.Count),
                _ => templates.OrderByDescending(t => t.CreatedAt)
            };
            return MapTemplatesWithLikes(templates.Skip((searchDto.Page - 1) * searchDto.PageSize).Take(searchDto.PageSize), currentUserId).ToList();
        }

        public bool ToggleLike(int templateId, int userId)
        {
            var existingLike = DA.LikeData().GetAll().FirstOrDefault(l => l.TemplateId == templateId && l.UserId == userId);
            if (existingLike != null) return DA.LikeData().Delete(existingLike);
            return DA.LikeData().Create(new Like { TemplateId = templateId, UserId = userId, CreatedAt = DateTime.UtcNow });
        }

        public List<TopicDTO> GetAllTopics()
        {
            var topics = DA.TopicData().GetAll();
            return (topics == null || !topics.Any()) ? new List<TopicDTO>() : GetMapper().Map<List<TopicDTO>>(topics);
        }

        public List<TopicDTO> SearchTopics(string searchTerm)
        {
            var topics = DA.TopicData().GetAll();
            if (topics == null || !topics.Any()) return new List<TopicDTO>();
            return GetMapper().Map<List<TopicDTO>>(topics.Where(t => t.TopicType.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList());
        }

        public bool AddNewTopic(TopicDTO topicDto) => DA.TopicData().Create(GetMapper().Map<Topic>(topicDto));

        private void UpdateQuestionsForTemplate(int templateId, List<QuestionDTO> questions, int currentUserId)
        {
            var existingQuestions = DA.QuestionData().GetByTemplateId(templateId).ToList();
            foreach (var existingQuestion in existingQuestions.Where(eq => !questions.Any(q => q.Id == eq.Id)))
                DA.QuestionData().Delete(existingQuestion);
            foreach (var questionDto in questions)
            {
                questionDto.TemplateId = templateId;
                if (questionDto.Id > 0)
                {
                    var existingQuestion = existingQuestions.FirstOrDefault(q => q.Id == questionDto.Id);
                    if (existingQuestion != null)
                    {
                        existingQuestion.QuestionTitle = questionDto.QuestionTitle;
                        existingQuestion.QuestionDescription = questionDto.QuestionDescription;
                        existingQuestion.QuestionType = questionDto.QuestionType;
                        existingQuestion.QuestionOrder = questionDto.QuestionOrder;
                        existingQuestion.Options = questionDto.Options?.ToList();
                        existingQuestion.IsMandatory = questionDto.IsMandatory;
                        DA.QuestionData().Update(existingQuestion);
                    }
                }
                else
                {
                    var question = GetMapper().Map<Question>(questionDto);
                    DA.QuestionData().Create(question);
                }
            }
        }

        public bool UpdateImageUrl(int templateId, string? imageUrl, int currentUserId)
        {
            var template = DA.TemplateData().Get(templateId);
            if (template == null || !DA.TemplateData().CanUserManageTemplate(templateId, currentUserId)) return false;
            template.ImageUrl = imageUrl;
            return DA.TemplateData().Update(template);
        }

        public IEnumerable<TemplateDTO> GetAllTemplates( int? currentUserId = null)
        {
            var templates = DA.TemplateData().GetAllTemplates();
            return GetMapper().Map<List<TemplateDTO>>(templates)
                .Select(t =>
                {
                    t.IsLikedByCurrentUser = currentUserId.HasValue && t.Likes.Any(l => l.UserId == currentUserId.Value);
                    return t;
                });
        }

        public IEnumerable<TemplateDTO> GetAccessibleTemplates(int userId, int? currentUserId = null)
        {
            var templates = DA.TemplateData().GetAccessibleTemplates(userId);
            return GetMapper().Map<List<TemplateDTO>>(templates)
                .Select(t =>
                {
                    t.IsLikedByCurrentUser = currentUserId.HasValue && t.Likes.Any(l => l.UserId == currentUserId.Value);
                    return t;
                });
        }
        public IEnumerable<TemplateDTO> GetPublicTemplates()
        {   
            var templates = DA.TemplateData().GetPublicTemplates();
            return GetMapper().Map<List<TemplateDTO>>(templates)
                .Select(t =>
                {
                    t.IsLikedByCurrentUser = false; // Public templates don't have likes by default
                    return t;
                });
        }
    }
}
