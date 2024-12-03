namespace ClassroomAPI.Services
{
    public class OnlineUserTracker
    {
        private static readonly HashSet<string> OnlineUsers = new();

        public static void SetUserOnline(string UserId)
        {
            OnlineUsers.Add(UserId);
        }

        public static void SetUserOffline(string UserId)
        {
            OnlineUsers.Remove(UserId);
        }

        public static bool IsUserOnline(string UserId)
        {
            return OnlineUsers.Contains(UserId);
        }
    }
}
