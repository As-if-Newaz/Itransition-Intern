using Iforms.DAL.Entity_Framework;
using Iforms.DAL.Entity_Framework.Table_Models;
using Iforms.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Repositories
{
    internal class TagRepository : Repository<Tag>, ITag
    {
        public TagRepository(ApplicationDBContext db) : base(db)
        {
        }

        public IEnumerable<Tag> GetTagsWithUsageCount()
        {
            return db.Tags
                .Include(t => t.TemplateTags)
                .AsNoTracking()
                .ToList();
        }
        public IEnumerable<Tag> SearchTags(string searchTerm)
        {
            return dbSet.Where(t => t.Name.Contains(searchTerm))
                       .AsNoTracking()
                       .ToList();
        }

        public Tag? GetOrCreateTag(string tagName)
        {
            var tag = dbSet.FirstOrDefault(t => t.Name == tagName);
            if (tag == null)
            {
                tag = new Tag { Name = tagName };
                dbSet.Add(tag);
                db.SaveChanges();
            }
            return tag;
        }
    }
}
