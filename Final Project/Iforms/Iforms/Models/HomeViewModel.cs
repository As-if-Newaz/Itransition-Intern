using Iforms.BLL.DTOs;

namespace Iforms.MVC.Models
{
    public class HomeViewModel
    {
        public List<TemplateDTO> LatestTemplates { get; set; } 
        public List<TemplateDTO> PopularTemplates { get; set; } 
        public List<TagDTO> TagCloud { get; set; } 
    }
}
