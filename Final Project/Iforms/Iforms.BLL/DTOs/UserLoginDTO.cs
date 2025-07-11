﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.BLL.DTOs
{
    public class UserLoginDTO
    {
        [Required(ErrorMessage = "User Email is Required!")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100)]
        public string UserEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is Required!")]
        public string Password { get; set; } = string.Empty;
    }
}
