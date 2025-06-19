using Iforms.DAL.Entity_Framework;
using Iforms.DAL.Entity_Framework.Table_Models;
using Iforms.DAL.Interfaces;
using Iforms.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL
{
    public class DataAccess
    {
        private readonly ApplicationDBContext db;

        public DataAccess(ApplicationDBContext dbContext)
        {
            db = dbContext;
        }
        public IUser UserData()
        {
            return new UserRepository(db);
        }
        public IAuditLog AuditLogData()
        {
            return new AuditLogRepository(db);
        }
    }
}
