using Iforms.DAL.Entity_Framework.Table_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Interfaces
{
    public interface IUser : IRepository<User>
    {
        bool Create(User obj, out string errorMsg);
        User? Authenticate(string email, string password, out string errorMsg);
        AuditLog? GetLastLogin(int userId);
        bool BlockUser(int userId);
        bool UnblockUser(int userId);
        User? GetByEmail(string email);
    }
}
