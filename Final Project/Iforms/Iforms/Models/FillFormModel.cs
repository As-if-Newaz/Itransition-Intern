using Iforms.BLL.DTOs;

namespace Iforms.MVC.Models
{
    public class FillFormModel
    {
        public int FormId { get; set; }
        public TemplateExtendedDTO Template { get; set; } = null!;
        public List<QuestionDTO> Questions { get; set; } = new();
        public List<AnswerDTO>? Answers { get; set; } = new();
    }
}
