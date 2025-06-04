using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresentationCollab.Models
{
    public class Slide
    {
        public int Id { get; set; }
        [Required]
        public int PresentationId { get; set; }
        [Required]
        public int Order { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR")]
        public string Content { get; set; }

        public DateTime LastModified { get; set; }

        public virtual Presentation Presentation { get; set; }
    }
}
