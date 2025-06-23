using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;

namespace Iforms.BLL.DTOs
{
    public class QuestionDTO
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string QuestionTitle { get; set; }

        [Required,StringLength(150)]
        public string QuestionDescription { get; set; }

        public QuestionType QuestionType { get; set; }

        [Required]
        public int QuestionOrder { get; set; }

        [Required]
        public int TemplateId { get; set; }

        public virtual List<AnswerDTO>? Answers { get; set; }
    }
}
