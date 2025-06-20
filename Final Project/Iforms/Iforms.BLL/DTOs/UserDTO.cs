using Iforms.DAL.Entity_Framework.Table_Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.BLL.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "UserName is Required!") , StringLength(50)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required"), EmailAddress(ErrorMessage = "Invalid email format"), StringLength(100)]
        public string UserEmail { get; set; }

        [Required(ErrorMessage = "Password is Required!")]
        public string PasswordHash { get; set; }
        [Required]
        public UserRole UserRole { get; set; }
        [Required]
        public UserStatus UserStatus { get; set; }
        public Language? PreferredLanguage { get; set; }
        public Theme? PreferredTheme { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
