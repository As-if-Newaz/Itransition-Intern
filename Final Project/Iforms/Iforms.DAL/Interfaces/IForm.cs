using Iforms.DAL.Entity_Framework.Table_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Interfaces
{
    public interface IForm : IRepository<Form>
    {
        IEnumerable<Form> GetUserForms(int userId);
        IEnumerable<Form> GetTemplateForms(int templateId);
        Form? GetFormWithDetails(int id);
        bool CanUserAccessForm(int formId, int userId);
        bool CanUserManageForm(int formId, int userId);
    }
}
