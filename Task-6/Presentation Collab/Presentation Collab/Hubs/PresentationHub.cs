using Microsoft.AspNetCore.SignalR;
using Presentation_Collab.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Presentation_Collab.Hubs
{
    public class PresentationHub : Hub
    {
        private readonly ApplicationDBContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PresentationHub(ApplicationDBContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Modify(string slideId, string Data)
        {
            await Clients.OthersInGroup(slideId).SendAsync("UpdateDrawing", Data);
        }

        public async Task<string> JoinBoard(string boardId, string username)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, boardId);
            await Clients.OthersInGroup(boardId).SendAsync("ReceiveUserJoinInfo", username);
            return $"Added to group with connection id {Context.ConnectionId} in group {boardId}";
        }

        public async Task SaveSvg(string slideId, string svgData)
        {
            try
            {
                var slide = await _db.Slides
                    .Include(s => s.Presentation)
                    .ThenInclude(p => p.ConnectedUsers)
                    .FirstOrDefaultAsync(s => s.Id == int.Parse(slideId));

                if (slide == null)
                {
                    throw new Exception("Slide not found");
                }

                var nickname = _httpContextAccessor.HttpContext?.Session.GetString("UserNickname");
                if (string.IsNullOrEmpty(nickname))
                {
                    throw new Exception("User not authenticated");
                }

                var user = slide.Presentation.ConnectedUsers.FirstOrDefault(u => u.Name == nickname);
                if (user == null || user.Role == UserRole.Viewer)
                {
                    throw new Exception("You don't have permission to edit this slide");
                }

                // Convert SVG string to bytes
                var svgBytes = System.Text.Encoding.UTF8.GetBytes(svgData);
                slide.SvgData = svgBytes;
                slide.LastModified = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                // Broadcast the update to all clients in the group
                await Clients.Group(slideId).SendAsync("SvgSaved", slideId);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", "Failed to save SVG: " + ex.Message);
                throw; // Re-throw to be caught by the client
            }
        }
    }
}
