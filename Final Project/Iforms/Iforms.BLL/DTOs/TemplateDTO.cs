using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.BLL.DTOs
{
    public class TemplateDTO
    {
        public TemplateDTO()
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

        [Required]
        public int TopicId { get; set; }

        public bool IsLikedByCurrentUser { get; set; }

        // Topic and related collections from TemplateExtendedDTO
        public TopicDTO Topic { get; set; }
        public List<QuestionDTO> Questions { get; set; }
        public List<FormDTO> Forms { get; set; }
        public List<CommentDTO> Comments { get; set; }
        public List<LikeDTO> Likes { get; set; }
        public List<TagDTO> TemplateTags { get; set; }
        public List<UserDTO> TemplateAccesses { get; set; }
    }
}
