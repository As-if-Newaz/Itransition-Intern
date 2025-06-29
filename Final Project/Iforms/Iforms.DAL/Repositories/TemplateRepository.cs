using Iforms.DAL.Entity_Framework;
using Iforms.DAL.Entity_Framework.Table_Models;
using Iforms.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;

namespace Iforms.DAL.Repositories
{
    internal class TemplateRepository : Repository<Template>, ITemplate
    {
        public TemplateRepository(ApplicationDBContext db) : base(db)
        {
        }

        Template ITemplate.Create(Template template)
        {
            db.Templates.Add(template);
            db.SaveChanges();
            return template;
        }

        public IEnumerable<Template> GetPublicTemplates()
        {
            return db.Templates
                .Include(t => t.CreatedBy)
                .Include(t => t.TemplateTags)
                    .ThenInclude(tt => tt.Tag)
                .Include(t => t.Likes)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.CreatedBy)
                .Include(t => t.Forms)
                .Where(t => t.IsPublic)
                .AsNoTracking()
                .ToList();
        }

        public IEnumerable<Template> GetUserTemplates(int userId)
        {
            return db.Templates
                .Include(t => t.CreatedBy)
                .Include(t => t.TemplateTags)
                    .ThenInclude(tt => tt.Tag)
                .Include(t => t.Likes)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.CreatedBy)
                .Include(t => t.Forms)
                .Where(t => t.CreatedById == userId)
                .AsNoTracking()
                .ToList();
        }

        public IEnumerable<Template> GetAccessibleTemplates(int userId)
        {
            return db.Templates
                .Include(t => t.CreatedBy)
                .Include(t => t.TemplateTags)
                    .ThenInclude(tt => tt.Tag)
                .Include(t => t.Likes)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.CreatedBy)
                .Include(t => t.Forms)
                .Include(t => t.TemplateAccesses)
                .Where(t => t.IsPublic ||
                           t.CreatedById == userId ||
                           t.TemplateAccesses.Any(ta => ta.UserId == userId))
                .AsNoTracking()
                .ToList();
        }

        public IEnumerable<Template> GetLatestTemplates(int count)
        {
            return db.Templates
                .Include(t => t.CreatedBy)
                .Include(t => t.TemplateTags)
                    .ThenInclude(tt => tt.Tag)
                .Include(t => t.Likes)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.CreatedBy)
                .Include(t => t.Forms)
                .Where(t => t.IsPublic)
                .OrderByDescending(t => t.CreatedAt)
                .Take(count)
                .AsNoTracking()
                .ToList();
        }

        public IEnumerable<Template> GetMostPopularTemplates(int count)
        {
            return db.Templates
                .Include(t => t.CreatedBy)
                .Include(t => t.TemplateTags)
                    .ThenInclude(tt => tt.Tag)
                .Include(t => t.Likes)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.CreatedBy)
                .Include(t => t.Forms)
                .Where(t => t.IsPublic)
                .OrderByDescending(t => t.Forms.Count)
                .Take(count)
                .AsNoTracking()
                .ToList();
        }

        public Template? GetTemplateWithDetails(int id)
        {
            return db.Templates
                .Include(t => t.CreatedBy)
                .Include(t => t.Questions.OrderBy(q => q.QuestionOrder))
                .Include(t => t.TemplateTags)
                    .ThenInclude(tt => tt.Tag)
                .Include(t => t.Likes)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.CreatedBy)
                .Include(t => t.Forms)
                .Include(t => t.TemplateAccesses)
                    .ThenInclude(ta => ta.User)
                .FirstOrDefault(t => t.Id == id);
        }

        public IEnumerable<Template> SearchTemplates(string searchTerm, Topic topic, List<string> tags)
        {
            var query = db.Templates
                .Include(t => t.CreatedBy)
                .Include(t => t.TemplateTags)
                    .ThenInclude(tt => tt.Tag)
                .Include(t => t.Likes)
                .Include(t => t.Comments)
                .Include(t => t.Forms)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(t =>
                    t.Title.Contains(searchTerm) ||
                    t.Description.Contains(searchTerm) ||
                    t.Questions.Any(q => q.QuestionTitle.Contains(searchTerm) || q.QuestionDescription.Contains(searchTerm)) ||
                    t.Comments.Any(c => c.Content.Contains(searchTerm)));
            }

            if (topic !=  null)
            {
                query = query.Where(t => t.Topic == topic);
            }

            if (tags.Any())
            {
                query = query.Where(t => t.TemplateTags.Any(tt => tags.Contains(tt.Tag.Name)));
            }

            return query.AsNoTracking().ToList();
        }

        public bool CanUserAccessTemplate(int templateId, int? userId)
        {
            var template = db.Templates
                .Include(t => t.TemplateAccesses)
                .FirstOrDefault(t => t.Id == templateId);

            if (template == null) return false;
            if (template.IsPublic) return true;
            if (!userId.HasValue) return false;

            return template.CreatedById == userId.Value ||
                   template.TemplateAccesses.Any(ta => ta.UserId == userId.Value);
        }

        public bool CanUserManageTemplate(int templateId, int userId)
        {
            var template = Get(templateId);
            if (template == null) return false;

            var user = db.Users.Find(userId);
            if (user?.UserRole == UserRole.Admin) return true;

            return template.CreatedById == userId;
        }


    }
}
