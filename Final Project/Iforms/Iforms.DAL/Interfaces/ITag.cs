using Iforms.DAL.Entity_Framework.Table_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Interfaces
{
    public interface ITag : IRepository<Tag>
    {
        IEnumerable<Tag> GetTagsWithUsageCount();
        IEnumerable<Tag> GetTagCloud(int count);
        IEnumerable<Tag> SearchTags(string searchTerm);
        Tag? GetOrCreateTag(string tagName);
    }
}
