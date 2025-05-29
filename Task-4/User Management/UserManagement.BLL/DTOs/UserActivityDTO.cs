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
    public class UserActivityDTO
    {
        public int activityId { get; set; }

        public DateTime? LastLogin { get; set; }

        [Required]
        public int UserId { get; set; }
    }
}
