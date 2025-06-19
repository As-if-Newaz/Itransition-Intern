using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.BLL.DTOs
{
    public class AuditLogDTO
    {
        public int Id { get; set; }

        [Required , StringLength(50)]
        public string Action { get; set; }

        [StringLength(150)]
        public string? Details { get; set; }

        [Required]
        public DateTime PerformedAt { get; set; }

        [Required]
        public int PerformedById { get; set; }
    }
}
