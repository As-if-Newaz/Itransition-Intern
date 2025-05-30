using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UserManagement.DAL.EntityFramework;
using UserManagement.DAL.EntityFramework.TableModels;
using UserManagement.DAL.Interfaces;
using static UserManagement.DAL.Repos.UserRepository;

namespace UserManagement.DAL.Repos
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
                var hashedPassword = pw.HashPassword("", obj.UserPassword);
                obj.UserPassword = hashedPassword;
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
            return false;
        }
        public bool UpdateUserStatus(int userId, string status)
        {
            var user = Get(userId);
            if (user != null)
            {
                user.UserStatus = status;
                return Update(user);
            }
            return false;
        }

        
    }



}

