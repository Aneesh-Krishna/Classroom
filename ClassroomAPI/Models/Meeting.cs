namespace ClassroomAPI.Models
{
    public class Meeting
    {
        public Guid MeetingId { get; set; } = Guid.NewGuid();
        public string MeetingName { get; set; } = string.Empty;
        public Guid CourseId {  get; set; }
        public Course? Course { get; set; }
        public bool hasEnded { get; set; } = false;
        public ICollection<Participant> Participants { get; set; } = new List<Participant>();
    }
}
