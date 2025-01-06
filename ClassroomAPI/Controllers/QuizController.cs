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
                .Select(q => new
                {
                    q.QuizId,
                    q.Title,
                    q.CourseId,
                    q.CreatedAt,
                    q.ScheduledTime,
                    q.Deadline,
                    q.Duration
                })
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

            var returnQuiz = new
            {
                quizId = quiz.QuizId,
                title = quiz.Title,
                courseId = quiz.CourseId,
                createdAt = quiz.CreatedAt,
                scheduledTime = quiz.ScheduledTime,
                deadline = quiz.Deadline,
                duration = quiz.Duration
            };

            return Ok(returnQuiz);
        }

        //Create quiz
        [HttpPost("{courseId}/CreateQuiz")]
        public async Task<IActionResult> CreateQuiz(Guid courseId, [FromForm] string quizTitle, [FromForm] DateTime scheduledTime, [FromForm] int duration)
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
                Title = quizTitle,
                CreatedAt = DateTime.Now,
                ScheduledTime = scheduledTime,
                Deadline = scheduledTime.AddMinutes(duration+20),
                Duration = duration,
                CourseId = courseId,
                Course = course
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            //Schedule reminders and report generation
            _schedulingService.ScheduleQuizReminder(quiz.QuizId, quiz.ScheduledTime);
            _schedulingService.ScheduleReportGeneration(quiz.QuizId, quiz.Deadline);

            var returnQuiz = new
            {
                quizId = quiz.QuizId,
                title = quiz.Title,
                courseId = quiz.CourseId,
                createdAt = quiz.CreatedAt,
                scheduledTime = quiz.ScheduledTime,
                deadline = quiz.Deadline,
                duration = quiz.Duration
            };

            return Ok(returnQuiz);
        }

        //Update a quiz
        [HttpPut("{quizId}/UpdateQuiz")]
        public async Task<IActionResult> UpdateQuiz(Guid quizId, [FromForm] string quizTitle, [FromForm] DateTime scheduledTime, [FromForm] int duration)
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

            existingQuiz.Title = quizTitle;
            existingQuiz.ScheduledTime = scheduledTime;
            existingQuiz.Deadline = scheduledTime.AddMinutes(duration);
            existingQuiz.Duration = duration;

            await _context.SaveChangesAsync();

            var returnQuiz = new
            {
                quizId = existingQuiz.QuizId,
                title = existingQuiz.Title,
                courseId = existingQuiz.CourseId,
                createdAt = existingQuiz.CreatedAt,
                scheduledTime = existingQuiz.ScheduledTime,
                deadline = existingQuiz.Deadline,
                duration = existingQuiz.Duration
            };

            return Ok(returnQuiz);
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
            return Ok();
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
        public int duration { get; set; }
    }
}
