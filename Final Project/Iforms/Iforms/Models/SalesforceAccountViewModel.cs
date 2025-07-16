using System.ComponentModel.DataAnnotations;

namespace Iforms.MVC.Models
{
    public class SalesforceAccountViewModel
    {
        [Required]
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
        public string? ResultMessage { get; set; }
    }
} 