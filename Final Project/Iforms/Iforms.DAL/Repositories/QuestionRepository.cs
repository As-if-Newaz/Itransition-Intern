using Iforms.DAL.Entity_Framework;
using Iforms.DAL.Entity_Framework.Table_Models;
using Iforms.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Repositories
{
    internal class QuestionRepository : Repository<Question>, IQuestion
    {
        public QuestionRepository(ApplicationDBContext db) : base(db)
        {
        }

        public IEnumerable<Question> GetByTemplateId(int templateId)
        {
            return dbSet.Where(q => q.TemplateId == templateId)
                       .OrderBy(q => q.QuestionOrder)
                       .AsNoTracking()
                       .ToList();
        }

        public bool ReorderQuestions(int templateId, Dictionary<int, int> questionOrders)
        {
            try
            {
                foreach (var kvp in questionOrders)
                {
                    var question = dbSet.FirstOrDefault(q => q.Id == kvp.Key && q.TemplateId == templateId);
                    if (question != null)
                    {
                        question.QuestionOrder = kvp.Value;
                        dbSet.Update(question);
                    }
                }
                return db.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }

        public int GetNextDisplayOrder(int templateId)
        {
            var maxOrder = dbSet.Where(q => q.TemplateId == templateId)
                              .Max(q => (int?)q.QuestionOrder) ?? 0;
            return maxOrder + 1;
        }
    }
}
