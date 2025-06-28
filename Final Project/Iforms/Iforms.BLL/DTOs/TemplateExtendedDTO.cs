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
        public TemplateExtendedDTO()
        {
            Topic = new TopicDTO();
            Questions = new List<QuestionDTO>();
            Forms = new List<FormDTO>();
            Comments = new List<CommentDTO>();
            Likes = new List<LikeDTO>();
            TemplateTags = new List<TagDTO>();
            TemplateAccesses = new List<UserDTO>();
        }

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

        public TopicDTO Topic { get; set; }

        public bool IsLikedByCurrentUser { get; set; }

        public List<QuestionDTO> Questions { get; set; }
        public List<FormDTO> Forms { get; set; }
        public List<CommentDTO> Comments { get; set; }
        public List<LikeDTO> Likes { get; set; }
        public List<TagDTO> TemplateTags { get; set; }
        public List<UserDTO> TemplateAccesses { get; set; }
    }
}
