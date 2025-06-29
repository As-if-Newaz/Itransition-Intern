using System.Collections.Generic;
using Iforms.BLL.DTOs;

namespace Iforms.MVC.Models
{
    public class BrowseTemplatesViewModel
    {
        public List<TemplateDTO> Templates { get; set; }
        public Dictionary<int, string> CreatedByNames { get; set; }
        public Dictionary<int, List<CommentDTO>> Comments { get; set; }
        public Dictionary<int, int> LikesCount { get; set; }
    }
} 