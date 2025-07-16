using System.ComponentModel.DataAnnotations;

namespace Iforms.MVC.Models
{
    public class SalesforceContactViewModel
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string? ResultMessage { get; set; }
    }
} 