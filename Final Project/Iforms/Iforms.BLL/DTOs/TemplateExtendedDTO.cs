using Iforms.DAL.Entity_Framework.Table_Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.BLL.DTOs
{
    public class TemplateExtendedDTO
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Title { get; set; }

        [Required, StringLength(200)]
        public string Description { get; set; }

        public string? ImageUrl { get; set; }

        [Required]
        public bool IsPublic { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public int CreatedById { get; set; }

        [Required]
        public int TopicId { get; set; }

        public bool IsLikedByCurrentUser { get; set; }

        public virtual List<QuestionDTO> Questions { get; set; }
        public virtual List<FormDTO> Forms { get; set; }
        public virtual List<CommentDTO> Comments { get; set; }
        public virtual List<LikeDTO> Likes { get; set; }
        public virtual List<TagDTO> TemplateTags { get; set; }
        public virtual List<UserDTO> TemplateAccesses { get; set; }
    }
}
