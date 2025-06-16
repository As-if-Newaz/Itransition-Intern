using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Entity_Framework.Table_Models
{
    public class Topic
    {
        public int Id { get; set; }

        public TopicType Type { get; set; }

    }

    public enum TopicType
    {
        Quiz,
        Submission,
        Poll,
        Survey,
        Feedback,
        Registration,
        Event,
        Other
    }
}
