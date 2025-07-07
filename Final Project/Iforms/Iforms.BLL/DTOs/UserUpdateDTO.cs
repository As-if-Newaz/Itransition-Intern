using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;

namespace Iforms.BLL.DTOs
{
    public class UserUpdateDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "UserName is Required!"), StringLength(50)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required"), EmailAddress(ErrorMessage = "Invalid email format"), StringLength(100)]
        public string UserEmail { get; set; }
        public UserStatus? UserStatus { get; set; }
        public string? EmailVerificationCode { get; set; }
        public DateTime? EmailVerificationExpiry { get; set; }
    }
}
