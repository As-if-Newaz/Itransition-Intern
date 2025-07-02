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
            return new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<TemplateProfile>()));
        }

        public FormDTO Create(FormDTO createFormDto, int filledById)
        {
            if (!TemplateService.CanUserAccessTemplate(createFormDto.TemplateId, filledById))
                throw new UnauthorizedAccessException("User cannot access this template");

            // Create the Form entity
            var form = new Form
            {
                TemplateId = createFormDto.TemplateId,
                FilledById = filledById,
                FilledAt = DateTime.UtcNow
            };

            var result = DA.FormData().Create(form);
            if (result != null)
            {
                // Create answers for the form
                if (createFormDto.Answers != null)
                {
                    foreach (var answerDto in createFormDto.Answers)
                    {
                        var answer = new Answer
                        {
                            FormId = result.Id,
                            QuestionId = answerDto.QuestionId,
                            Text = answerDto.Text,
                            Number = answerDto.Number,
                            SignleChoice = answerDto.SignleChoice,
                            FileUrl = answerDto.FileUrl,
                            Date = answerDto.Date
                        };

                        var answerResult = DA.AnswerData().Create(answer);
                        if (!answerResult)
                        {
                        }
                    }
                }

                // Return the created form as DTO
                return GetMapper().Map<FormDTO>(result);
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

        public bool DeleteForms(int[] formIds)
        {
            foreach (var formId in formIds)
            {
                var form = DA.FormData().Get(formId);
                if (form == null)
                {
                    return false;
                }
                if (!DA.FormData().Delete(form))
                {
                    return false;
                }
            }
            return true;
        }

        public bool CanUserAccessForm(int formId, int userId)
        {
            return DA.FormData().CanUserAccessForm(formId, userId);
        }

        public bool CanUserManageForm(int formId, int userId)
        {
            return DA.FormData().CanUserManageForm(formId, userId);
        }

        public bool Update(FormDTO formDto, int currentUserId)
        {
            // Check access
            if (!CanUserAccessForm(formDto.Id, currentUserId))
                return false;

            var form = DA.FormData().GetFormWithDetails(formDto.Id);
            if (form == null)
                return false;

            // Update answers
            var existingAnswers = form.Answers.ToList();
            var submittedAnswers = formDto.Answers ?? new List<AnswerDTO>();

            // Update or create answers
            foreach (var answerDto in submittedAnswers)
            {
                var existing = existingAnswers.FirstOrDefault(a => a.QuestionId == answerDto.QuestionId);
                if (existing != null)
                {
                    // Update existing answer
                    existing.Text = answerDto.Text;
                    existing.Number = answerDto.Number;
                    existing.SignleChoice = answerDto.SignleChoice;
                    existing.FileUrl = answerDto.FileUrl;
                    existing.Date = answerDto.Date;
                    DA.AnswerData().Update(existing);
                }
                else
                {
                    // Create new answer
                    var newAnswer = new Iforms.DAL.Entity_Framework.Table_Models.Answer
                    {
                        FormId = form.Id,
                        QuestionId = answerDto.QuestionId,
                        Text = answerDto.Text,
                        Number = answerDto.Number,
                        SignleChoice = answerDto.SignleChoice,
                        FileUrl = answerDto.FileUrl,
                        Date = answerDto.Date
                    };
                    DA.AnswerData().Create(newAnswer);
                }
            }

            // Delete removed answers
            foreach (var existing in existingAnswers)
            {
                if (!submittedAnswers.Any(a => a.QuestionId == existing.QuestionId))
                {
                    DA.AnswerData().Delete(existing);
                }
            }

            return true;
        }

        public Answer? GetAnswerById(int id)
        {
            return DA.AnswerData().Get(id);
        }

        public bool UpdateAnswer(Answer answer)
        {
            return DA.AnswerData().Update(answer);
        }

        public IEnumerable<FormDTO> GetAllForms()
        {
            var forms = DA.FormData().GetAll();
            return GetMapper().Map<IEnumerable<FormDTO>>(forms);
        }
    }
}
