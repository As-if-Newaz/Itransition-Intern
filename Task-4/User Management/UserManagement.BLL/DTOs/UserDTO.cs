using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.DAL.EntityFramework.TableModels;

namespace UserManagement.BLL.DTOs
{
    public class UserDTO
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "UserName is Required!")]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is Required!")]
        [StringLength(50)]
        public string UserPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100)]
        public string UserEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please provide phone number")]
        [StringLength(20)]
        public string UserPhone { get; set; } = string.Empty;

 
        public string? UserStatus { get; set; } = string.Empty;

        [StringLength(100)]
        public string? UserDescription { get; set; }


        public DateTime? CreatedAt { get; set; }

        public int? LastUpdatedByUserId { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public virtual ICollection<UserActivity>? UserActivities { get; set; }

        public List<int>? ActivityData { get; set; } = new List<int>();

        public DateTime? LastLogin { get; set; }
    }
}
