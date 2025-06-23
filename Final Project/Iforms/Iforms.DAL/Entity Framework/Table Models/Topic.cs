using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;

namespace Iforms.DAL.Entity_Framework.Table_Models
{
    public class Topic
    {
        public int Id { get; set; }

        public TopicType Type { get; set; }

    }

    
}
