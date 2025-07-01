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
                cfg.CreateMap<Form, FormDTO>()
                    .ForMember(dest => dest.Template, opt => opt.MapFrom(src => src.Template))
                    .ForMember(dest => dest.FilledBy, opt => opt.MapFrom(src => src.FilledBy))
                    .ForMember(dest => dest.Answers, opt => opt.MapFrom(src => src.Answers));
                cfg.CreateMap<FormDTO, Form>();
                cfg.CreateMap<Answer, AnswerDTO>()
                    .ForMember(dest => dest.Question, opt => opt.MapFrom(src => src.Question));
                cfg.CreateMap<AnswerDTO, Answer>();
                cfg.CreateMap<User, UserDTO>();
                cfg.CreateMap<UserDTO, User>();
                cfg.CreateMap<Template, TemplateDTO>();
                cfg.CreateMap<TemplateDTO, Template>();
                cfg.CreateMap<Template, TemplateExtendedDTO>();
                cfg.CreateMap<TemplateExtendedDTO, Template>();
                cfg.CreateMap<Question, QuestionDTO>()
                    .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options != null ? src.Options.ToList() : new List<string>()));
                cfg.CreateMap<QuestionDTO, Question>()
                    .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options ?? new List<string>()));
                cfg.CreateMap<Comment, CommentDTO>();
                cfg.CreateMap<CommentDTO, Comment>();
                cfg.CreateMap<Like, LikeDTO>();
                cfg.CreateMap<LikeDTO, Like>();
                cfg.CreateMap<Tag, TagDTO>();
                cfg.CreateMap<TagDTO, Tag>();
                cfg.CreateMap<Topic, TopicDTO>();
                cfg.CreateMap<TopicDTO, Topic>();
            });
            return new Mapper(config);
        }

        public FormDTO Create(FormDTO createFormDto, int filledById)
        {
            Console.WriteLine($"[FormService.Create] Starting form creation for template {createFormDto.TemplateId} by user {filledById}");
            
            if (!TemplateService.CanUserAccessTemplate(createFormDto.TemplateId, filledById))
                throw new UnauthorizedAccessException("User cannot access this template");

            // Create the Form entity
            var form = new Form
            {
                TemplateId = createFormDto.TemplateId,
                FilledById = filledById,
                FilledAt = DateTime.UtcNow
            };

            Console.WriteLine($"[FormService.Create] Creating form with TemplateId: {form.TemplateId}, FilledById: {form.FilledById}");

            var result = DA.FormData().Create(form);
            if (result != null)
            {
                Console.WriteLine($"[FormService.Create] Form created successfully with ID: {result.Id}");
                
                // Create answers for the form
                if (createFormDto.Answers != null)
                {
                    Console.WriteLine($"[FormService.Create] Creating {createFormDto.Answers.Count} answers");
                    
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
                        
                        Console.WriteLine($"[FormService.Create] Creating answer for question {answerDto.QuestionId}, Text: {answerDto.Text}, Number: {answerDto.Number}, SingleChoice: {answerDto.SignleChoice}");
                        
                        var answerResult = DA.AnswerData().Create(answer);
                        if (!answerResult)
                        {
                            Console.WriteLine($"[FormService.Create] Failed to create answer for question {answerDto.QuestionId}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"[FormService.Create] No answers to create");
                }

                // Return the created form as DTO
                return GetMapper().Map<FormDTO>(result);
            }

            Console.WriteLine($"[FormService.Create] Failed to create form");
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
    }
}
