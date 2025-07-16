using Iforms.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using Iforms.BLL.DTOs;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums.QuestionType;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;
using Microsoft.AspNetCore.Cors;

namespace Iforms.MVC.Controllers
{
    [EnableCors("AllowAllOrigins")]
    [ApiController]
    [Route("api/aggregated-results")]
    public class AggregatedResultsApiController : ControllerBase
    {
        private readonly ApiTokenService apiTokenService;
        private readonly TemplateService templateService;
        private readonly FormService formService;
        private readonly AutoMapper.IMapper mapper;

        public AggregatedResultsApiController(ApiTokenService apiTokenService, TemplateService templateService, FormService formService, AutoMapper.IMapper mapper)
        {
            this.apiTokenService = apiTokenService;
            this.templateService = templateService;
            this.formService = formService;
            this.mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetAggregatedResults([FromQuery] int templateId)
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(new { error = "Invalid or missing API token" });

            var template = templateService.GetTemplateDetailedById(templateId, userId.Value);
            if (template == null)
                return NotFound(new { error = "Template not found or not accessible" });
            var forms = formService.GetTemplateForms(templateId, userId.Value);

            return Ok(new
            {
                TemplateId = template.Id,
                TemplateTitle = template.Title,
                AuthorId = template.CreatedById,
                AuthorName = template.CreatedBy?.UserName,
                Questions = template.Questions.Select(q => Aggregate(q, forms)).ToList()
            });
        }

        [HttpGet("templates")]
        public IActionResult GetUserTemplates()
        {
            var userId = GetUserIdFromToken();
            if (userId == null)
                return Unauthorized(new { error = "Invalid or missing API token" });
            var templates = templateService.GetUserTemplates(userId.Value);
            var result = templates.Select(t => new {
                t.Id,
                t.Title,
                t.Description,
                t.IsPublic,
                t.CreatedAt,
                t.CreatedBy
            }).ToList();
            return Ok(new { templates = result });
        }

        private int? GetUserIdFromToken()
        {
            var header = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Bearer ")) return null;
            var token = apiTokenService.GetByKey(header.Substring("Bearer ".Length).Trim());
            return (token == null || token.IsRevoked) ? null : token.UserId;
        }

        private object Aggregate(QuestionDTO question, List<FormDTO> forms)
        {
            var answers = forms
                .SelectMany(f => f.Answers ?? new List<AnswerDTO>())
                .Where(a => a.QuestionId == question.Id)
                .ToList();

            return question.QuestionType switch
            {
                QuestionType.Number => AggregateNumber(question, answers),
                QuestionType.Text or QuestionType.LongText => AggregateText(question, answers),
                QuestionType.Checkbox => AggregateCheckbox(question, answers),
                QuestionType.FileUpload or QuestionType.Date => AggregateMetaOnly(question),
                _ => AggregateMetaOnly(question)
            };
        }

        private object AggregateNumber(QuestionDTO question, List<AnswerDTO> answers)
        {
            var numberAnswers = answers
                .Where(a => a.Number.HasValue)
                .Select(a => a.Number.Value)
                .ToList();
            return new
            {
                QuestionId = question.Id,
                QuestionTitle = question.QuestionTitle,
                QuestionType = question.QuestionType.ToString(),
                AnswerCount = numberAnswers.Count,
                NumberAverage = numberAnswers.Any() ? (double?)numberAnswers.Average() : null,
                NumberMin = numberAnswers.Any() ? (int?)numberAnswers.Min() : null,
                NumberMax = numberAnswers.Any() ? (int?)numberAnswers.Max() : null
            };
        }

        private object AggregateText(QuestionDTO question, List<AnswerDTO> answers)
        {
            var textAnswers = answers
                .Select(a => a.Text ?? a.LongText)
                .Where(s => !string.IsNullOrWhiteSpace(s));
            return new
            {
                QuestionId = question.Id,
                QuestionTitle = question.QuestionTitle,
                QuestionType = question.QuestionType.ToString(),
                MostPopularAnswers = GetMostPopularAnswers(textAnswers)
            };
        }

        private object AggregateCheckbox(QuestionDTO question, List<AnswerDTO> answers)
        {
            var allSelectedOptions = answers
                .Where(a => !string.IsNullOrWhiteSpace(a.Text))
                .SelectMany(a => a.Text.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries))
                .ToList();

            return new
            {
                QuestionId = question.Id,
                QuestionTitle = question.QuestionTitle,
                QuestionType = question.QuestionType.ToString(),
                MostPopularAnswers = GetMostPopularAnswers(allSelectedOptions)
            };
        }

        private object AggregateMetaOnly(QuestionDTO question)
        {
            return new
            {
                QuestionId = question.Id,
                QuestionTitle = question.QuestionTitle,
                QuestionType = question.QuestionType.ToString(),
            };
        }

        private List<object> GetMostPopularAnswers(IEnumerable<string> answerStrings)
        {
            return answerStrings
                .GroupBy(ans => ans)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => new { Answer = g.Key, Count = g.Count() })
                .Cast<object>()
                .ToList();
        }
    }
} 