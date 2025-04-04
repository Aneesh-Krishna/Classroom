﻿using ClassroomAPI.Data;
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

        [HttpGet("{courseId}/getMeetings")]
        public async Task<IActionResult> GetMeetings(Guid courseId)
        {
            var userId = getCurrentUserId();
            if (userId == null)
                return Unauthorized("You're not logged in!");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized("You're not registered!");

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId);
            if (course == null)
                return NotFound("Course not found!");

            var isMember = await _context.CourseMembers
                .Where(cm => cm.UserId == userId && cm.CourseId == courseId)
                .SingleOrDefaultAsync() != null;

            if (!isMember)
                return Unauthorized("You're not a member of the course!");

            var meetings = await _context.Meetings
                .Where(m => m.CourseId == courseId)
                .Select(m => new
                {
                    m.MeetingId,
                    m.MeetingName,
                    m.hasEnded
                })
                .ToListAsync();

            return Ok(meetings);
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
        public async Task<IActionResult> CreateMeeting(Guid courseId, [FromForm] string meetingName)
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
        public async Task<IActionResult> JoinMeeting(Guid meetingId)
        {
            var userId = getCurrentUserId();
            if (userId == null)
                return Unauthorized("You're not authorized!");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized("You're not authorized!");
            
            var meeting = await _context.Meetings.SingleOrDefaultAsync(m => m.MeetingId == meetingId);
            if (meeting == null)
                return BadRequest("Meeting not found!");

            if (meeting.hasEnded)
                return BadRequest("Meeting has already ended!");

            var course = await _context.Courses.SingleOrDefaultAsync(c => c.CourseId == meeting.CourseId);
            if (course == null)
                return NotFound("Course not found!");

            var isCourseMember = await _context.CourseMembers
                .AnyAsync(cm => cm.UserId == userId && cm.CourseId == course.CourseId);
            if (!isCourseMember)
                return BadRequest("You're not enrolled in the course!");

            //var hasJoined = await _context.Participants
            //    .AnyAsync(p => p.UserId == userId && p.MeetingId == meeting.MeetingId);

            //if (hasJoined)
            //    return BadRequest("You've already joined the meeting!");

            var participant = new Participant
            {
                ParticipantId = Guid.NewGuid(),
                MeetingId = meetingId,
                MeetingName = meeting.MeetingName,
                Meeting = meeting,
                UserId = userId,
                User = user
            };

            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            var returnParticipant = new
            {
                participant.ParticipantId,
                participant.MeetingId,
                participant.MeetingName,
                participant.UserId,
                participant.User.UserName
            };

            await _meetContext.Groups.AddToGroupAsync(userId, meetingId.ToString());
            await _meetContext.Clients.Groups(meetingId.ToString()).SendAsync("UserJoined", user.UserName);
   
            return Ok(returnParticipant);
        }

        [HttpPut("{meetingId}/LeaveMeeting")]
        public async Task<IActionResult> LeaveMeeting(Guid meetingId)
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

            participant.hasLeft = true;
            await _context.SaveChangesAsync();

            await _meetContext.Groups.RemoveFromGroupAsync(userId, meetingId.ToString());
            await _meetContext.Clients.Groups(meetingId.ToString()).SendAsync("UserLeft", participant.User?.UserName);

            return Ok();
        }

        [HttpPut("{meetingId}/EndMeeting")]
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

            meeting.hasEnded = true;

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
