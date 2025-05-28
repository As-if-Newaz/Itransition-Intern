using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.DAL.EntityFramework;
using UserManagement.DAL.EntityFramework.TableModels;
namespace UserManagement.DAL.Interfaces
{
    public interface IUser : IRepository<User>
    {
        bool Create(User obj, out string errorMsg);
        bool UpdateUserStatus(int userId, string status);
    }
}
