using Iforms.DAL.Entity_Framework.Table_Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Interfaces
{
    public interface IApplicationDBContext
    {
        DbSet<User> Users { get; set; }
        DbSet<Template> Templates { get; set; }
        DbSet<Form> Forms { get; set; }
        DbSet<Question> Questions { get; set; }
        DbSet<Answer> Answers { get; set; }
        DbSet<Like> Likes { get; set; }
        DbSet<Comment> Comments { get; set; }
        DbSet<Topic> Topics { get; set; }
        DbSet<TemplateTag> TemplateTags { get; set; }
        DbSet<TemplateAccess> TemplateAccesses { get; set; }
        DbSet<Tag> Tags { get; set; }
        DbSet<AuditLog> AuditLogs { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    }
}
