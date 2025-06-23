using Iforms.DAL.Entity_Framework.Table_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Interfaces
{
    public interface IComment : IRepository<Comment>
    {
        IEnumerable<Comment> GetTemplateComments(int templateId);
        bool CanUserDeleteComment(int commentId, int userId);
    }
}
