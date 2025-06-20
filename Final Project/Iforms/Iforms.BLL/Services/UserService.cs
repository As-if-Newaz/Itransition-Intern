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
    public class UserService
    {
        private readonly DataAccess DA;

        public UserService(DataAccess dataAccess)
        {
            DA = dataAccess;
        }

        static Mapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserDTO>();
                cfg.CreateMap<UserDTO, User>();
            });
            return new Mapper(config);
        }

        public bool Register(UserDTO obj, out string errorMsg)
        {
            errorMsg = string.Empty;
            obj.UserRole = UserRole.User;
            obj.PreferredLanguage = Language.English;
            obj.PreferredTheme = Theme.Light;
            obj.CreatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            obj.UserStatus = UserStatus.Inactive;
            var data = GetMapper().Map<User>(obj);
            return DA.UserData().Create(data, out errorMsg);
        }

        public UserDTO? GetById(int userId)
        {
            var data = DA.UserData().Get(userId);
            if (data == null) return null;
            return GetMapper().Map<UserDTO>(data);
        }

        public IEnumerable<UserDTO> GetAll()
        {
            var data = DA.UserData().GetAll();
            return GetMapper().Map<List<UserDTO>>(data);

        }

        public bool BlockUsers(int[] userIds)
        {
            foreach (var userId in userIds)
            {
                if (!DA.UserData().BlockUser(userId))
                {
                    return false;
                }
            }
            return true;
        }

        public bool UnblockUsers(int[] userIds)
        {
            foreach (var userId in userIds)
            {
                if (!DA.UserData().UnblockUser(userId))
                {
                    return false;
                }
            }
            return true;
        }

        public bool DeleteUsers(int[] userIds)
        {
            foreach (var userId in userIds)
            {
                var user = DA.UserData().Get(userId);
                if (user == null)
                {
                    return false;
                }
                if (!DA.UserData().Delete(user))
                {
                    return false;
                }
            }
            return true;
        }

        public bool UpdateUser(UserDTO obj)
        {
            var data = GetMapper().Map<User>(obj);
            if (data == null)
            {
                return false;
            }
            return DA.UserData().Update(data);

        }

       
    }
}
