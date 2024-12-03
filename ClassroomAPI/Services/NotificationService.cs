using ClassroomAPI.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ClassroomAPI.Services
{
    public class NotificationService
    {
        private readonly IHubContext<ChatHub> _hubContext;
        public NotificationService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotification(string userId, string notification)
        {
            await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", notification);
        }
    }
}
