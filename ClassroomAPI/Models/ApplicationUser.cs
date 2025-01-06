using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ClassroomAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        public Roles Role { get; set; }
        public ICollection<CourseMember> CourseMemberships { get; set; } = new List<CourseMember>();
        public ICollection<Course> CourseAdmin { get; set; } = new List<Course>();
        public ICollection<QuizResponse> QuizResponses { get; set; } = new List<QuizResponse>();
        public ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();
        public ICollection<Chat> Chats { get; set; } = new List<Chat>();
        public ICollection<Participant> MeetingParticipants { get; set; } = new List<Participant>();
    }

    public enum Roles
    {
        Admin,
        User
    }
}
