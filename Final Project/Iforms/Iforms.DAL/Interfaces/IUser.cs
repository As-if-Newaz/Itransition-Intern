using Iforms.DAL.Entity_Framework.Table_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;

namespace Iforms.DAL.Interfaces
{
    public interface IUser : IRepository<User>
    {
        bool Create(User obj, out string errorMsg);
        User? Authenticate(string email, string password, out string errorMsg);
        AuditLog? GetLastLogin(int userId);
        User? GetByEmail(string email);
        IEnumerable<User> SearchUsers(string searchTerm);
        bool UpdateUserStatus(int userId, UserStatus status);
        bool UpdateUserRole(int userId, UserRole role);
        bool UpdatePreferences(int userId, Language language, Theme theme);
    }
}
