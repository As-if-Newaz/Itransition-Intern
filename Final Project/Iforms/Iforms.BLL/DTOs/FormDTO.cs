using Iforms.DAL.Entity_Framework.Table_Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.BLL.DTOs
{
    public class FormDTO
    {
        public int Id { get; set; }

        [Required]
        public DateTime FilledAt { get; set; }

        [Required]
        public int FilledById { get; set; }

        [Required]
        public int TemplateId { get; set; }

        public virtual List<AnswerDTO>? Answers { get; set; }
        
        // Navigation properties for details view
        public virtual TemplateDTO? Template { get; set; }
        public virtual UserDTO? FilledBy { get; set; }
    }
}
