using Iforms.DAL.Entity_Framework.Table_Models;
using Iforms.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Entity_Framework
{
    public class ApplicationDBContext : DbContext, IApplicationDBContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Template> Templates { get; set; }
        public DbSet<Form> Forms { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<TemplateTag> TemplateTags { get; set; }
        public DbSet<TemplateAccess> TemplateAccesses { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }
    }
}
