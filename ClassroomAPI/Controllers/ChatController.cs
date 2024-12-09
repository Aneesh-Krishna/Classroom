using ClassroomAPI.Data;
using ClassroomAPI.Hubs;
using ClassroomAPI.Models;
using ClassroomAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClassroomAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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

            string? fileUrl = null;
            if (model.file != null)
            {
                fileUrl = await _fileService.UploadFileAsync(model.file);
            }

            var chat = new Chat
            {
                ChatId = Guid.NewGuid(),
                CourseId = courseId,
                Course = course,
                UserId = userId,
                User = user,
                SentAt = DateTime.Now,
                Message = model.message,
                FileUrl = fileUrl
            };

            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            // Notify via SignalR
            await _hubContext.Clients.Group(courseId.ToString()).SendAsync("ReceiveMessage", chat.User.FullName, chat.Message, chat.FileUrl);

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
}
