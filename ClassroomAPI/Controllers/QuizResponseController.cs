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
    public class QuizResponseController : ControllerBase
    {
        private readonly ClassroomDbContext _context;
        public QuizResponseController(ClassroomDbContext context)
        {
            _context = context;
        }

        //All Responses of a quiz
        [HttpGet("{quizId}/GetAllResponses")]
        public async Task<IActionResult> GetAllResponses(Guid quizId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("User Id not found!");

            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == quizId);
            if (quiz == null)
                return NotFound("Quiz not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == quiz.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            var isMember = await _context.CourseMembers
                .Where(cm => cm.CourseId == quiz.CourseId && cm.UserId == userId)
                .FirstOrDefaultAsync() != null;
            if (!isMember)
                return Unauthorized("You're not  authoried!");

            var responses = await _context.QuizResponses
                .Where(qr => qr.QuizId == quizId)
                .ToListAsync();
            return Ok(responses);
        }

        //Get all the responses of a user in a course
        [HttpGet("{courseId}/GetAllResponsesOfUser")]
        public async Task<IActionResult> GetAllResponsesOfUser(Guid courseId, [FromForm] string courseUserId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return NotFound("User Id not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null) return NotFound("Course not found!");

            if (course.AdminId != userId)
                return Unauthorized("You're not authorized!");

            var courseUser = await _context.Users.FindAsync(courseUserId);
            if (courseUser == null) 
                return NotFound("No user with the provided userId");

            var isMember = await _context.CourseMembers
                .Where(cm => cm.CourseId == courseId && cm.UserId == courseUserId)
                .FirstOrDefaultAsync() != null;
            if (!isMember) return NotFound("The user is not a member of the course!");

            var quizzes = await _context.Quizzes
                .Where(q => q.CourseId == course.CourseId)
                .ToListAsync();
            if (quizzes.Count < 1)
                return NotFound("No quizzes in the course!");

            List<QuizResponse> responses = new List<QuizResponse>();
            foreach(var quiz in quizzes)
            {
                var response = await _context.QuizResponses
                    .Where(qr => qr.QuizId == quiz.QuizId && qr.UserId == courseUserId)
                    .FirstOrDefaultAsync();

                if(response != null)
                    responses.Add(response);
            }

            if (responses.Count < 1)
                return NotFound("No responses found for the given user!");

            return Ok(responses);
        }

        //Get all the answers submitted by a user for a quiz
        [HttpGet("{quizId}/GetAnswersByMember")]
        public async Task<IActionResult> GetAnswersByMember(Guid quizId, [FromForm] string courseUserId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return NotFound("User Id not found!");

            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == quizId);
            if (quiz == null) return NotFound("Quiz not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == quiz.CourseId);
            if (course == null) return NotFound("Course not found!");

            if (course.AdminId != userId)
                return Unauthorized("You're not authorized!");

            var isMember = await _context.CourseMembers
                .Where(cm => cm.CourseId == course.CourseId && cm.UserId == courseUserId)
                .FirstOrDefaultAsync() != null;
            if (!isMember) 
                return NotFound("User is not a member of the course!");

            var quizResponse = await _context.QuizResponses.FirstOrDefaultAsync(qr => qr.QuizId == quizId && qr.UserId == courseUserId);
            if (quizResponse == null)
                return NotFound("There's no response by the given user for this quiz!");

            var answers = await _context.Answers
                .Where(a => a.QuizResponseId == quizResponse.QuizResponseId)
                .Select(a => new
                {
                    Question = a.Question.Text,
                    AnswerGiven = a.Option.Text,
                    AnswerGivenIsCorrect = a.Option.IsCorrect
                })
                .ToListAsync();
            
            return Ok(answers);
        }

        //Submit a response (answers)
        [HttpPost("{quizId}/SubmitQuiz")]
        public async Task<IActionResult> SubmitQuiz(Guid quizId, [FromBody] SubmitQuizModel model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("User Id not found!");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found!");

            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.QuizId == quizId);
            if (quiz == null)
                return NotFound("Quiz not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == quiz.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            var isMember = await _context.CourseMembers
                .Where(cm => cm.CourseId == quiz.CourseId && cm.UserId == userId)
                .FirstOrDefaultAsync() != null;
            if (!isMember)
                return Unauthorized("You're not authorized!");

            if (DateTime.Now > quiz.Deadline)
                return BadRequest("Sorry, the deadline has passed!");

            var existingQuizResponse = await _context.QuizResponses
                .Where(qr => qr.QuizId == quizId && qr.UserId == userId)
                .FirstOrDefaultAsync() != null;
            if (existingQuizResponse)
                return BadRequest("You've already submitted your answers!");

            List<Answer> FinalAnswers = new List<Answer>();
            var quizResponse = new QuizResponse
            {
                QuizResponseId = Guid.NewGuid(),
                QuizId = quizId,
                Quiz = quiz,
                UserId = userId,
                User = user,
                SubmittedAt = DateTime.Now
            };

            int finalScore = 0;

            foreach(var answer in model.Answers)
            {
                var question = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == answer.questionId);
                if (question == null)
                    return BadRequest("Question not found!");

                var option = await _context.Options.FirstOrDefaultAsync(o => o.OptionId == answer.optionId && o.QuestionId == question.QuestionId);
                if (option == null) return BadRequest("Option not found!");

                if (option.IsCorrect)
                    finalScore++;

                var finalAnswer = new Answer
                {
                    AnswerId = Guid.NewGuid(),
                    QuizResponseId = quizResponse.QuizResponseId,
                    QuizResponse = quizResponse,
                    QuestionId = question.QuestionId,
                    Question = question,
                    OptionId = option.OptionId,
                    Option = option
                };

                FinalAnswers.Add(finalAnswer);
            }

            quizResponse.Score = finalScore;

            _context.QuizResponses.Add(quizResponse);
            _context.Answers.AddRange(FinalAnswers);
            await _context.SaveChangesAsync();

            return Ok("Answers submitted successfully!");
        }
    }

    public class SubmitQuizModel
    {
        public List<AnswerSubmissionModel> Answers { get; set; } = new List<AnswerSubmissionModel>();
    }

    public class AnswerSubmissionModel
    {
        public Guid questionId { get; set; }
        public Guid optionId { get; set; }
    }
}
