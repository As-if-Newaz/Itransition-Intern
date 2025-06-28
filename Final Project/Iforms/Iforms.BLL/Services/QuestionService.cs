using AutoMapper;
using Iforms.BLL.DTOs;
using Iforms.DAL;
using Iforms.DAL.Entity_Framework.Table_Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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

        public QuestionDTO? GetById(int id)
        {
            var question = DA.QuestionData().Get(id);
            if (question == null) return null;
            var questionDto = GetMapper().Map<QuestionDTO>(question);
            return questionDto;
        }

        public List<QuestionDTO> GetByTemplateId(int templateId)
        {
            var questions = DA.QuestionData().GetByTemplateId(templateId);
            var questionDtos = GetMapper().Map<List<QuestionDTO>>(questions);
            foreach (var questionDto in questionDtos)
            {
                var question = questions.First(q => q.Id == questionDto.Id);
            }
            return questionDtos;
        }

        public bool Update(int id, QuestionDTO updateQuestionDto, int currentUserId)
        {
            var question = DA.QuestionData().Get(id);
            if (question == null) return false;
            
            if (!templateService.CanUserManageTemplate(question.TemplateId, currentUserId))
                throw new UnauthorizedAccessException("User cannot update this template");
            var data = GetMapper().Map<Question>(updateQuestionDto);
            return DA.QuestionData().Update(data);
        }

        public bool Delete(int id, int currentUserId)
        {
            var question = DA.QuestionData().Get(id);
            if (question == null) return false;

            if (!templateService.CanUserManageTemplate(question.TemplateId, currentUserId))
                throw new UnauthorizedAccessException("User cannot Delete this template");

            return DA.QuestionData().Delete(question);
        }
        public bool ReorderQuestions( TemplateExtendedDTO templateDto, int currentUserId)
        {
            if (!templateService.CanUserManageTemplate(templateDto.Id, currentUserId))
                throw new UnauthorizedAccessException("User cannot manage this template");

            var questionOrders = templateDto.Questions.ToDictionary(q => q.Id, q => q.QuestionOrder);
            return DA.QuestionData().ReorderQuestions(templateDto.Id, questionOrders);
        }
    }

}
