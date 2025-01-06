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
    public class QuestionController : ControllerBase
    {
        private readonly ClassroomDbContext _context;
        public QuestionController(ClassroomDbContext context)
        {
            _context = context;
        }

        //Get all questions of a quiz
        [HttpGet("{quizId}/GetAllQuestions")]
        public async Task<IActionResult> GetAllQuestions(Guid quizId)
        {
            var userId = GetCurrentUserId();
            if(userId == null)
                return NotFound("User Id not found!");

            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == quizId);
            if (quiz == null)
                return NotFound("Quiz not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c  => c.CourseId == quiz.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            var isMember = await _context.CourseMembers
                .Where(cm => cm.CourseId == course.CourseId && cm.UserId == userId)
                .FirstOrDefaultAsync() != null;
            if (!isMember)
                return Unauthorized("You're not authorized!");

            var questions = await _context.Questions
                .Where(q => q.QuizId == quizId)
                .Select(q => new
                {
                    q.QuestionId,
                    q.Text,
                    q.QuizId,
                    q.Difficulty,
                    q.Points
                })
                .ToListAsync();

            return Ok(questions);
        }

        //Get all the questions of a quiz with their options
        [HttpGet("{quizId}/GetAllQuestionsWithOptions")]
        public async Task<IActionResult> GetAllQuestionsWithOptions(Guid quizId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return NotFound("User Id not found!");

            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == quizId);
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

            var questionsWithOptions = await _context.Questions
                .Where(q => q.QuizId == quizId)
                .Select(q => new
                {
                    q.QuestionId,
                    q.Text,
                    q.QuizId,
                    q.Difficulty,
                    q.Points,
                    Options = _context.Options
                        .Where(o => o.QuestionId == q.QuestionId)
                        .Select(o => new
                        {
                            o.OptionId,
                            o.Text,
                            o.IsCorrect
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(questionsWithOptions);
        }

        //Get question by id
        [HttpGet("{questionId}/GetQuestionById")]
        public async Task<IActionResult> GetQuestionById(Guid questionId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return NotFound("User Id not found!");

            var question = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == questionId);
            if (question == null)
                return NotFound("Question not found!");

            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == question.QuestionId);
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

            var returnQuestion = new
            {
                question.QuestionId,
                question.QuizId,
                question.Text,
                question.Difficulty,
                question.Points
            };

            return Ok(returnQuestion);
        }

        //Add a question
        [HttpPost("{quizId}/AddQuestion")]
        public async Task<IActionResult> AddQuestion(Guid quizId, [FromForm] string text, [FromForm] int difficulty, [FromForm] int points)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == quizId);
            if (quiz == null)
                return NotFound("Quiz not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == quiz.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            if (course.AdminId != userId)
                return Unauthorized("You're not authorized!");

            var question = new Question
            {
                QuestionId = Guid.NewGuid(),
                QuizId = quizId,
                Quiz = quiz,
                Text = text,
                Difficulty = difficulty,
                Points = points
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            var returnQuestion = new
            {
                question.QuestionId,
                question.QuizId,
                question.Text,
                question.Difficulty,
                question.Points
            };

            return Ok(returnQuestion);
        }

        //Update a question
        [HttpPut("{questionId}/UpdateQuestion")]
        public async Task<IActionResult> UpdateQuestion(Guid questionId, [FromForm] string text, [FromForm] int difficulty, [FromForm] int points)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("User Id not found!");

            var existingQuestion = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == questionId);
            if (existingQuestion == null)
                return NotFound("Question not found!");

            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == existingQuestion.QuizId);
            if (quiz == null)
                return NotFound("Quiz not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == quiz.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            if (course.AdminId != userId)
                return Unauthorized("You're not authorized!");

            existingQuestion.Text = text;
            existingQuestion.Difficulty = difficulty;
            existingQuestion.Points = points;

            await _context.SaveChangesAsync();

            var returnQuestion = new
            {
                existingQuestion.QuestionId,
                existingQuestion.QuizId,
                existingQuestion.Text,
                existingQuestion.Difficulty,
                existingQuestion.Points
            };

            return Ok(returnQuestion);
        }

        //Delete a question 
        [HttpDelete("{questionId}/DeleteQuestion")]
        public async Task<IActionResult> DeleteQuestion(Guid questionId)
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

            _context.Questions.Remove(question);    
            await _context.SaveChangesAsync();
            return Ok();
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
