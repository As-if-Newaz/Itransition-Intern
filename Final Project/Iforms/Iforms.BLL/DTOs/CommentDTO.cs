using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.BLL.DTOs
{
    public class CommentDTO
    {
        public int Id { get; set; }

        [Required,StringLength(200)]
        public string Content { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public int TemplateId { get; set; }

        [Required]
        public int CreatedById { get; set; }

        public string? CreatedByUserName { get; set; }

    }
}
