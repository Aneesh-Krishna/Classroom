namespace ClassroomAPI.Models
{
    public class Chat
    {
        public Guid ChatId { get; set; } = Guid.NewGuid();
        public Guid CourseId { get; set; }
        public Course? Course { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;

        public string Message { get; set; } = string.Empty;
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
    }
}
