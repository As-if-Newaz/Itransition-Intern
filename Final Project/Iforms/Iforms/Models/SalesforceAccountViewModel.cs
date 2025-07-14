using System.ComponentModel.DataAnnotations;

namespace Iforms.MVC.Models
{
    public class SalesforceAccountViewModel
    {
        [Required]
        public string AccountName { get; set; }
        public string AccountPhone { get; set; }
        public string AccountWebsite { get; set; }
        public string? ResultMessage { get; set; }
    }
} 