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
    internal class FormRepository : Repository<Form>, IForm
    {
        public FormRepository(ApplicationDBContext db) : base(db)
        {
        }

        public Form Create(Form form)
        {
            db.Forms.Add(form);
            db.SaveChanges();
            return form;
        }

        public IEnumerable<Form> GetUserForms(int userId)
        {
            return db.Forms
                .Include(f => f.FilledBy)
                .Include(f => f.Template)
                    .ThenInclude(t => t.CreatedBy)
                .Include(f => f.Answers)
                    .ThenInclude(a => a.Question)
                .Where(f => f.FilledById == userId)
                .OrderByDescending(f => f.FilledAt)
                .AsNoTracking()
                .ToList();
        }

        public IEnumerable<Form> GetTemplateForms(int templateId)
        {
            return db.Forms
                .Include(f => f.FilledBy)
                .Include(f => f.Template)
                    .ThenInclude(t => t.CreatedBy)
                .Include(f => f.Answers)
                    .ThenInclude(a => a.Question)
                .Where(f => f.TemplateId == templateId)
                .OrderByDescending(f => f.FilledAt)
                .AsNoTracking()
                .ToList();
        }

        public Form? GetFormWithDetails(int id)
        {
            return db.Forms
                .Include(f => f.FilledBy)
                .Include(f => f.Template)
                    .ThenInclude(t => t.CreatedBy)
                .Include(f => f.Answers)
                    .ThenInclude(a => a.Question)
                .FirstOrDefault(f => f.Id == id);
        }

        public bool CanUserAccessForm(int formId, int userId)
        {
            var form = db.Forms
                .Include(f => f.Template)
                .FirstOrDefault(f => f.Id == formId);

            if (form == null) return false;

            var user = db.Users.Find(userId);
            if (user?.UserRole == UserRole.Admin) return true;

            return form.FilledById == userId || form.Template.CreatedById == userId;
        }

        public bool CanUserManageForm(int formId, int userId)
        {
            var form = db.Forms
                .Include(f => f.Template)
                .FirstOrDefault(f => f.Id == formId);

            if (form == null) return false;

            var user = db.Users.Find(userId);
            if (user?.UserRole == UserRole.Admin) return true;

            return form.FilledById == userId || form.Template.CreatedById == userId;
        }
    }
}
