using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.DAL.EntityFramework.TableModels;

namespace UserManagement.DAL.EntityFramework
{
    public interface IApplicationDBContext
    {
        DbSet<User> Users { get; set; }
        DbSet<UserActivity> UserActivities { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
