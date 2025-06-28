using AutoMapper;
using Iforms.BLL.DTOs;
using Iforms.DAL;
using Iforms.DAL.Entity_Framework.Table_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;


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
            byte[] bytes = new byte[4];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            int code = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1000000;
            obj.EmailVerificationCode = code.ToString("D6");
            obj.EmailVerificationExpiry = DateTime.UtcNow.AddMinutes(5);
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

        public UserDTO? GetUserByEmail(string email)
        {
            var user = DA.UserData().GetByEmail(email);
            if (user == null)
            {
                return null;
            }
            return GetMapper().Map<UserDTO>(user);
        }

        public bool BlockUsers(int[] userIds)
        {
            foreach (var userId in userIds)
            {
                if (!UpdateUserStatus(userId , UserStatus.Blocked))
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
                if (!UpdateUserStatus(userId, UserStatus.Active))
                {
                    return false;
                }
            }
            return true;
        }

        public bool ActivateUsers(int[] userIds)
        {
            foreach (var userId in userIds)
            {
                if (!UpdateUserStatus(userId, UserStatus.Active))
                {
                    return false;
                }
            }
            return true;
        }

        public bool InactivateUsers(int[] userIds)
        {
            foreach (var userId in userIds)
            {
                if (!UpdateUserStatus(userId, UserStatus.Inactive))
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

        public bool UpdateUserStatus(int userId, UserStatus status)
        {
            return DA.UserData().UpdateUserStatus(userId, status);
        }

        public bool UpdateUserRole(int userId, UserRole role)
        {
            return DA.UserData().UpdateUserRole(userId, role);
        }

        public bool UpdatePreferences(int userId, UserPreferencesDTO preferences)
        {
            return DA.UserData().UpdatePreferences(userId, preferences.PreferredLanguage, preferences.PreferredTheme);
        }
        public List<UserDTO> SearchUsers(string searchTerm)
        {
            var users = DA.UserData().SearchUsers(searchTerm);
            return GetMapper().Map<List<UserDTO>>(users);
        }
    }
}
