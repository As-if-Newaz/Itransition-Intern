using AutoMapper;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.BLL.DTOs;
using UserManagement.DAL;
using UserManagement.DAL.EntityFramework;
using UserManagement.DAL.EntityFramework.TableModels; 

namespace UserManagement.BLL.Services
{
    public class UserServices
    { 
        private readonly DataAccess DA;

        public UserServices(DataAccess dataAccess)
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
            obj.CreatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            obj.UserStatus = "Active";
            var data = GetMapper().Map<User>(obj);
            return DA.UserValidationData().Create(data, out errorMsg);
        }

        public IEnumerable<UserDTO> GetAll()
        {
            var data =  DA.UserCRUDData().Get();
            return GetMapper().Map<List<UserDTO>>(data);

        }

        public bool BlockUser(int[] userId)
        {
            var users = DA.UserCRUDData().Get().Where(u => userId.Contains(u.UserId));
            if (users == null) return false;
            foreach (var user in users)
            {
                user.UserStatus = "Blocked";
                DA.UserValidationData().Update(user);
            }
            return true;
        }
        public bool UnblockUser(int[] userId)
        {
            var users = DA.UserCRUDData().Get().Where(u => userId.Contains(u.UserId));
            if (users == null) return false;
            foreach (var user in users)
            {
                user.UserStatus = "Active";
                DA.UserValidationData().Update(user);
            }
            return true;
        }

        public bool Delete(int[] userId)
        {
            var users = DA.UserCRUDData().Get().Where(u => userId.Contains(u.UserId));
            if (users == null) return false;
            foreach (var user in users)
            {
                DA.UserCRUDData().Delete(user);
            }
            return true;
        }

        public IEnumerable<UserDTO> GetAllWithActivity(ActivityService activityService)
        {
            var data = DA.UserCRUDData().Get();
            var users = GetMapper().Map<List<UserDTO>>(data);
            foreach (var user in users)
            {
                var activities = activityService.GetAllLoginActivityForUser(user.UserId).ToList();
                user.LastLogin = activities.FirstOrDefault()?.LastLogin;

                // Sparkline: show last 10 logins as bars
                user.ActivityData = activities
                    .OrderByDescending(a => a.LastLogin)
                    .Take(10)
                    .Select(a => 1)
                    .ToList();
            }
            return users;
        }
    }
}
