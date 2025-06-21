using Iforms.DAL.Entity_Framework;
using Iforms.DAL.Entity_Framework.Table_Models;
using Iforms.DAL.Interfaces;
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
    }
}
