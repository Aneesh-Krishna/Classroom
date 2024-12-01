namespace ClassroomAPI.Models
{
    public class Answer
    {
        public Guid AnswerId { get; set; } = Guid.NewGuid();
        public Guid QuestionId { get; set; }
        public Question? Question { get; set; }
        public Guid OptionId { get; set; }   
        public Option? Option { get; set; }
        public Guid QuizResponseId { get; set; }
        public QuizResponse? QuizResponse { get; set; }
    }
}
