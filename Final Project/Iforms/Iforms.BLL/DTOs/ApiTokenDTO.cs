using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.BLL.DTOs
{
    public class ApiTokenDTO
    {
        public int Id { get; set; }
        public string? Key { get; set; }
        public string? Name { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public UserDTO User { get; set; }
    }
}
