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
                cfg.CreateMap<Template, TemplateDetailedDTO>();
                cfg.CreateMap<TemplateDetailedDTO, Template>();
                cfg.CreateMap<TagDTO, Tag>();
                cfg.CreateMap<Tag, TagDTO>();
                cfg.CreateMap<TemplateTagDTO, TemplateTag>();
                cfg.CreateMap<TemplateAccessDTO, TemplateAccess>(); 
                cfg.CreateMap<TopicDTO, Topic>();



            });
            return new Mapper(config);
        }

        public bool Create(TemplateDetailedDTO createTemplateDto, int createdById)
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

                return result != null;
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

        public TemplateDetailedDTO? GetTemplateDetailedById(int id, int? currentUserId = null)
        {
            var template = DA.TemplateData().GetTemplateWithDetails(id);
            if (template == null) return null;
            var templateDto = GetMapper().Map<TemplateDetailedDTO>(template);
            templateDto.IsLikedByCurrentUser = currentUserId.HasValue &&
                template.Likes.Any(l => l.UserId == currentUserId.Value);
            return templateDto;
        }
        public bool Delete(int id, int currentUserId)
        {
            if (!DA.TemplateData().CanUserManageTemplate(id, currentUserId))
                return false;

            var template = DA.TemplateData().Get(id);
            if (template == null) return false;

            return DA.TemplateData().Delete(template);
        }

        public bool Update(int id, TemplateDetailedDTO updateTemplateDto, int currentUserId)
        {
            if (!DA.TemplateData().CanUserManageTemplate(id, currentUserId))
                return false;

            var template = DA.TemplateData().Get(id);
            if (template == null) return false;

            if (DA.TemplateData().Update(template))
            {
                UpdateTemplateTags(id, updateTemplateDto.TemplateTags);
                UpdateAccessibleUsers(id, updateTemplateDto.TemplateAccesses, updateTemplateDto.IsPublic);

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

        public TemplateSearchResultDTO Search(TemplateSearchDTO searchDto, int? currentUserId = null)
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

            return new TemplateSearchResultDTO
            {
                Templates = templateDtos.ToList(),
                TotalCount = totalCount,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize)
            };
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
    }
}
