using AutoMapper;
using Iforms.BLL.DTOs;
using Iforms.DAL;
using Iforms.DAL.Entity_Framework.Table_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Iforms.BLL.Services
{
    public class QuestionService
    {
        private readonly DataAccess DA;
        private readonly TemplateService templateService;

        public QuestionService(DataAccess dataAccess , TemplateService templateService)
        {
            this.DA = dataAccess;
            this.templateService = templateService;

        }

        static Mapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Question, QuestionDTO>();
                cfg.CreateMap<UserDTO, User>();

            });
            return new Mapper(config);
        }

        public bool Create(int templateId, QuestionDTO QuestionDTO, int currentUserId)
        {
            if (!templateService.CanUserManageTemplate(templateId, currentUserId))
                throw new UnauthorizedAccessException("User cannot manage this template");

            var data = GetMapper().Map<Question>(QuestionDTO);
            return DA.QuestionData().Create(data);
           
        }
    }
}
