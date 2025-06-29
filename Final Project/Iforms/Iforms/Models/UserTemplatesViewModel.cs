using Iforms.BLL.DTOs;
using System.Collections.Generic;

namespace Iforms.MVC.Models
{
    public class UserTemplatesViewModel
    {
        public List<TemplateDTO> Templates { get; set; } = new List<TemplateDTO>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
} 