using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;

namespace Iforms.DAL.Entity_Framework.Table_Models
{
    public class Question
    {
        public int Id { get; set; }

        [Required, Column(TypeName = "VARCHAR"), StringLength(100)]
        public string QuestionTitle { get; set; }

        [Required, Column(TypeName = "VARCHAR"), StringLength(500)]
        public string QuestionDescription { get; set; }

        public QuestionType QuestionType { get; set; }

        [Required]
        public int QuestionOrder { get; set; }

        public IEnumerable<string>? Options { get; set; }

        [Required]
        public bool IsMandatory { get; set; }

        [Required]
        public int TemplateId { get; set; }

        [ForeignKey("TemplateId")]
        public virtual Template Template { get; set; }

        public virtual ICollection<Answer> Answers { get; set; }

        public Question()
        {
            Answers = new List<Answer>();
        }
    }


}
