using Iforms.DAL.Entity_Framework.Table_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Interfaces
{
    public interface IAuditLog : IRepository<AuditLog>
    {
        bool RecordLog(int userId, string Action, string? Details);

        IEnumerable<AuditLog>? GetAuditByUserId(int userId);
    }
}
