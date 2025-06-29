using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Iforms.MVC.Controllers
{
    public class TemplateHub : Hub
    {
        public async Task BroadcastComment(int templateId, object comment)
        {
            await Clients.All.SendAsync("ReceiveComment", templateId, comment);
        }

        public async Task BroadcastLike(int templateId, int likesCount, bool isLiked, int userId)
        {
            await Clients.All.SendAsync("ReceiveLike", templateId, likesCount, isLiked, userId);
        }
    }
} 