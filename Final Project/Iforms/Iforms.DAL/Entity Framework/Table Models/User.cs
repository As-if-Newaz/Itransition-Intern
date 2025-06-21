using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Iforms.DAL.Entity_Framework.Table_Models
{
    [Index(nameof(UserEmail), IsUnique = true)]
    public class User
    {
        public int Id { get; set; }

        [Required, Column(TypeName = "VARCHAR"), StringLength(50)]
        public string UserName { get; set; }

        [Required, Column(TypeName = "VARCHAR"), StringLength(100)]
        public string UserEmail { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public UserRole UserRole { get; set; }

        [Required]
        public UserStatus UserStatus { get; set; }

        [Required]
        public Language PreferredLanguage { get; set; }

        [Required]
        public Theme PreferredTheme { get; set; } 

        [Required]
        public DateTime CreatedAt { get; set; }

        public string? EmailVerificationCode { get; set; }
        public DateTime? EmailVerificationExpiry { get; set; }

        public virtual ICollection<Template> CreatedTemplates { get; set; }
        public virtual ICollection<Form> FilledForms { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<TemplateAccess> AccessibleTemplates { get; set; }

        public User()
        {
            CreatedTemplates = new List<Template>();
            FilledForms = new List<Form>();
            Comments = new List<Comment>();
            Likes = new List<Like>();
            AccessibleTemplates = new List<TemplateAccess>();
        }
    }
    public enum UserRole
    {
        Admin,
        User,
    }

    public enum UserStatus
    {
        Active,
        Inactive,
        Blocked
    }

    public enum Language
    {
        English,
        Polish
    }

    public enum Theme
    {
        Light,
        Dark
    }



}
