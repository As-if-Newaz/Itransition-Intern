using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;

namespace Iforms.DAL.Entity_Framework.Table_Models
{
    public class Topic
    {
        public int Id { get; set; }

        [Required, Column(TypeName = "VARCHAR"), StringLength(50)]
        public string TopicType { get; set; }

    }

    
}
