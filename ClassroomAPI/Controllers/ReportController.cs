using ClassroomAPI.Data;
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
    public class ReportController : ControllerBase
    {
        private readonly ClassroomDbContext _context;
        private readonly SchedulingService _schedule;
        private readonly ReportService _reportService;
        public ReportController(ClassroomDbContext context, SchedulingService schedule, ReportService reportService)
        {
            _context = context;
            _schedule = schedule;   
            _reportService = reportService;
        }

        [HttpGet("{quizId}/reports")]
        public async Task<IActionResult> GetAllReports(Guid quizId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(userId == null)
                return NotFound("User Id not found!");

            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == quizId);
            if (quiz == null)
                return BadRequest("Quiz not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == quiz.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            if (course.AdminId != userId)
                return Unauthorized("You're not authorized!");

            var reports = await _context.Reports
                .Where(r => r.QuizId == quiz.QuizId)
                .ToListAsync();
            if (reports == null)
                return NotFound("No reports for this quiz!");

            return Ok(reports);
        }

        [HttpPost("{quizId}/GenerateReport")]
        public async Task<IActionResult> GenerateReport(Guid quizId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(userId == null)
                return NotFound("User Id not found!");

            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == quizId);
            if (quiz == null)
                return BadRequest("Quiz not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == quiz.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            if (course.AdminId != userId)
                return Unauthorized("You're not authorized!");

            await _reportService.GenerateReport(quizId);
            return Ok("Report generated and sent successfully!");
        }
    }
}
