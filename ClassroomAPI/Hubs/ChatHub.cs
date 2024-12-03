using ClassroomAPI.Services;
using Microsoft.AspNetCore.SignalR;

namespace ClassroomAPI.Hubs
{
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var UserId = Context.UserIdentifier;
            if (UserId != null)
                OnlineUserTracker.SetUserOnline(UserId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var UserId = Context.UserIdentifier;
            if (UserId != null)
                OnlineUserTracker.SetUserOffline(UserId);
            await base.OnDisconnectedAsync(exception);
        }

        //Send message to a specific course
        public async Task SendMessage(string courseId, string userName, string message, string? fileUrl = null)
        {
            if(fileUrl != null)
                await Clients.Group(courseId).SendAsync("ReceiveMessage", userName, message, fileUrl);
            else
                await Clients.Group(courseId).SendAsync("ReceiveMessage", userName, message);
        }

        //Send in-app notification to a user
        public async Task SendNotification(string userId, string notification)
        {
            await Clients.Group(userId).SendAsync("ReceiveNotification", notification);
        }


        //Add user to a signalR group for their course
        public async Task JoinGroup(string courseId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, courseId);
        }


        //Remove user from a signalR group for their course
        public async Task LeaveGroup(string courseId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, courseId);
        }
    }
}
