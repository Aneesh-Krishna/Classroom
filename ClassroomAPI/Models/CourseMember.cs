namespace ClassroomAPI.Models
{
    public class CourseMember
    {
        public Guid CourseMemberId { get; set; } = Guid.NewGuid();
        public Guid CourseId { get; set; }
        public Course? Course { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public string Role { get; set; } = "Student";
    }
}
