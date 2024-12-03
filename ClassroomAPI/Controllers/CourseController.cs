using ClassroomAPI.Data;
using ClassroomAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClassroomAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CourseController : ControllerBase
    {
        private readonly ClassroomDbContext _context;
        public CourseController(ClassroomDbContext context)
        {
            _context = context;
        }

        //Get courses of a user
        [HttpGet]
        public async Task<IActionResult> GetAllCourses()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return NotFound("User Id not found!");

            var courses = await _context.CourseMembers
                .Include(cm => cm.Course)
                .Where(cm => cm.UserId == userId)
                .Select(cm => cm.Course.CourseName)
                .ToListAsync();

            if (courses.Count == 0)
                return NotFound("You're not enrolled in any courses!");

            return Ok(courses);
        }

        //Get all the members of a course
        [HttpGet("{courseId}/members")]
        public async Task<IActionResult> GetAllMembers(Guid courseId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return NotFound("User Id not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
                return NotFound("Course not found!");

            var members = await _context.CourseMembers
                .Include(cm => cm.User)
                .Where(cm => cm.CourseId == course.CourseId)
                .Select(cm => cm.User.FullName)
                .ToListAsync();

            return Ok(members);
        }

        //Create a course
        [HttpPost("Create")]
        public async Task<IActionResult> CreateCourse([FromForm] string courseName, [FromForm] string description)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var admin = await _context.Users.FindAsync(userId);
            if (admin == null)
                return NotFound("User not found!");

            if (string.IsNullOrWhiteSpace(courseName))
                return BadRequest("Course name cannot be empty!");

            var course = new Course
            {
                CourseId = Guid.NewGuid(),
                CourseName = courseName,
                Description = description,
                AdminId = userId,
                GroupAdmin = admin,
                CreatedDate = DateTime.Now
            };

            var adminMember = new CourseMember
            {
                CourseMemberId = Guid.NewGuid(),
                CourseId = course.CourseId,
                Course = course,
                UserId = userId,
                User = admin,
                Role = "Teacher"
            };

            _context.Courses.Add(course);
            _context.CourseMembers.Add(adminMember);
            await _context.SaveChangesAsync();
            return Ok(course);
        }

        //Add members
        [HttpPost("{courseId}/AddMember")]
        public async Task<IActionResult> AddMember(Guid courseId, [FromBody] string newUserId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
                return NotFound("Course not found!");

            if (course.AdminId != userId)
                return Unauthorized("You're not authorized!");

            var newUser = await _context.Users.FindAsync(newUserId);
            if (newUser == null)
                return NotFound("No new user found!");

            var isMember = await _context.CourseMembers.FirstOrDefaultAsync(cm => cm.CourseId == courseId && cm.UserId == newUserId);
            if (isMember != null)
                return BadRequest("The user has already enrolled!");

            var newMember = new CourseMember
            {
                CourseMemberId = Guid.NewGuid(),
                CourseId = course.CourseId,
                Course = course,
                UserId = userId,
                User = newUser
            };

            _context.CourseMembers.Add(newMember);
            await _context.SaveChangesAsync();
            return Ok(newMember);
        }

        //Remove a member
        [HttpPost("{memberId}/RemoveMember")]
        public async Task<IActionResult> RemoveMember(Guid memberId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var course = await _context.CourseMembers
                .Where(cm => cm.CourseMemberId == memberId)
                .Select(cm => cm.Course)
                .FirstOrDefaultAsync();
            if (course == null)
                return NotFound("Course not found!");

            var userMember = await _context.CourseMembers.FirstOrDefaultAsync(cm => cm.CourseMemberId == memberId);
            if (userMember == null)
                return NotFound("User is not a member of the course!");

            if (course.AdminId != userId || userMember.UserId != userId)
                return Unauthorized("You're not authorized!");

            _context.CourseMembers.Remove(userMember);
            await _context.SaveChangesAsync();
            return Ok(userMember + " has been removed!");
        }

        //Update a course's details
        [HttpPut("{courseId}/UpdateCourse")]
        public async Task<IActionResult> UpdateCourse(Guid courseId, string courseName, string description)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var existingCourse = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (existingCourse == null)
                return BadRequest("Course not found!");

            if (existingCourse.AdminId != userId)
                return Unauthorized("You're not authorized!");

            if (string.IsNullOrWhiteSpace(courseName))
                return BadRequest("Course name cannot be empty!");

            existingCourse.CourseName = courseName;
            existingCourse.Description = description;
            await _context.SaveChangesAsync();
            return Ok(existingCourse);
        }

        //Delete course
        [HttpDelete("{courseId}")]
        public async Task<IActionResult> DeleteCourse(Guid courseId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
                return NotFound("Course not found!");

            if (course.AdminId != userId)
                return Unauthorized("You're not authorized!");

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return Ok(course.CourseName +" is deleted!");
        }


        //Get current user's Id
        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
