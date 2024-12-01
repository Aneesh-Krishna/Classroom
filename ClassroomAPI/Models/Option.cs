namespace ClassroomAPI.Models
{
    public class Option
    {
        public Guid OptionId { get; set; } = Guid.NewGuid();
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; } = false;
        public Guid QuestionId { get; set; }
        public Question? Question { get; set; }
    }
}
