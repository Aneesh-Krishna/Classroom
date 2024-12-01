using System.ComponentModel.DataAnnotations;

namespace ClassroomAPI.Models
{
    public class Course
    {
        public Guid CourseId { get; set; } = Guid.NewGuid();
        [Required]
        [MaxLength(100)]
        public string CourseName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AdminId { get; set; } = string.Empty; //AdminId
        public ApplicationUser? GroupAdmin { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public ICollection<CourseMember> CourseMembers { get; set; } = new List<CourseMember>();
        public ICollection<Chat> Chats { get; set; } = new List<Chat>();
        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    }
}
