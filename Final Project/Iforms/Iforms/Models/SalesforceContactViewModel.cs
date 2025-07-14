using System.ComponentModel.DataAnnotations;

namespace Iforms.MVC.Models
{
    public class SalesforceContactViewModel
    {
        [Required]
        public string ContactFirstName { get; set; }
        [Required]
        public string ContactLastName { get; set; }
        [Required]
        [EmailAddress]
        public string ContactEmail { get; set; }
        public string? ResultMessage { get; set; }
    }
} 