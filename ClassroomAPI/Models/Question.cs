using System.ComponentModel.DataAnnotations;

namespace ClassroomAPI.Models
{
    public class Question
    {
        public Guid QuestionId { get; set; } = Guid.NewGuid();
        public Guid QuizId { get; set; }
        public Quiz? Quiz { get; set; }
        [Required]
        public string Text { get; set; } = string.Empty;
        [Required]
        public int Difficulty { get; set; }

        public int Points { get; set; } = 1;
        public ICollection<Option> Options { get; set; } = null!;
    }
}
