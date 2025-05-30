using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.BLL.DTOs;
using UserManagement.DAL;
using UserManagement.DAL.EntityFramework.TableModels;

namespace UserManagement.BLL.Services
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
            var user = DA.UserActivityData().Authenticate(loginData.UserEmail, loginData.Password, out errorMsg );
            if (user == null)
            {   
                return null;
            }
            if (user.UserStatus.Equals("Blocked"))
            {
                return null;
            }
            return GetMapper().Map<UserDTO>(user);

        }
    }
}
