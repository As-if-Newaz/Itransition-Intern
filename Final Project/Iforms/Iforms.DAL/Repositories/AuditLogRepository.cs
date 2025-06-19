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
    internal class AuditLogRepository : Repository<AuditLog>, IAuditLog
    {
        public AuditLogRepository(ApplicationDBContext db) : base(db)
        {
        }

        public bool RecordLog(int userId, string Action, string? Details)
        {
            var auditLog = new AuditLog
            {
                Action = Action,
                Details = Details,
                PerformedById = userId,
                PerformedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)
            };
            db.AuditLogs.Add(auditLog);
            return db.SaveChanges() > 0;
        }
    }
}
