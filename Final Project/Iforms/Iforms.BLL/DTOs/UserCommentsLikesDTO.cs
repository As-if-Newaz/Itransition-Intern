using Iforms.DAL.Entity_Framework.Table_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.BLL.DTOs
{
    public class UserCommentsLikesDTO
    {
        public int UserId { get; set; }
        public virtual List<CommentDTO>? Comments { get; set; }
        public virtual List<LikeDTO>? Likes { get; set; }
    }
}
