using ClassroomAPI.Data;
using ClassroomAPI.Models;
using ClassroomAPI.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ClassroomAPI.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ClassroomDbContext _context;
        public ChatHub(ClassroomDbContext context)
        {
            _context = context;
        }
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var courseId = httpContext.Request.Query["courseId"];

            if (!string.IsNullOrEmpty(courseId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, courseId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var UserId = Context.UserIdentifier;
            if (UserId != null)
            {
                OnlineUserTracker.SetUserOffline(UserId);

                var userCourses = await _context.CourseMembers
                    .Where(cm => cm.UserId == UserId)
                    .Select (cm => cm.CourseId.ToString())
                    .ToListAsync();

                foreach(var courseId in userCourses)
                    await LeaveGroup(courseId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        //Send message to a specific course
        public async Task SendMessage( string courseId, object realTimeChat)
        {
                await Clients.Group(courseId).SendAsync("ReceiveMessage", realTimeChat);
        }

        //Send in-app notification to a user
        public async Task SendNotification(string courseId, string notification)
        {
            await Clients.Group(courseId).SendAsync("ReceiveNotification", notification);
        }


        //Add user to a signalR group for their course
        public async Task JoinGroup(string courseId)
        {
            try
            {
                Console.WriteLine($"User {Context.ConnectionId} is attempting to join group: {courseId}");
                await Groups.AddToGroupAsync(Context.ConnectionId, courseId);
                Console.WriteLine($"User {Context.ConnectionId} successfully joined group: {courseId}");
                await Clients.Caller.SendAsync("GroupJoined", $"You have joined the course: {courseId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in JoinGroup: {ex.Message}");
            }
        }


        //Remove user from a signalR group for their course
        public async Task LeaveGroup(string courseId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, courseId);
        }
    }
}
