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
    public class ActivityService
    {
        private readonly DataAccess DA;

        public ActivityService(DataAccess dataAccess)
        {
            DA = dataAccess;
        }

        static Mapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UserActivity, UserActivityDTO>();
                cfg.CreateMap<UserActivityDTO, UserActivity>();
            });
            return new Mapper(config);
        }

        public IEnumerable<UserActivityDTO> GetAll()
        {
            var data = DA.UserActivityCRUDData().Get();
            return GetMapper().Map<List<UserActivityDTO>>(data);

        }

        public UserActivityDTO GetLastLogin(int userId)
        {
            var data = DA.UserActivityData().GetLastLogin(userId);
            return GetMapper().Map<UserActivityDTO>(data);
        }

        public IEnumerable<UserActivityDTO> GetAllLoginActivityForUser(int userId)
        {
            var data = DA.UserActivityData().GetAllLoginActivityForUser(userId);
            return GetMapper().Map<List<UserActivityDTO>>(data);
        }
    }
}
