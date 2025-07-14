using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Entity_Framework.Table_Models
{
    [Index(nameof(Key), IsUnique = true)]
    public class ApiToken
    {
        [Key]
        public int Id { get; set; }
        public string? Key { get; set; }
        public string? Name { get; set; } 
        public bool IsRevoked { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
