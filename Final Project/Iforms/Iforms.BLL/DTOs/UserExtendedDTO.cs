using Iforms.DAL.Entity_Framework.Table_Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Iforms.DAL.Entity_Framework.Table_Models.Enums;

namespace Iforms.BLL.DTOs
{
    public class UserExtendedDTO
    {

        public int Id { get; set; }

        [Required, StringLength(50)]
        public string UserName { get; set; }

        [Required, StringLength(100)]
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

        public virtual List<Template> CreatedTemplates { get; set; }
        public virtual List<Form> FilledForms { get; set; }
        public virtual List<Comment> Comments { get; set; }
        public virtual List<Like> Likes { get; set; }
        public virtual List<TemplateAccess> AccessibleTemplates { get; set; }
    }
}
