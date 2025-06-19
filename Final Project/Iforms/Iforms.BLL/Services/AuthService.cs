using AutoMapper;
using Iforms.BLL.DTOs;
using Iforms.DAL;
using Iforms.DAL.Entity_Framework.Table_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.BLL.Services
{
    public class AuthService
    {
        private readonly DataAccess DA;

        public AuthService(DataAccess dataAccess)
        {
            DA = dataAccess;
        }

        static Mapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserDTO>();
            });
            return new Mapper(config);
        }

        public UserDTO? Authenticate(UserLoginDTO loginData, out string errorMsg)
        {
            errorMsg = string.Empty;
            var user = DA.UserData().Authenticate(loginData.UserEmail, loginData.Password, out errorMsg);
            if (user == null)
            {
                errorMsg = "Login Failed!";
                return null;
            }
            if (user.IsBlocked == true)
            {
                errorMsg = "User is blocked!";
                DA.AuditLogData().RecordLog(user.Id, "Blocked Login Attempt", "Block user attempted login");
                return null;
            }
            DA.AuditLogData().RecordLog(user.Id, "Login", "User logged in successfully");
            return GetMapper().Map<UserDTO>(user);

        }

    }
}
