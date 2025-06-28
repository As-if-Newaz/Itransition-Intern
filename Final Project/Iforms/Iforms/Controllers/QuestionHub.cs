using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Iforms.MVC.Controllers
{
    public class QuestionHub : Hub
    {
        public async Task AddQuestion(object question)
        {
            await Clients.Others.SendAsync("QuestionAdded", question);
        }

        public async Task UpdateQuestionOrder(object questions)
        {
            await Clients.Others.SendAsync("QuestionOrderUpdated", questions);
        }
    }
} 