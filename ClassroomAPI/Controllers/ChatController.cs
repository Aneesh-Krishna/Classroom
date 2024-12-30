using ClassroomAPI.Data;
using ClassroomAPI.Hubs;
using ClassroomAPI.Models;
using ClassroomAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClassroomAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ClassroomDbContext _context;
        private readonly FileService _fileService;
        private readonly IHubContext<ChatHub> _hubContext;
        public ChatController(IHubContext<ChatHub> hubContext, ClassroomDbContext context, FileService fileService)
        {
            _hubContext = hubContext;
            _context = context;
            _fileService = fileService;
        }

        //Get all chats
        [HttpGet("{courseId}/GetAllChats")]
        public async Task<IActionResult> GetAllChats(Guid courseId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("User Id not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
                return NotFound("Course not found!");

            var isMember = await _context.CourseMembers
                .Where(cm => cm.CourseId == courseId && cm.UserId == userId)
                .FirstOrDefaultAsync() != null;
            if (!isMember)
                return Unauthorized("You're not authorized!");

            var chats = await _context.Chats
                .Where(c => c.CourseId == courseId)
                .Select(c => new
                {
                    c.ChatId,
                    c.UserId,
                    c.CourseId,
                    c.Message,
                    c.FileName,
                    c.FileUrl,
                    c.SenderName,
                    c.SentAt
                })
                .OrderBy(c => c.SentAt)
                .ToListAsync();

            return Ok(chats);
        }

        //Send a message
        [HttpPost("{courseId}/SendMessage")]
        public async Task<IActionResult> SendMessage(Guid courseId, [FromForm] ChatSubmissionModel model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized("User Id not found!");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return BadRequest("User not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
                return NotFound("Course not found!");

            string fileUrl;
            string fileName;
            if (model.file != null)
            {
                fileName = model.file.FileName;
                fileUrl = await _fileService.UploadFileAsync(model.file);
            }
            else
            {
                fileName = "";
                fileUrl = "";
            }

            var chat = new Chat
            {
                ChatId = Guid.NewGuid(),
                CourseId = courseId,
                Course = course,
                UserId = userId,
                SenderName = user.FullName,
                User = user,
                SentAt = DateTime.Now,
                Message = model.message,
                FileName = fileName,
                FileUrl = fileUrl
            };

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            var realTimeChat = await _context.Chats
                .Where(c => c.ChatId == chat.ChatId)
                .Select(c => new ChatDto
                {
                    ChatId = c.ChatId,
                    CourseId = c.CourseId,
                    CourseName = course.CourseName,
                    UserId = c.UserId,
                    SenderName = c.SenderName,
                    SentAt = c.SentAt,
                    Message = c.Message,
                    FileName = c.FileName,
                    FileUrl = c.FileUrl
                })
                .SingleOrDefaultAsync();

            // Notify via SignalR
            await _hubContext.Clients.Group(courseId.ToString()).SendAsync("ReceiveMessage", realTimeChat);

            return Ok("Message sent");
        }

        [HttpPost("SendNotification")]
        public async Task<IActionResult> SendNotification(string userId, string notification)
        {
            await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", notification);
            return Ok("Notification sent.");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty");
            }

            var fileUrl = await _fileService.UploadFileAsync(file);
            return Ok(new { Url = fileUrl });
        }

    }

    public class ChatSubmissionModel
    {
        public string message { get; set; } = string.Empty;
        public IFormFile? file { get; set; }
    }

    public class ChatDto
    {
        public Guid ChatId { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; }
        public string UserId { get; set; }
        public string SenderName { get; set; }
        public DateTime SentAt { get; set; }
        public string Message { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
    }

}
