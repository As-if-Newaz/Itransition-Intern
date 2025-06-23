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
    internal class CommentRepository : Repository<Comment>, IComment
    {
        public CommentRepository(ApplicationDBContext db) : base(db)
        {
        }

        public IEnumerable<Comment> GetTemplateComments(int templateId)
        {
            return db.Comments
                .Include(c => c.CreatedBy)
                .Where(c => c.TemplateId == templateId)
                .OrderBy(c => c.CreatedAt)
                .AsNoTracking()
                .ToList();
        }

        public bool CanUserDeleteComment(int commentId, int userId)
        {
            var comment = Get(commentId);
            if (comment == null) return false;

            var user = db.Users.Find(userId);
            if (user?.UserRole == UserRole.Admin) return true;

            return comment.CreatedById == userId;
        }


    }
}
