using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Entity_Framework.Table_Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required, Column(TypeName = "VARCHAR"), StringLength(50)]
        public string Action { get; set; }

        [Required, Column(TypeName = "VARCHAR"), StringLength(150)]
        public string Details { get; set; }

        [Required]
        public DateTime PerformedAt { get; set; }

        [Required]
        public int PerformedById { get; set; }

        [ForeignKey("PerformedById")]
        public virtual User PerformedBy { get; set; }
       
    }
}
