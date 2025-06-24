using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.BLL.DTOs
{
    public class UserFormsDTO
    {
        public int UserId { get; set; }

        public virtual List<FormDTO>? FilledForms { get; set; }
    }
}
