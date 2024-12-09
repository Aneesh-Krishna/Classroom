using ClassroomAPI.Data;
using ClassroomAPI.Models;
using ClassroomAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClassroomAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentSubmissionController : ControllerBase
    {
        private readonly ClassroomDbContext _context;
        private readonly FileService _fileService;
        public AssignmentSubmissionController(ClassroomDbContext context, FileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        //Get all submissions of an assingment
        [HttpGet("{assignmentId}/GetAllAssignment-Submissions")]
        public async Task<IActionResult> GetAllSubmissions(Guid assignmentId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return NotFound("User Id not found!");

            var assignment = await _context.Assignments.FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);
            if (assignment == null)
                return NotFound("Assignment not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == assignment.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            var isMember = await _context.CourseMembers
                .Where(cm => cm.CourseId == assignment.CourseId && cm.UserId == userId)
                .FirstOrDefaultAsync() != null;
            if (!isMember)
                return Unauthorized("You're not authorized!");

            var submissions = await _context.AssignmentSubmissions
                .Where(asb => asb.AssignmentId == assignment.AssignmentId)
                .ToListAsync();
            return Ok(submissions);
        }

        //Submit an assignment
        [HttpPost("{assignmentId}/SubmitAssignment")]
        public async Task<IActionResult> SubmitAssignment(Guid assignmentId, [FromForm] string text, IFormFile? file)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found!");

            var assignment = await _context.Assignments.FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);
            if (assignment == null) 
                return NotFound("Assignment not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == assignment.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            var isMember = await _context.CourseMembers
                .Where(cm => cm.CourseId == assignment.CourseId && cm.UserId == userId)
                .FirstOrDefaultAsync() != null;

            if (!isMember)
                return Unauthorized("You're not authorized!");

            if (file == null)
                return BadRequest("Upload a file!");

            var fileUrl = await _fileService.UploadFileAsync(file);

            var submission = new AssignmentSubmission
            {
                AssignmentSubmissionId = Guid.NewGuid(),
                AssignmentId = assignmentId,
                Assignment = assignment,
                SubmittedBy = user,
                Text = text,
                SubmissionFileUrl = fileUrl,
                SubmittedAt = DateTime.Now
            };

            _context.AssignmentSubmissions.Add(submission);
            await _context.SaveChangesAsync();
            return Ok(submission);
        }

        
        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
