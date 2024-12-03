using ClassroomAPI.Data;
using ClassroomAPI.Migrations;
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
    [Authorize]
    public class AssignmentController : ControllerBase
    {
        private readonly ClassroomDbContext _context;
        private readonly FileService _fileService;
        public AssignmentController(ClassroomDbContext context, FileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        //Get all the assignments of a course
        [HttpGet("{courseId}/assignments")]
        public async Task<IActionResult> GetAllAssignments(Guid courseId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return NotFound("User Id not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
                return NotFound("Course not found!");

            var isMember = await _context.CourseMembers
                .Where(cm => cm.CourseId == courseId && cm.UserId == userId)
                .FirstOrDefaultAsync() != null;
            if (!isMember)
                return Unauthorized("You're not authorized!");

            var assignments = await _context.Assignments
                .Where(a => a.CourseId == course.CourseId)
                .ToListAsync();

            return Ok(assignments);
        }

        //Get an assignment by Id
        [HttpGet("{assignmentId}/GetAssignmentById")]
        public async Task<IActionResult> GetAssignmentById(Guid assignmentId)
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
                .Where(cm => cm.CourseId == course.CourseId && cm.UserId == userId)
                .FirstOrDefaultAsync() != null;
            if (!isMember)
                return Unauthorized("You're not authorized!");

            return Ok(assignment);
        }

        //Post an assignment
        [HttpPost("{courseId}/Post-Assignment")]
        public async Task<IActionResult> PostAssignment(Guid courseId, [FromBody] PostAssignmentModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if(course == null)
                return NotFound("Course not found!");

            if (course.AdminId != userId)
                return Unauthorized("You're not authorized!");

            if (model.file == null)
                return BadRequest("Upload a file!");

            var fileUrl = await _fileService.UploadFileAsync(model.file);

            var assignment = new Assignment
            {
                AssignmentId = Guid.NewGuid(),
                CourseId = courseId,
                Course = course,
                Text = model.text,
                AssignmentFileUrl = fileUrl
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();
            return Ok(assignment);
        }

        //Delete assignemnt
        [HttpDelete("{assignmentId}/DeleteAssignment")]
        public async Task<IActionResult> DeleteAssignment(Guid assignmentId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var assignment = await _context.Assignments.FirstOrDefaultAsync(a => a.AssignmentId == assignmentId);
            if (assignment == null)
                return NotFound("Assignment not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == assignment.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            if (course.AdminId != userId)
                return Unauthorized("You're not authorized!");

            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();
            return Ok(assignment);
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }

    public class PostAssignmentModel
    {
        public string text { get; set; } = string.Empty;
        public IFormFile? file { get; set; }
    }
}
