using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Entity_Framework.Table_Models
{
    public class Answer
    {
        public int Id { get; set; }

        [Column(TypeName = "VARCHAR")]
        public string? Text { get; set; }

        public int? Number { get; set; }

        public bool? SignleChoice { get; set; }

        [Column(TypeName = "VARCHAR")]
        public string? FileUrl { get; set; }

        public DateTime? Date { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }

        [Required]
        public int FormId { get; set; }

        [ForeignKey("FormId")]
        public virtual Form Form { get; set; }


    }
}
