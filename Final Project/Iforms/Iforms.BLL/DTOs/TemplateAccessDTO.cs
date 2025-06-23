using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.BLL.DTOs
{
    public class TemplateAccessDTO
    {
        public int Id { get; set; }

        [Required]
        public int TemplateId { get; set; }

        [Required]
        public int UserId { get; set; }
    }
}
