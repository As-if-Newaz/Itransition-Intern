using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.DAL.EntityFramework.TableModels
{
    public class UserActivity
    { 
        [Key]
        public int activityId { get; set; }

        public DateTime? LastLogin { get; set; }

        public virtual User User { get; set; }
        [ForeignKey("User")]
        [Required]
        public int UserId { get; set; }
    }
}

