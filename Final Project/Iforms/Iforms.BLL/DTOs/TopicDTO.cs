using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;

namespace Iforms.BLL.DTOs
{
    public class TopicDTO
    {
        public TopicDTO()
        {
            TopicType = string.Empty;
        }
        
        public int Id { get; set; }
        
        [Required]
        public string TopicType { get; set; }
    }
}
