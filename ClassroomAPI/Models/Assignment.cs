namespace ClassroomAPI.Models
{
    public class Assignment
    {
        public Guid AssignmentId { get; set; } = Guid.NewGuid();
        public string Text { get; set; } = string.Empty;
        public string AssignmentFileName { get; set; } = string.Empty;
        public string AssignmentFileUrl { get; set; } = string.Empty;
        public Guid CourseId { get; set; }
        public Course? Course { get; set; }

        public ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();
    }
}
