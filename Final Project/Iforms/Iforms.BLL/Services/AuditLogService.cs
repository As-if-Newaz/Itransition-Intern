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
    public class AuditLogService
    {
        private readonly DataAccess DA;

        public AuditLogService(DataAccess dataAccess)
        {
            DA = dataAccess;
        }

        static Mapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<AuditLog, AuditLogDTO>();
                cfg.CreateMap<AuditLogDTO, AuditLog>();
            });
            return new Mapper(config);
        }

        public bool RecordLog(int userId, string action, string description)
        {
            return DA.AuditLogData().RecordLog(userId, action, description);
        }

        public bool Delete(int logId)
        {
            var log = DA.AuditLogData().Get(logId);
            if (log == null) return false;
            DA.AuditLogData().Delete(log);
            return true;
        }

        public IEnumerable<AuditLogDTO> GetAll()
        {
            var data = DA.UserData().GetAll();
            return GetMapper().Map<List<AuditLogDTO>>(data);

        }

        public AuditLogDTO? GetById(int logId)
        {
            var data = DA.AuditLogData().Get(logId);
            if (data == null) return null;
            return GetMapper().Map<AuditLogDTO>(data);
        }

        public bool Update(AuditLogDTO obj)
        {
            var data = GetMapper().Map<AuditLog>(obj);
            if (data == null)
            {
                return false;
            }
            return DA.AuditLogData().Update(data);
        }
    }
}
