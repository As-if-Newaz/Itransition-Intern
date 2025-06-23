using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Entity_Framework.Table_Models
{
    public class Template
    {
        public int Id { get; set; }

        [Required, Column(TypeName = "VARCHAR"), StringLength(150)]
        public string Title { get; set; }

        [Required, Column(TypeName = "VARCHAR"), StringLength(200)]
        public string Description { get; set; }

        [Column(TypeName = "VARCHAR")]
        public string? ImageUrl { get; set; }

        [Required]
        public bool IsPublic { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public int CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        public virtual User CreatedBy { get; set; }

        [Required]
        public int TopicId { get; set; }
        [ForeignKey("TopicId")]
        public virtual Topic Topic { get; set; }

        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<Form> Forms { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<TemplateTag> TemplateTags { get; set; }
        public virtual ICollection<TemplateAccess> TemplateAccesses { get; set; }

        public Template()
        {
            Questions = new List<Question>();
            Forms = new List<Form>();
            Comments = new List<Comment>();
            Likes = new List<Like>();
            TemplateTags = new List<TemplateTag>();
            TemplateAccesses = new List<TemplateAccess>();
        }
    }


}
