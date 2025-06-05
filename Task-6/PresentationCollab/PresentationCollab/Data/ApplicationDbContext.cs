using Microsoft.EntityFrameworkCore;
using PresentationCollab.Models;

namespace PresentationCollab.Data
{
    public class ApplicationDbContext :DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Presentation> Presentations { get; set; }
        public DbSet<Slide> Slides { get; set; }
        public DbSet<User> UserPresences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Presentation>()
                .HasMany(p => p.Slides)
                .WithOne(s => s.Presentation)
                .HasForeignKey(s => s.PresentationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Presentation>()
                .HasMany(p => p.ConnectedUsers)
                .WithOne(u => u.Presentation)
                .HasForeignKey(u => u.PresentationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Slide>()
                .Property(s => s.Content)
                .IsRequired();

            modelBuilder.Entity<Slide>()
                .Property(s => s.LastModified)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<User>()
                .Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<User>()
                .HasIndex(u => new { u.PresentationId, u.Name })
                .IsUnique();
        }

    }
}
