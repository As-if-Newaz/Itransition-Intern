using Iforms.DAL.Entity_Framework.Table_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Interfaces
{
    public interface ITemplate : IRepository<Template>
    {
        new Template Create(Template template);
        IEnumerable<Template> GetPublicTemplates();
        IEnumerable<Template> GetUserTemplates(int userId);
        IEnumerable<Template> GetAccessibleTemplates(int userId);
        IEnumerable<Template> GetLatestTemplates(int count);
        IEnumerable<Template> GetMostPopularTemplates(int count);
        Template? GetTemplateWithDetails(int id);
        IEnumerable<Template> SearchTemplates(string searchTerm, Topic topic, List<string> tags);
        bool CanUserAccessTemplate(int templateId, int? userId);
        bool CanUserManageTemplate(int templateId, int userId);
        IEnumerable<Template> GetAllTemplates();
    }
}
