namespace ClassroomAPI.Models
{
    public class Participant
    {
        public Guid ParticipantId  { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public string MeetingName { get; set; } = string.Empty;
        public Guid MeetingId { get; set; }
        public Meeting? Meeting { get; set; }
        public bool hasLeft { get; set; } = false;
    }
}
