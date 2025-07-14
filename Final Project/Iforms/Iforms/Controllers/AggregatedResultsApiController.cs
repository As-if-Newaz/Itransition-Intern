using Iforms.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using Iforms.BLL.DTOs;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums.QuestionType;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;

namespace Iforms.MVC.Controllers
{
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

        private int? GetUserIdFromToken()
        {
            var header = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Bearer ")) return null;
            var token = apiTokenService.GetByKey(header.Substring("Bearer ".Length).Trim());
            return (token == null || token.IsRevoked) ? null : token.UserId;
        }

        private object Aggregate(QuestionDTO q, List<FormDTO> forms)
        {
            var answers = forms.SelectMany(f => f.Answers ?? new List<AnswerDTO>()).Where(a => a.QuestionId == q.Id).ToList();
            bool IsType(QuestionType t) => q.QuestionType == t;
            return new
            {
                QuestionId = q.Id,
                QuestionTitle = q.QuestionTitle,
                QuestionType = q.QuestionType.ToString(),
                AnswerCount = answers.Count,
                NumberAverage = IsType(Number) && answers.Any(a => a.Number.HasValue) ? (double?)answers.Where(a => a.Number.HasValue).Average(a => a.Number.Value) : null,
                NumberMin = IsType(Number) && answers.Any(a => a.Number.HasValue) ? (int?)answers.Where(a => a.Number.HasValue).Min(a => a.Number.Value) : null,
                NumberMax = IsType(Number) && answers.Any(a => a.Number.HasValue) ? (int?)answers.Where(a => a.Number.HasValue).Max(a => a.Number.Value) : null,
                MostPopularAnswers = (IsType(Text) || IsType(LongText)) ? answers.Select(a => a.Text ?? a.LongText)
                .Where(s => !string.IsNullOrWhiteSpace(s)).GroupBy(t => t).OrderByDescending(g => g.Count()).Take(3).Select(g => new { Answer = g.Key, Count = g.Count() }).ToList() : null,
                CheckboxTrueCount = IsType(Checkbox) ? (int?)answers.Count(a => a.Checkbox == true) : null,
                CheckboxFalseCount = IsType(Checkbox) ? (int?)answers.Count(a => a.Checkbox == false) : null,
                DateMin = IsType(Date) && answers.Any(a => a.Date.HasValue) ? (System.DateTime?)answers.Where(a => a.Date.HasValue).Min(a => a.Date.Value) : null,
                DateMax = IsType(Date) && answers.Any(a => a.Date.HasValue) ? (System.DateTime?)answers.Where(a => a.Date.HasValue).Max(a => a.Date.Value) : null
            };
        }
    }
} 