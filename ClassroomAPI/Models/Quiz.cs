using System.ComponentModel.DataAnnotations;

namespace ClassroomAPI.Models
{
    public class Quiz
    {
        public Guid QuizId { get; set; } = Guid.NewGuid();
        public Guid CourseId { get; set; }
        public Course? Course { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Required]
        public DateTime ScheduledTime { get; set; }
        public int Duration { get; set; } //Duration of the quiz in minutes
        public DateTime Deadline { get; set; }

        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<QuizResponse> QuizResponses { get; set; } = new List<QuizResponse>();
    }
}
