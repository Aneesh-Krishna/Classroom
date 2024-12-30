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
using Microsoft.AspNetCore.Http.HttpResults;

namespace ClassroomAPI.Services
{
    public class ReportService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<ChatHub> _chatHubContext;
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        public ReportService(IBackgroundJobClient backgroundJobClient, IServiceProvider serviceProvider, IHubContext<ChatHub> chatHubContext, IConfiguration configuration)
        {
            _backgroundJobClient = backgroundJobClient;
            _serviceProvider = serviceProvider;
            _chatHubContext = chatHubContext;
            _bucketName = configuration["AWS:S3BucketName"];
            _s3Client = new AmazonS3Client(configuration["AWS:AccessKey"], configuration["AWS:SecretKey"], Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"]));
        }

        public async Task GenerateReport(Guid quizId)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ClassroomDbContext>();

                var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == quizId);
                if (quiz == null)
                {
                    Console.WriteLine("Quiz not found!");
                    return;
                }
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == quiz.CourseId);
                if (course == null)
                {
                    Console.WriteLine("Course not found!");
                    return;
                }

                var adminName = await _context.Users
                    .Where(u => u.Id == course.AdminId)
                    .Select(u => u.FullName)
                    .SingleOrDefaultAsync();
                if (adminName == null)
                    adminName = "Instructor";

                using (var memoryStream = new MemoryStream())
                {
                    var document = new Document();
                    PdfWriter.GetInstance(document, memoryStream).CloseStream = false;
                    document.Open();
                    document.Add(new Paragraph("Quiz Report"));

                    var members = await _context.CourseMembers
                        .Where(cm => cm.CourseId == quiz.CourseId)
                        .ToListAsync();
                    if (members == null)
                    {
                        Console.WriteLine("No members in the course!");
                        return;
                    }
                    foreach (var member in members)
                    {
                        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == member.UserId);
                        if (user == null)
                            continue;

                        var response = await _context.QuizResponses
                            .FirstOrDefaultAsync(qr => qr.QuizId == quiz.QuizId && qr.UserId == user.Id);

                        document.Add(new Paragraph($"User: {user.FullName}, Score: {response?.Score ?? 0}, Status: {(response == null ? "Absent" : "Present")}"));

                    }

                    document.Close();
                    memoryStream.Position = 0;

                    var fileName = $"{quiz.Title}_Report.pdf";
                    var reportUrl = await UploadReportToS3Storage(memoryStream, fileName);

                    if (!string.IsNullOrEmpty(reportUrl))
                    {
                        var reportMessage = new Chat
                        {
                            ChatId = Guid.NewGuid(),
                            Message = "Quiz-Report",
                            FileUrl = reportUrl,
                            FileName = fileName,
                            CourseId = course.CourseId,
                            Course = course,
                            UserId = course.AdminId,
                            User = course.GroupAdmin,
                            SenderName = adminName,
                            SentAt = DateTime.Now
                        };

                        _context.Chats.Add(reportMessage);

                        var newReport = new Report
                        {
                            ReportId = Guid.NewGuid(),
                            reportUrl = reportUrl,
                            QuizId = quiz.QuizId,
                            Quiz = quiz
                        };
                        _context.Reports.Add(newReport);

                        quiz.isReportGenerated = true;

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
    }
}
