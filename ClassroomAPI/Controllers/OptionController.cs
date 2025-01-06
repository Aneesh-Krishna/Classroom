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
                .Select(o => new
                {
                    o.OptionId,
                    o.Text,
                    o.QuestionId,
                    o.IsCorrect
                })
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

            var returnOption = new
            {
                option.OptionId,
                option.QuestionId,
                option.Text,
                option.IsCorrect
            };

            return Ok(returnOption);
        }

        //Add option
        [HttpPost("{questionId}/AddOption")]
        public async Task<IActionResult> AddOption(Guid questionId, [FromForm] string text, [FromForm] bool isCorrect)
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
                Text = text,
                IsCorrect = isCorrect,
                QuestionId = questionId,
                Question = question
            };

            _context.Options.Add(option);
            await _context.SaveChangesAsync();

            var returnOption = new
            {
                option.OptionId,
                option.QuestionId,
                option.Text,
                option.IsCorrect
            };

            return Ok(returnOption);
        }

        //Update option
        [HttpPost("{optionId}/UpdateOption")]
        public async Task<IActionResult> UpdateOption(Guid optionId, [FromForm] string text, [FromForm] bool isCorrect)
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

            existingOption.Text = text;
            existingOption.IsCorrect = isCorrect;

            await _context.SaveChangesAsync();

            var returnOption = new
            {
                existingOption.OptionId,
                existingOption.QuestionId,
                existingOption.Text,
                existingOption.IsCorrect
            };

            return Ok(returnOption);
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
            return Ok();
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
