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
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Template, TemplateDTO>();
                cfg.CreateMap<TemplateDTO, Template>();
                cfg.CreateMap<Template, TemplateExtendedDTO>()
                    .ForMember(dest => dest.TemplateTags, opt => opt.MapFrom(src => src.TemplateTags.Select(tt => tt.Tag)))
                    .ForMember(dest => dest.TemplateAccesses, opt => opt.MapFrom(src => src.TemplateAccesses.Select(ta => ta.User)));
                cfg.CreateMap<TemplateExtendedDTO, Template>()
                    .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.Topic.Id))
                    .ForMember(dest => dest.Topic, opt => opt.Ignore())
                    .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                    .ForMember(dest => dest.Questions, opt => opt.Ignore())
                    .ForMember(dest => dest.Forms, opt => opt.Ignore())
                    .ForMember(dest => dest.Comments, opt => opt.Ignore())
                    .ForMember(dest => dest.Likes, opt => opt.Ignore())
                    .ForMember(dest => dest.TemplateTags, opt => opt.Ignore())
                    .ForMember(dest => dest.TemplateAccesses, opt => opt.Ignore());
                cfg.CreateMap<TagDTO, Tag>();
                cfg.CreateMap<Tag, TagDTO>();
                cfg.CreateMap<TemplateTagDTO, TemplateTag>();
                cfg.CreateMap<TemplateAccessDTO, TemplateAccess>(); 
                cfg.CreateMap<TopicDTO, Topic>();
                cfg.CreateMap<Topic, TopicDTO>();
                cfg.CreateMap<User, UserDTO>();
                cfg.CreateMap<QuestionDTO, Question>()
                    .ForMember(dest => dest.Template, opt => opt.Ignore())
                    .ForMember(dest => dest.Answers, opt => opt.Ignore())
                    .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options ?? new List<string>()));
                cfg.CreateMap<Question, QuestionDTO>()
                    .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options != null ? src.Options.ToList() : new List<string>()));
                
                // Add missing mappings for Form, Comment, and Like
                cfg.CreateMap<Form, FormDTO>()
                    .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.Answers));
                cfg.CreateMap<FormDTO, Form>()
                    .ForMember(dest => dest.Template, opt => opt.Ignore())
                    .ForMember(dest => dest.FilledBy, opt => opt.Ignore())
                    .ForMember(dest => dest.Answers, opt => opt.Ignore());
                
                cfg.CreateMap<Comment, CommentDTO>()
                    .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedBy != null ? src.CreatedBy.UserName : null));
                cfg.CreateMap<CommentDTO, Comment>()
                    .ForMember(dest => dest.Template, opt => opt.Ignore())
                    .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());
                
                cfg.CreateMap<Like, LikeDTO>();
                cfg.CreateMap<LikeDTO, Like>()
                    .ForMember(dest => dest.Template, opt => opt.Ignore())
                    .ForMember(dest => dest.User, opt => opt.Ignore());
                
                // Add Answer mappings
                cfg.CreateMap<Answer, AnswerDTO>();
                cfg.CreateMap<AnswerDTO, Answer>()
                    .ForMember(dest => dest.Form, opt => opt.Ignore())
                    .ForMember(dest => dest.Question, opt => opt.Ignore());
            });
            return new Mapper(config);
        }

        public Template? Create(TemplateExtendedDTO createTemplateDto, int createdById)
        {
            var data = GetMapper().Map<Template>(createTemplateDto);
            var result = DA.TemplateData().Create(data);
            if (result != null)
            {
                AddTagsToTemplate(result.Id, createTemplateDto.TemplateTags);

                if (!createTemplateDto.IsPublic && createTemplateDto.TemplateAccesses.Any())
                {
                    AddAccessibleUsers(result.Id, createTemplateDto.TemplateAccesses);
                }

                // Create questions for the template
                if (createTemplateDto.Questions != null && createTemplateDto.Questions.Any())
                {
                    CreateQuestionsForTemplate(result.Id, createTemplateDto.Questions, createdById);
                }

                return result;
            }

            throw new InvalidOperationException("Failed to create template");
        }

        public TemplateDTO? GetTemplateById(int id, int? currentUserId = null)
        {
            var template = DA.TemplateData().Get(id);
            if (template == null) return null;

            var templateDto = GetMapper().Map<TemplateDTO>(template);
            templateDto.IsLikedByCurrentUser = currentUserId.HasValue && 
                template.Likes.Any(l => l.UserId == currentUserId.Value);
            return templateDto;
        }

        public TemplateExtendedDTO? GetTemplateDetailedById(int id, int? currentUserId = null)
        {
            var template = DA.TemplateData().GetTemplateWithDetails(id);
            if (template == null) return null;
            var templateDto = GetMapper().Map<TemplateExtendedDTO>(template);
            templateDto.IsLikedByCurrentUser = currentUserId.HasValue &&
                template.Likes.Any(l => l.UserId == currentUserId.Value);

            // Map shared users (TemplateAccesses) to UserDTOs
            if (template.TemplateAccesses != null && template.TemplateAccesses.Any())
            {
                templateDto.TemplateAccesses = template.TemplateAccesses
                    .Where(ta => ta.User != null)
                    .Select(ta => new UserDTO
                    {
                        Id = ta.User.Id,
                        UserName = ta.User.UserName,
                        UserEmail = ta.User.UserEmail,
                        UserRole = (Iforms.DAL.Entity_Framework.Table_Models.Enums.UserRole)ta.User.UserRole,
                        UserStatus = (Iforms.DAL.Entity_Framework.Table_Models.Enums.UserStatus)ta.User.UserStatus,
                        CreatedAt = ta.User.CreatedAt
                    })
                    .ToList();
            }
            else
            {
                templateDto.TemplateAccesses = new List<UserDTO>();
            }

            return templateDto;
        }
        public bool DeleteTemplates(int[] templateIds)
        {
            foreach (var templateId in templateIds)
            {
                var template = DA.TemplateData().Get(templateId);
                if (template == null)
                {
                    return false;
                }
                if (!DA.TemplateData().Delete(template))
                {
                    return false;
                }
            }
            return true;
        }

        public bool Update(int id, TemplateExtendedDTO updateTemplateDto, int currentUserId)
        {
            if (!DA.TemplateData().CanUserManageTemplate(id, currentUserId))
                return false;

            var existingTemplate = DA.TemplateData().Get(id);
            if (existingTemplate == null) return false;

            // Update the existing template with new values
            existingTemplate.Title = updateTemplateDto.Title;
            existingTemplate.Description = updateTemplateDto.Description;
            existingTemplate.ImageUrl = updateTemplateDto.ImageUrl;
            existingTemplate.IsPublic = updateTemplateDto.IsPublic;
            existingTemplate.TopicId = updateTemplateDto.Topic.Id;

            if (DA.TemplateData().Update(existingTemplate))
            {
                UpdateTemplateTags(id, updateTemplateDto.TemplateTags);
                UpdateAccessibleUsers(id, updateTemplateDto.TemplateAccesses, updateTemplateDto.IsPublic);
                
                // Update questions for the template
                if (updateTemplateDto.Questions != null && updateTemplateDto.Questions.Any())
                {
                    UpdateQuestionsForTemplate(id, updateTemplateDto.Questions, currentUserId);
                }

                return true;
            }

            return false;
        }

        public bool CanUserAccessTemplate(int templateId, int? userId)
        {
            return DA.TemplateData().CanUserAccessTemplate(templateId, userId);
        }

        public bool CanUserManageTemplate(int templateId, int userId)
        {
            return DA.TemplateData().CanUserManageTemplate(templateId, userId);
        }

        public IEnumerable<TemplateDTO> GetUserTemplates(int userId, int? currentUserId = null)
        {
            var templates = DA.TemplateData().GetUserTemplates(userId);
            return MapTemplatesWithLikes(templates, currentUserId);
        }
        //public IEnumerable<TemplateExtendedDTO> GetUserExtendedTemplates(int userId, int? currentUserId = null)
        //{
        //    var templates = DA.TemplateData().GetUserTemplates(userId);
        //    return templates.Select(t =>
        //    {
        //        var dto = GetMapper().Map<TemplateExtendedDTO>(t);
        //        dto.IsLikedByCurrentUser = currentUserId.HasValue && t.Likes.Any(l => l.UserId == currentUserId.Value);
        //        return dto;
        //    });
        //}

        public IEnumerable<TemplateDTO> GetLatestTemplates(int count = 10, int? currentUserId = null)
        {
            var templates = DA.TemplateData().GetLatestTemplates(count);
            return MapTemplatesWithLikes(templates, currentUserId);
        }

        public IEnumerable<TemplateDTO> GetMostPopularTemplates(int count = 5, int? currentUserId = null)
        {
            var templates = DA.TemplateData().GetMostPopularTemplates(count);
            return MapTemplatesWithLikes(templates, currentUserId);
        }
        private IEnumerable<TemplateDTO> MapTemplatesWithLikes(IEnumerable<Template> templates, int? currentUserId)
        {
            return templates.Select(t =>
            {
                var dto = GetMapper().Map<TemplateDTO>(t);
                dto.IsLikedByCurrentUser = currentUserId.HasValue && t.Likes.Any(l => l.UserId == currentUserId.Value);
                return dto;
            });
        }

        private void AddTagsToTemplate(int templateId, List<TagDTO> tags)
        {
            foreach (var tago in tags)
            {
                var tag = DA.TagData().GetOrCreateTag(tago.Name);
                if (tag != null)
                {
                    var templateTag = new TemplateTagDTO
                    {
                        TemplateId = templateId,
                        TagId = tag.Id
                    };
                    var data = GetMapper().Map<TemplateTag>(templateTag);
                    DA.TemplateTagData().Create(data);
                }
            }
        }

        private void UpdateTemplateTags(int templateId, List<TagDTO> tags)
        {
            // Remove existing tags
            var existingTemplateTags = DA.TemplateTagData().Find(tt => tt.TemplateId == templateId);
            foreach (var templateTag in existingTemplateTags)
            {
                DA.TemplateTagData().Delete(templateTag);
            }
            // Add new tags
            AddTagsToTemplate(templateId, tags);
        }

        private void AddAccessibleUsers(int templateId, List<UserDTO> users)
        {
            foreach (var user in users)
            {
                var templateAccess = new TemplateAccessDTO
                {
                    TemplateId = templateId,
                    UserId = user.Id,
                };
                var data = GetMapper().Map<TemplateAccess>(templateAccess);
                DA.TemplateAccessData().Create(data);
            }
        }


        private void UpdateAccessibleUsers(int templateId, List<UserDTO> users, bool isPublic)
        {
            // Remove existing access
            var existingAccess = DA.TemplateAccessData().Find(ta => ta.TemplateId == templateId);
            foreach (var access in existingAccess)
            {
                DA.TemplateAccessData().Delete(access);
            }
            // Add new access if not public
            if (!isPublic && users.Any())
            {
                AddAccessibleUsers(templateId, users);
            }
        }

        private void CreateQuestionsForTemplate(int templateId, List<QuestionDTO> questions, int createdById)
        {
            foreach (var questionDto in questions)
            {
                questionDto.TemplateId = templateId;
                Console.WriteLine($"Creating question: {questionDto.QuestionTitle}, Options: {string.Join(", ", questionDto.Options)}");
                var question = GetMapper().Map<Question>(questionDto);
                var result = DA.QuestionData().Create(question);
                if (!result)
                {
                    Console.WriteLine($"Failed to create question: {questionDto.QuestionTitle}");
                }
            }
        }

        public List<TemplateDTO> Search(TemplateSearchDTO searchDto, int? currentUserId = null)
        {
            var templates = DA.TemplateData().SearchTemplates(searchDto.SearchTerm ?? "", GetMapper().Map<Topic>(searchDto.Topic), searchDto.Tags);

            // Apply access control
            if (currentUserId.HasValue)
            {
                var user = DA.UserData().Get(currentUserId.Value);
                if (user?.UserRole != UserRole.Admin)
                {
                    templates = templates.Where(t =>
                        t.IsPublic ||
                        t.CreatedById == currentUserId.Value ||
                        t.TemplateAccesses.Any(ta => ta.UserId == currentUserId.Value));
                }
            }
            else
            {
                templates = templates.Where(t => t.IsPublic);
            }

            var totalCount = templates.Count();

            // sorted
            templates = searchDto.SortBy.ToLower() switch
            {
                "title" => searchDto.SortDescending ? templates.OrderByDescending(t => t.Title) : templates.OrderBy(t => t.Title),
                "createdat" => searchDto.SortDescending ? templates.OrderByDescending(t => t.CreatedAt) : templates.OrderBy(t => t.CreatedAt),
                "popularity" => searchDto.SortDescending ? templates.OrderByDescending(t => t.Forms.Count) : templates.OrderBy(t => t.Forms.Count),
                _ => templates.OrderByDescending(t => t.CreatedAt)
            };

            var pagedTemplates = templates
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .ToList();

            var templateDtos = MapTemplatesWithLikes(pagedTemplates, currentUserId);

            return templateDtos.ToList();
        }

        public bool ToggleLike(int templateId, int userId)
        {
            var existingLike = DA.LikeData().GetAll().FirstOrDefault(l =>
                l.TemplateId == templateId && l.UserId == userId);

            if (existingLike != null)
            {
                return DA.LikeData().Delete(existingLike);
            }
            else
            {
                var like = new Like
                {
                    TemplateId = templateId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                return DA.LikeData().Create(like);
            }
        }

        public List<TopicDTO> GetAllTopics()
        {
            var topics = DA.TopicData().GetAll();
            if (topics == null || !topics.Any())
            {
                return new List<TopicDTO>();
            }
            return GetMapper().Map<List<TopicDTO>>(topics);
        }

        public List<TopicDTO> SearchTopics(string searchTerm)
        {
            var topics = DA.TopicData().GetAll();
            if (topics == null || !topics.Any())
            {
                return new List<TopicDTO>();
            }

            var filteredTopics = topics.Where(t => 
                t.TopicType.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            
            return GetMapper().Map<List<TopicDTO>>(filteredTopics);
        }

        public bool AddNewTopic(TopicDTO topicDto)
        {
            var topic = GetMapper().Map<Topic>(topicDto);
            return DA.TopicData().Create(topic);
        }

        private void UpdateQuestionsForTemplate(int templateId, List<QuestionDTO> questions, int currentUserId)
        {
            // Get existing questions for this template
            var existingQuestions = DA.QuestionData().GetByTemplateId(templateId).ToList();
            
            // Remove questions that are no longer in the updated list
            foreach (var existingQuestion in existingQuestions)
            {
                if (!questions.Any(q => q.Id == existingQuestion.Id))
                {
                    DA.QuestionData().Delete(existingQuestion);
                }
            }
            
            // Update or create questions
            foreach (var questionDto in questions)
            {
                questionDto.TemplateId = templateId;
                
                if (questionDto.Id > 0)
                {
                    // Update existing question
                    var existingQuestion = existingQuestions.FirstOrDefault(q => q.Id == questionDto.Id);
                    if (existingQuestion != null)
                    {
                        existingQuestion.QuestionTitle = questionDto.QuestionTitle;
                        existingQuestion.QuestionDescription = questionDto.QuestionDescription;
                        existingQuestion.QuestionType = questionDto.QuestionType;
                        existingQuestion.QuestionOrder = questionDto.QuestionOrder;
                        existingQuestion.Options = questionDto.Options?.ToList();
                        DA.QuestionData().Update(existingQuestion);
                    }
                }
                else
                {
                    // Create new question
                    Console.WriteLine($"Creating question: {questionDto.QuestionTitle}, Options: {string.Join(", ", questionDto.Options)}");
                    var question = GetMapper().Map<Question>(questionDto);
                    var result = DA.QuestionData().Create(question);
                    if (!result)
                    {
                        Console.WriteLine($"Failed to create question: {questionDto.QuestionTitle}");
                    }
                }
            }
        }

        public bool UpdateImageUrl(int templateId, string? imageUrl, int currentUserId)
        {
            var template = DA.TemplateData().Get(templateId);
            if (template == null) return false;
            if (!DA.TemplateData().CanUserManageTemplate(templateId, currentUserId)) return false;
            template.ImageUrl = imageUrl;
            return DA.TemplateData().Update(template);
        }

        public IEnumerable<TemplateDTO> GetAllTemplates()
        {
            var templates = DA.TemplateData().GetAll();

            return GetMapper().Map<List<TemplateDTO>>(templates);
        }
    }
}
