using Microsoft.AspNetCore.SignalR;

namespace ClassroomAPI.Hubs
{
    public class MeetingHub : Hub
    {
        public async Task JoinMeeting(Guid meetingId, string userId, string userName)
        {
            await Groups.AddToGroupAsync(userId, meetingId.ToString());
            await Clients.Group(meetingId.ToString()).SendAsync("UserJoined", userName);
        }

        public async Task LeaveMeeting(Guid meetingId, string userId, string userName)
        {
            await Groups.RemoveFromGroupAsync(userId, meetingId.ToString());
            await Clients.Group(meetingId.ToString()).SendAsync("UserLeft", userName);
        }

        public async Task SendMessage(Guid meetingId, string message)
        {
            await Clients.Groups(meetingId.ToString()).SendAsync("ReceiveMessage", message);
        }
    }
}
