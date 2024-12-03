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
    [Authorize]
    public class QuizController : ControllerBase
    {
        private readonly ClassroomDbContext _context;
        private readonly SchedulingService _schedulingService;
        public QuizController(ClassroomDbContext context, SchedulingService schedulingService)
        {
            _context = context;
            _schedulingService = schedulingService;
        }

        //Get all quizzes of a course
        [HttpGet("{courseId}/GetAllQuiz")]
        public async Task<IActionResult> GetAllQuizzes(Guid courseId)
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

            var quizzes = await _context.Quizzes
                .Where(q => q.CourseId == courseId)
                .ToListAsync();

            return Ok(quizzes);
        }

        //Get quiz by Id
        [HttpGet("{quizId}/GetQuiz")]
        public async Task<IActionResult> GetQuizById(Guid quizId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return NotFound("User Id not found!");

            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == quizId);
            if (quiz == null)
                return NotFound("Return not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == quiz.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            var isMember = await _context.CourseMembers
                .Where(cm => cm.CourseId == course.CourseId && cm.UserId == userId)
                .FirstOrDefaultAsync() != null;
            if (!isMember)
                return Unauthorized("You're not authorized!");

            return Ok(quiz);
        }

        //Create quiz
        [HttpPost("{courseId}/CreateQuiz")]
        public async Task<IActionResult> CreateQuiz(Guid courseId, [FromBody] CreateQuizModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
                return NotFound("Course not found!");

            if (course.AdminId != userId)
                return Unauthorized("You're not authorized!");

            var quiz = new Quiz
            {
                QuizId = Guid.NewGuid(),
                Title = model.title,
                CreatedAt = DateTime.Now,
                ScheduledTime = model.scheduledTime,
                Deadline = model.deadline,
                Duration = model.duration,
                CourseId = courseId,
                Course = course
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            //Schedule reminders and report generation
            _schedulingService.ScheduleQuizReminder(quiz.QuizId, quiz.ScheduledTime);
            _schedulingService.ScheduleReportGeneration(quiz.QuizId, quiz.Deadline);

            return Ok(quiz);
        }

        //Update a quiz
        [HttpPut("{quizId}/UpdateQuiz")]
        public async Task<IActionResult> UpdateQuiz(Guid quizId, [FromBody] CreateQuizModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == null) 
                return Unauthorized("User Id not found!");

            var existingQuiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == quizId);
            if (existingQuiz == null)
                return NotFound("Quiz not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == existingQuiz.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            if (course.AdminId != userId)
                return Unauthorized("You're not authorized!");

            existingQuiz.Title = model.title;
            existingQuiz.ScheduledTime = model.scheduledTime;
            existingQuiz.Deadline = model.deadline;
            existingQuiz.Duration = model.duration;

            await _context.SaveChangesAsync();
            return Ok(existingQuiz);
        }

        //Delete a quiz
        [HttpDelete("{quizId}/DeleteQuiz")]
        public async Task<IActionResult> DeleteQuiz(Guid quizId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var existingQuiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == quizId);
            if (existingQuiz == null)
                return NotFound("Quiz not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == existingQuiz.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            if (course.AdminId != userId)
                return Unauthorized("You're not authorized!");

            _context.Quizzes.Remove(existingQuiz);
            await _context.SaveChangesAsync();
            return Ok(existingQuiz);
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }

    public class CreateQuizModel
    {
        public string title { get; set; } = string.Empty;
        public DateTime scheduledTime { get; set; }
        public DateTime deadline { get; set; }
        public int duration { get; set; }
    }
}
