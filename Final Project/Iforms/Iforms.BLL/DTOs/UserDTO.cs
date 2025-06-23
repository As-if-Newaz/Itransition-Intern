using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;

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
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).*$", ErrorMessage = "The password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
        public string PasswordHash { get; set; }
        [Required]
        public UserRole UserRole { get; set; }
        [Required]
        public UserStatus UserStatus { get; set; }
        public Language? PreferredLanguage { get; set; }
        public Theme? PreferredTheme { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? EmailVerificationCode { get; set; }
        public DateTime? EmailVerificationExpiry { get; set; }
    }
}
