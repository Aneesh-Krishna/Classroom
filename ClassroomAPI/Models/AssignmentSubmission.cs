namespace ClassroomAPI.Models
{
    public class AssignmentSubmission
    {
        public Guid AssignmentSubmissionId { get; set; } = Guid.NewGuid();
        public string Text { get; set; } = string.Empty;
        public string SubmissionFileUrl { get; set; } = string.Empty;
        public Guid AssignmentId { get; set; }
        public Assignment? Assignment { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? SubmittedBy { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
