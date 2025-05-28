using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.DAL.EntityFramework.TableModels;

namespace UserManagement.DAL.EntityFramework
{
    public class ApplicationDBContext : DbContext , IApplicationDBContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {

        }
    }
}
