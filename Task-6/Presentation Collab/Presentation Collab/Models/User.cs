using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Presentation_Collab.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
        public string Name { get; set; }
        [Required]
        public UserRole Role { get; set; }

        public virtual Presentation Presentation { get; set; }
        public int PresentationId { get; set; }
    }
    public enum UserRole
    {
        Viewer,
        Editor,
        Creator
    }
}

