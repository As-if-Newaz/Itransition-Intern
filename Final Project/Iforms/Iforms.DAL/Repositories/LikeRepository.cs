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
    internal class LikeRepository : Repository<Like>, ILike
    {
        public LikeRepository(ApplicationDBContext db) : base(db)
        {
        }

        public IEnumerable<Like> GetTemplateLikes(int templateId)
        {
            return db.Likes
                .Include(l => l.UserId)
                .Where(l => l.TemplateId == templateId)
                .OrderBy(l => l.CreatedAt)
                .AsNoTracking()
                .ToList();
        }
    }
}
