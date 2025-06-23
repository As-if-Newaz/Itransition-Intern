using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Entity_Framework.Table_Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required, Column(TypeName = "VARCHAR"), StringLength(50)]
        public string Name { get; set; }

        public virtual ICollection<TemplateTag> TemplateTags { get; set; }
        public Tag()
        {
            TemplateTags = new List<TemplateTag>();
        }
    }
}
