using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Presentation_Collab.Models
{
    public class Presentation
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR")]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        [Column(TypeName = "VARCHAR")]
        [StringLength(255)]
        public string CreatorName { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<Slide> Slides { get; set; }

        public virtual ICollection<User> ConnectedUsers { get; set; }
    }
}
