using Hangfire;

namespace ClassroomAPI.Services
{
    public class SchedulingService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        public SchedulingService(IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
        }
        public void ScheduleReminder(Guid quizId, DateTime scheduledTime)
        {
            var reminderTime = scheduledTime.AddMinutes(-1);
            _backgroundJobClient.Schedule(() => SendReminder(quizId), reminderTime);
        }

        public void ScheduleReportGeneration(Guid quizId, DateTime deadline)
        {
            var reportTime = deadline.AddMinutes(1);
            _backgroundJobClient.Schedule(() => GenerateReport(quizId), reportTime);
        }

        public void SendReminder(Guid quizId)
        {
            //Logic to send a reminder to every member
        }

        public void GenerateReport(Guid quizId)
        {
            //Logic to generate and send report
        }
    }
}
