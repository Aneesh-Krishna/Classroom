namespace ClassroomAPI.Models
{
    public class QuizResponse
    {
        public Guid QuizResponseId { get; set; } = Guid.NewGuid();
        public Guid QuizId { get; set; }
        public Quiz? Quiz { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public int Score { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
        public ICollection<Answer> Answers { get; set; } = new List<Answer>();  
    }
}
