using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.DAL.EntityFramework;
using UserManagement.DAL.EntityFramework.TableModels;
using UserManagement.DAL.Interfaces;
using UserManagement.DAL.Repos;

namespace UserManagement.DAL
{
    public class DataAccess
    {
        private readonly ApplicationDBContext db;

        public DataAccess(ApplicationDBContext dbContext)
        {
            db = dbContext;
        }

        public IRepository<User> UserCRUDData()
        {
            return new UserRepository(db);
        }

        public IRepository<UserActivity> UserActivityCRUDData()
        {
            return new UserActivityRepository(db);
        }
       public IUser UserValidationData()
        {
            return new UserRepository(db);
        }

        public IUserActivity UserActivityData()
        {
            return new UserActivityRepository(db);
        }
    }
}
