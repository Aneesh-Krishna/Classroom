namespace ClassroomAPI.Models
{
    public class Report
    {
        public Guid ReportId { get; set; } = Guid.NewGuid();
        public string reportUrl { get; set; } = string.Empty;
        public Guid QuizId { get; set; }
        public Quiz? Quiz { get; set; }
    }
}
