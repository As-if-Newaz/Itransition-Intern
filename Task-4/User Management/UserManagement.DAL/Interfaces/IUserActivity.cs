﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.DAL.EntityFramework.TableModels;

namespace UserManagement.DAL.Interfaces
{
    public interface IUserActivity
    {
        User? Authenticate(string email, string password , out string errorMsg);
        void RecordLogin(int userId); 
        UserActivity? GetLastLogin(int userId);

        IEnumerable<UserActivity> GetAllLoginActivityForUser(int userId);
    }
}
