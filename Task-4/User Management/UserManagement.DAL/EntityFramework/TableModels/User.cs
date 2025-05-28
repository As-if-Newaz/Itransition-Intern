using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.DAL.EntityFramework.TableModels
{
    [Index(nameof(UserEmail), IsUnique = true)]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR")]
        [StringLength(100)]
        public string UserName { get; set; }

        [Required]
        public string UserPassword { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR")]
        [StringLength(100)]
        public string UserEmail { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR")]
        [StringLength(20)]
        public string UserPhone { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
        public string UserStatus { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(100)]
        public string? UserDescription { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }


        public int? LastUpdatedByUserId { get; set; }

        public DateTime? LastUpdatedAt { get; set; }

        public virtual ICollection<UserActivity>? UserActivities { get; set; }

        public User()
        {
            UserActivities = new List<UserActivity>();
        }


    }
}
