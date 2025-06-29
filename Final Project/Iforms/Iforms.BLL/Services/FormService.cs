using AutoMapper;
using Iforms.BLL.DTOs;
using Iforms.DAL;
using Iforms.DAL.Entity_Framework.Table_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;

namespace Iforms.BLL.Services
{
    public class FormService
    {
        private readonly DataAccess DA;
        private readonly TemplateService TemplateService;

        public FormService(DataAccess DA, TemplateService TemplateService)
        {
            this.DA = DA;
            this.TemplateService = TemplateService;
        }

        static Mapper GetMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Form, FormDTO>();
                cfg.CreateMap<FormDTO, Form>();
                cfg.CreateMap<Answer, AnswerDTO>();
                cfg.CreateMap<AnswerDTO, Answer>();
                cfg.CreateMap<User, UserDTO>();
                cfg.CreateMap<Template, TemplateDTO>();
                cfg.CreateMap<Question, QuestionDTO>()
                    .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options != null ? src.Options.ToList() : new List<string>()));
                cfg.CreateMap<QuestionDTO, Question>()
                    .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options ?? new List<string>()));
            });
            return new Mapper(config);
        }

        public FormDTO Create(FormDTO createFormDto, int filledById)
        {
            if (!TemplateService.CanUserAccessTemplate(createFormDto.TemplateId, filledById))
                throw new UnauthorizedAccessException("User cannot access this template");

            var data = GetMapper().Map<Template>(createFormDto);
            var result = DA.TemplateData().Create(data);
            if (result != null)
            {

                foreach (var answerDto in createFormDto.Answers)
                {
                    var ans = GetMapper().Map<Answer>(answerDto);
                    DA.AnswerData().Create(ans);  
                }
            }

            throw new InvalidOperationException("Failed to create form");
        }

        public FormDTO? GetById(int id, int currentUserId)
        {
            if (!DA.FormData().CanUserAccessForm(id, currentUserId))
                return null;

            var form = DA.FormData().GetFormWithDetails(id);
            return form != null ? GetMapper().Map<FormDTO>(form) : null;
        }

        public List<FormDTO> GetUserForms(int userId)
        {
            var forms = DA.FormData().GetUserForms(userId);
            return GetMapper().Map<List<FormDTO>>(forms);
        }
        public List<FormDTO> GetTemplateForms(int templateId, int currentUserId)
        {
            if (!TemplateService.CanUserManageTemplate(templateId, currentUserId))
                throw new UnauthorizedAccessException("User cannot access template forms");

            var forms = DA.FormData().GetTemplateForms(templateId);
            return GetMapper().Map<List<FormDTO>>(forms);
        }

        public bool Delete(int id, int currentUserId)
        {
            if (!DA.FormData().CanUserManageForm(id, currentUserId))
                return false;

            var form = DA.FormData().Get(id);
            if (form == null) return false;

            return DA.FormData().Delete(form);
        }

        public bool CanUserAccessForm(int formId, int userId)
        {
            return DA.FormData().CanUserAccessForm(formId, userId);
        }

        public bool CanUserManageForm(int formId, int userId)
        {
            return DA.FormData().CanUserManageForm(formId, userId);
        }


    }
}
