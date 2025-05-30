using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.DAL.EntityFramework;
using UserManagement.DAL.EntityFramework.TableModels;
using UserManagement.DAL.Interfaces;

namespace UserManagement.DAL.Repos
{
    internal class UserActivityRepository : Repository<UserActivity>, IUserActivity
    {
        private readonly PasswordHasher<string> pw;
        public UserActivityRepository(ApplicationDBContext db) : base(db)
        {
            pw = new PasswordHasher<string>();
        }
        public User? Authenticate(string email, string password, out string errorMsg)
        {
            errorMsg = string.Empty;
            var user = db.Users.Where(u => u.UserEmail == email).FirstOrDefault(); 
            if (user != null)
            {
                var passVerification = pw.VerifyHashedPassword("", user.UserPassword, password);
                if (passVerification == PasswordVerificationResult.Success)
                {
                    RecordLogin(user.UserId);
                    return user;
                }
                errorMsg = "Wrong Password";
                return null;
            }
            errorMsg = "User not found!";
            return null;
        }

        public IEnumerable<UserActivity> GetAllLoginActivityForUser(int userId)
        {
            return db.UserActivities
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.LastLogin)
                .ToList();
        }

        public UserActivity? GetLastLogin(int userId)
        {
            return db.UserActivities
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.LastLogin)
                .FirstOrDefault();
        }
        public void RecordLogin(int userId)
        {
            var activity = new UserActivity
            {
                UserId = userId,
                LastLogin = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)
            };

            Create(activity);
        }
    }
}
