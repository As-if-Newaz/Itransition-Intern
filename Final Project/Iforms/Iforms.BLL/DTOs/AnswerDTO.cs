using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.BLL.DTOs
{
    public class AnswerDTO
    {
        public int Id { get; set; }

        public string? Text { get; set; }

        public string? LongText { get; set; }

        public int? Number { get; set; }

        public bool? Checkbox { get; set; }

        public string? FileUrl { get; set; }

        public DateTime? Date { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        public int FormId { get; set; }

        // Navigation property for details view
        public virtual QuestionDTO? Question { get; set; }
    }
}
