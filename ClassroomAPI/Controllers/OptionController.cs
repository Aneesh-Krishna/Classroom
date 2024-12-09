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
    public class OptionController : ControllerBase
    {
        private readonly ClassroomDbContext _context;
        public OptionController(ClassroomDbContext context)
        {
            _context = context;
        }

        //Get all options of a question
        [HttpGet("{questionId}/GetAllOptions")]
        public async Task<IActionResult> GetAllOptions(Guid questionId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var question = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == questionId);
            if (question == null)
                return NotFound("Question not found!");

            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == question.QuizId);
            if (quiz == null)
                return NotFound("Quiz not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == quiz.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            var isMember = await _context.CourseMembers
                .Where(cm => cm.CourseId == course.CourseId && cm.UserId == userId)
                .FirstOrDefaultAsync() != null;
            if (!isMember)
                return Unauthorized("You're not authorized!");

            var options = await _context.Options
                .Where(o => o.QuestionId == questionId)
                .ToListAsync();

            return Ok(options);
        }

        //Get options by their Ids
        [HttpGet("{optionId}/GetOptionById")]
        public async Task<IActionResult> GetOptionById(Guid optionId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized("User Id not found!");

            var option = await _context.Options.FirstOrDefaultAsync(o => o.OptionId == optionId);
            if (option == null) return NotFound("Option not found!");

            var question = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == option.QuestionId);
            if (question == null) return NotFound("Question not found!");

            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == question.QuizId);
            if (quiz == null) return NotFound("Quiz not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == quiz.CourseId);
            if (course == null) return NotFound("Course not found!");

            var isMember = await _context.CourseMembers
                .Where(cm => cm.CourseId == course.CourseId && cm.UserId == userId)
                .FirstOrDefaultAsync() != null;
            if (!isMember) return Unauthorized("You're not authorized!");

            return Ok(option);
        }

        //Add option
        [HttpPost("{questionId}/AddOption")]
        public async Task<IActionResult> AddOption(Guid questionId, [FromBody] AddOptionModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var question = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == questionId);
            if (question == null)
                return NotFound("Question not found!");

            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == question.QuizId);
            if (quiz == null)
                return NotFound("Quiz not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == quiz.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            if (course.AdminId != userId)
                return Unauthorized("You're not authorized!");

            var option = new Option
            {
                OptionId = Guid.NewGuid(),
                Text = model.text,
                IsCorrect = model.isCorrect,
                QuestionId = questionId,
                Question = question
            };

            _context.Options.Add(option);
            await _context.SaveChangesAsync();
            return Ok(option);
        }

        //Update option
        [HttpPost("{optionId}/UpdateOption")]
        public async Task<IActionResult> UpdateOption(Guid optionId, [FromBody] AddOptionModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var existingOption = await _context.Options.FirstOrDefaultAsync(o => o.OptionId == optionId);
            if (existingOption == null)
                return NotFound("Option not found!");

            var question = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == existingOption.QuestionId);
            if (question == null)
                return NotFound("Question not found!");

            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == question.QuizId);
            if (quiz == null)
                return NotFound("Quiz not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == quiz.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            if (course.AdminId != userId)
                return Unauthorized("You're not authorized!");

            existingOption.Text = model.text;
            existingOption.IsCorrect = model.isCorrect;

            await _context.SaveChangesAsync();
            return Ok(existingOption);
        }

        //Delete option
        [HttpDelete("{optionId}/DeleteOption")]
        public async Task<IActionResult> DeleteOption(Guid optionId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var existingOption = await _context.Options.FirstOrDefaultAsync(o => o.OptionId == optionId);
            if (existingOption == null)
                return NotFound("Option not found!");

            var question = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == existingOption.QuestionId);
            if (question == null)
                return NotFound("Question not found!");

            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == question.QuizId);
            if (quiz == null)
                return NotFound("Quiz not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == quiz.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            if (course.AdminId != userId)
                return Unauthorized("You're not authorized!");

            _context.Options.Remove(existingOption);
            await _context.SaveChangesAsync();
            return Ok(existingOption);
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }

    public class AddOptionModel
    {
        public string text { get; set; } = string.Empty;
        public bool isCorrect { get; set; }
    }
}
