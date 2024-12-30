using ClassroomAPI.Data;
using ClassroomAPI.Hubs;
using ClassroomAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ClassroomAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MeetingController : ControllerBase
    {
        private readonly ClassroomDbContext _context;
        private readonly IHubContext<MeetingHub> _meetContext;
        public MeetingController(ClassroomDbContext context, IHubContext<MeetingHub> meetContext)
        {
            _context = context;
            _meetContext = meetContext;
        }

        [HttpGet("{meetingId}/getParticipants")]
        public async Task<IActionResult> GetParticipants(Guid meetingId)
        {
            var userId = getCurrentUserId();
            if (userId == null)
                return Unauthorized("You're not authorized!");

            var participants = await _context.Participants
                .Where(p => p.MeetingId == meetingId)
                .Select(p => new
                {
                    p.UserId,
                    p.User.UserName
                })
                .ToListAsync();

            return Ok(participants);
        }

        [HttpPost("{courseId}/CreateMeeting")]
        public async Task<IActionResult> CreateMeeting(Guid courseId, [FromBody] string meetingName)
        {
            var userId = getCurrentUserId();
            if (userId == null)
                return Unauthorized("You're not a registered user!");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized("You're not registered!");

            var course = await _context.Courses.SingleOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
                return NotFound("Course not found!");

            if (course.AdminId != userId)
                return Unauthorized("You're not authorized!");

            var meeting = new Meeting
            {
                MeetingId = Guid.NewGuid(),
                MeetingName = meetingName,
                CourseId = courseId,
                Course = course
            };

            var participant = new Participant
            {
                ParticipantId = Guid.NewGuid(),
                User = user,
                UserId = userId,
                MeetingId = meeting.MeetingId,
                MeetingName = meetingName,
                Meeting = meeting
            };

            _context.Meetings.Add(meeting);
            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            var returnMeeting = await _context.Meetings
                .Where(m => m.MeetingId == meeting.MeetingId)
                .Select(m => new
                {
                    m.MeetingId,
                    m.MeetingName,
                    m.CourseId,
                    CourseName = m.Course.CourseName,
                    AdminName = m.Course.GroupAdmin.UserName
                })
                .SingleOrDefaultAsync();

            return Ok(returnMeeting);
        }

        [HttpPost("{meetingId}/JoinMeeting")]
        public async Task<IActionResult> JoinMeeting(Guid meetingId, [FromBody] string newParticipantUserId)
        {
            var userId = getCurrentUserId();
            if (userId == null)
                return Unauthorized("You're not authorized!");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized("You're not authorized!");

            var newParticipantUser = await _context.Users.FindAsync(newParticipantUserId);
            if(newParticipantUser == null)  
                return NotFound("User not found!");
            
            var meeting = await _context.Meetings.SingleOrDefaultAsync(m => m.MeetingId == meetingId);
            if (meeting == null)
                return BadRequest("Meeting not found!");

            if (meeting.hasEnded)
                return BadRequest("Meeting has already ended!");

            var course = await _context.Courses.SingleOrDefaultAsync(c => c.CourseId == meeting.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            var isCourseMember = await _context.CourseMembers
                .Where(cm => cm.UserId == newParticipantUserId)
                .SingleOrDefaultAsync() != null;
            if (!isCourseMember)
                return BadRequest("You're not enrolled in the course!");

            if (userId != course.AdminId || userId != newParticipantUserId)
                return Unauthorized("You're not authorized!");

            var participant = new Participant
            {
                ParticipantId = Guid.NewGuid(),
                MeetingId = meetingId,
                MeetingName = meeting.MeetingName,
                Meeting = meeting,
                UserId = newParticipantUserId,
                User = newParticipantUser
            };

            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            var returnParticipant = await _context.Participants
                .Where(p => p.UserId == userId && p.MeetingId == meetingId)
                .Select(p => new
                {
                    p.ParticipantId,
                    p.MeetingId,
                    p.UserId,
                    p.MeetingName,
                    ParticipantName = p.User.UserName
                })
                .SingleOrDefaultAsync();

            await _meetContext.Groups.AddToGroupAsync(newParticipantUserId, meetingId.ToString());
            await _meetContext.Clients.Groups(meetingId.ToString()).SendAsync("UserJoined", newParticipantUser.UserName);
   
            return Ok(returnParticipant);
        }

        [HttpPost("{meetingId}/LeaveMeeting")]
        public async Task<IActionResult> LeaveMeeting(Guid meetingId, [FromBody] Guid participantId)
        {
            var userId = getCurrentUserId();
            if (userId == null)
                return Unauthorized("You're not authorized!");
            
            var meeting = await _context.Meetings.FirstOrDefaultAsync(m => m.MeetingId == meetingId);
            if (meeting == null)
                return BadRequest("Meeting not found!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == meeting.CourseId);
            if(course == null)
                return BadRequest("Course not found!");

            var participant = await _context.Participants.SingleOrDefaultAsync(p => p.UserId == userId);
            if (participant == null)
                return BadRequest("Participant not found!");

            if (userId != course.AdminId || userId != participant.UserId)
                return Unauthorized("You're not authorized to perform this task!");

            participant.hasLeft = true;
            await _context.SaveChangesAsync();

            await _meetContext.Groups.RemoveFromGroupAsync(userId, meetingId.ToString());
            await _meetContext.Clients.Groups(meetingId.ToString()).SendAsync("UserLeft", participant.User?.UserName);

            return Ok();
        }

        [HttpPost("{meetingId}/EndMeeting")]
        public async Task<IActionResult> EndMeeting(Guid meetingId)
        {
            var userId = getCurrentUserId();
            if (userId == null)
                return Unauthorized("You're not authorized!");

            var meeting = await _context.Meetings.FirstOrDefaultAsync(m => m.MeetingId == meetingId);
            if (meeting == null)
                return NotFound("Meeting not found!");

            if (meeting.hasEnded)
                return BadRequest("Meeting has already ended!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == meeting.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            if (userId != course.AdminId)
                return Unauthorized("Only the admin is authorized to perform this task!");

            var participants = await _context.Participants.Where(p => p.MeetingId == meetingId).ToListAsync();
            
            foreach(var participant in participants)
            {
                await _meetContext.Groups.RemoveFromGroupAsync(userId, meetingId.ToString());
                await _meetContext.Clients.Groups(meetingId.ToString()).SendAsync("UserLeft", participant.User?.UserName);
            }

            await _context.SaveChangesAsync();
            return Ok();

        }

        private string getCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

    }
}
