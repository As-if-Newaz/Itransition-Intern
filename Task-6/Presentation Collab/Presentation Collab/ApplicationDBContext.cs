using Microsoft.EntityFrameworkCore;
using Presentation_Collab.Models;

namespace Presentation_Collab
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Presentation> Presentation { get; set; }
        public DbSet<Slide> Slides { get; set; }
    }
}
