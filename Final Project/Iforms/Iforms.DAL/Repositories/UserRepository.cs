using Iforms.DAL.Entity_Framework;
using Iforms.DAL.Entity_Framework.Table_Models;
using Iforms.DAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Repositories
{
    internal class UserRepository : Repository<User>, IUser
    {
        private readonly PasswordHasher<string> pw;

        public UserRepository(ApplicationDBContext db) : base(db)
        {
            pw = new PasswordHasher<string>();
        }

        public bool Create(User obj, out string errorMsg)
        {
            errorMsg = string.Empty;
            try
            {
                var hashedPassword = pw.HashPassword("", obj.PasswordHash);
                obj.PasswordHash = hashedPassword;
                dbSet.Add(obj);
                if (db.SaveChanges() > 0) return true;
                return false;
            }
            catch (DbUpdateException ex)
            {
                errorMsg = "Email is already registered!";
                return false;
            }
            catch (Exception ex)
            {
                errorMsg = "Internal server error";
                return false;
            }
        }

        public User? Authenticate(string email, string password, out string errorMsg)
        {
            errorMsg = string.Empty;
            var user = db.Users.Where(u => u.UserEmail == email).FirstOrDefault();
            if (user != null)
            {
                var passVerification = pw.VerifyHashedPassword("", user.PasswordHash, password);
                if (passVerification == PasswordVerificationResult.Success)
                {
                    return user;
                }
                errorMsg = "Wrong Password";
                return null;
            }
            errorMsg = "User not found!";
            return null;
        }

        public AuditLog? GetLastLogin(int userId)
        {
            return db.AuditLogs.FirstOrDefault(u => u.PerformedById == userId && u.Action == "Login");
        }

        public User? GetByEmail(string email)
        {
            return dbSet.AsNoTracking().FirstOrDefault(u => u.UserEmail == email);
        }

        public IEnumerable<User> SearchUsers(string searchTerm)
        {
            return dbSet.Where(u => u.UserName.Contains(searchTerm) || u.UserEmail.Contains(searchTerm))
                       .AsNoTracking()
                       .ToList();
        }

        public bool UpdateUserStatus(int userId, UserStatus status)
        {
            var user = Get(userId);
            if (user != null)
            {
                user.UserStatus = status;
                return Update(user);
            }
            return false;
        }

        public bool UpdateUserRole(int userId, UserRole role)
        {
            var user = Get(userId);
            if (user != null)
            {
                user.UserRole = role;
                return Update(user);
            }
            return false;
        }

        public bool UpdatePreferences(int userId, Language language, Theme theme)
        {
            var user = Get(userId);
            if (user != null)
            {
                user.PreferredLanguage = language;
                user.PreferredTheme = theme;
                return Update(user);
            }
            return false;
        }
    }
}
