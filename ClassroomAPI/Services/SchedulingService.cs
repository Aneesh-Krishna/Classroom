using ClassroomAPI.Data;
using ClassroomAPI.Hubs;
using ClassroomAPI.Models;
using Hangfire;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Amazon.S3;
using Amazon.S3.Model;

namespace ClassroomAPI.Services
{
    public class SchedulingService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<ChatHub> _chatHubContext;
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        public SchedulingService(IBackgroundJobClient backgroundJobClient, IServiceProvider serviceProvider, IHubContext<ChatHub> chatHubContext, IConfiguration configuration)
        {
            _backgroundJobClient = backgroundJobClient;
            _serviceProvider = serviceProvider;
            _chatHubContext = chatHubContext;
            _bucketName = configuration["AWS:S3BucketName"];
            _s3Client = new AmazonS3Client(configuration["AWS:AccessKey"], configuration["AWS:SecretKey"], Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"]));
        }
        public void ScheduleQuizReminder(Guid quizId, DateTime scheduledTime)
        {
            var reminderTime = scheduledTime.AddMinutes(-1);
            _backgroundJobClient.Schedule(() => SendReminder(quizId), reminderTime);
        }

        public void ScheduleReportGeneration(Guid quizId, DateTime deadline)
        {
            var reportTime = deadline.AddMinutes(1);
            _backgroundJobClient.Schedule(() => GenerateReport(quizId), reportTime);
        }

        public async Task SendReminder(Guid quizId)
        {
            //Logic to send a reminder to every member
            using (var scope = _serviceProvider.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ClassroomDbContext>();
                var quiz = _context.Quizzes
                    .Include(q => q.Course)
                    .ThenInclude(c => c.CourseMembers)
                    .ThenInclude(cm => cm.User)
                    .FirstOrDefault(q => q.QuizId == quizId);

                if (quiz != null) 
                {
                    var members = await _context.CourseMembers
                        .Where(cm => cm.CourseId == quiz.CourseId)
                        .ToListAsync();
                    
                    if (members == null)
                        return;

                    foreach (var member in members)
                    { 
                        var message = $"Reminder: The quiz '{quiz.Title}' is starting in 1 minute.";
                        await SendNotification(member.UserId, message); 
                    }
                }
            }
        }

        //Logic to generate and send report
        public async Task GenerateReport(Guid quizId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ClassroomDbContext>();

                var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == quizId);
                if (quiz == null)
                    return;
                // Fetch group and quiz data
                var course = await _context.Courses
                    .Include(c => c.CourseMembers)
                    .ThenInclude(gm => gm.User)
                    .FirstOrDefaultAsync(c => c.CourseId == quiz.CourseId);
                if (course == null)
                {
                    Console.WriteLine("Group not found!");
                    return;
                }

                using (var memoryStream = new MemoryStream())
                {
                    var document = new Document();
                    PdfWriter.GetInstance(document, memoryStream).CloseStream = false;
                    document.Open();
                    document.Add(new Paragraph("Quiz Scores Report"));

                    var Members = await _context.CourseMembers
                        .Where(cm => cm.CourseId == course.CourseId)
                        .ToListAsync();
                    if (Members == null)
                    {
                        Console.WriteLine("No group members!");
                        return;
                    }

                    foreach (var member in Members)
                    {
                        var user = await _context.Users.FindAsync(member.UserId);
                        if (user == null)
                        {
                            continue;
                        }
                        var response = quiz.QuizResponses.FirstOrDefault(qr => qr.UserId == member.UserId);
                        document.Add(new Paragraph($"User: {user.FullName}, Score: {response?.Score ?? 0}, Status: {(response == null ? "Absent" : "Present")}"));
                    }

                    document.Close();
                    memoryStream.Position = 0;

                    // Upload the report to a file server or blob storage (example: Azure Blob Storage)
                    var fileName = $"{quiz.Title}_Report.pdf";
                    var reportUrl = await UploadReportToS3Storage(memoryStream, fileName);

                    if (!string.IsNullOrEmpty(reportUrl))
                    {
                        var reportMessage = new Chat
                        {
                            ChatId = Guid.NewGuid(),
                            Message = "Quiz-Report",
                            FileUrl = reportUrl,
                            CourseId = course.CourseId,
                            Course = course,
                            UserId = course.AdminId,
                            User = course.GroupAdmin
                        };

                        _context.Chats.Add(reportMessage);
                        await _context.SaveChangesAsync();

                        // Notify the group with the report URL
                        await _chatHubContext.Clients.Group(course.CourseId.ToString()).SendAsync("ReceiveReport", reportUrl);
                    }
                    else
                    {
                        Console.WriteLine("Failed to upload report to storage.");
                    }
                }
            }
        }

        private async Task<string> UploadReportToS3Storage(Stream reportStream, string fileName)
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                InputStream = reportStream,
                ContentType = "application/pdf",
            };

            await _s3Client.PutObjectAsync(putRequest);
            return $"https://{_bucketName}.s3.amazonaws.com/{fileName}";
        }

            private async Task SendNotification(string userId, string message) 
        { 
            await _chatHubContext.Clients.Group(userId).SendAsync("ReceiveNotification", message);
        }
    }
}
