using Iforms.DAL.Entity_Framework.Table_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iforms.DAL.Interfaces
{
    public interface IQuestion : IRepository<Question>
    {
        IEnumerable<Question> GetByTemplateId(int templateId);
        bool ReorderQuestions(int templateId, Dictionary<int, int> questionOrders);
        int GetNextDisplayOrder(int templateId);
    }
}
