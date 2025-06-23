using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Entity_Framework.Table_Models
{
    public class Form
    {

        public int Id { get; set; }

        [Required]
        public DateTime FilledAt { get; set; }

        [Required]
        public int FilledById { get; set; }

        [ForeignKey("FilledById")]
        public virtual User FilledBy { get; set; }

        [Required]
        public int TemplateId { get; set; }

        [ForeignKey("TemplateId")]
        public virtual Template Template { get; set; }

        public virtual ICollection<Answer> Answers { get; set; }


        public Form()
        {
            Answers = new List<Answer>();
        }
    }
    
}
