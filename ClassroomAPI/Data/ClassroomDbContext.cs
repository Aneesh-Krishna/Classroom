using ClassroomAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClassroomAPI.Data
{
    public class ClassroomDbContext : IdentityDbContext<ApplicationUser>
    {
        public ClassroomDbContext(DbContextOptions<ClassroomDbContext> options) : base(options) 
        { 
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<QuizResponse> QuizResponses { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<CourseMember> CourseMembers { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
        public DbSet<Material> Materials { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<Course>()
                .HasOne(c => c.GroupAdmin)
                .WithMany(u => u.CourseAdmin)
                .HasForeignKey(c => c.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CourseMember>()
                .HasOne(cm => cm.Course)
                .WithMany(c => c.CourseMembers)
                .HasForeignKey(cm => cm.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<CourseMember>()
                .HasOne(cm => cm.User)
                .WithMany(u => u.CourseMemberships)
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Quiz>()
                .HasOne(q => q.Course)
                .WithMany(c => c.Quizzes)
                .HasForeignKey(q => q.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(qu => qu.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Option>()
                .HasOne(o => o.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<QuizResponse>()
                .HasOne(qr => qr.Quiz)
                .WithMany(q => q.QuizResponses)
                .HasForeignKey(qr => qr.QuizId)
                .OnDelete(DeleteBehavior.NoAction);
            builder.Entity<QuizResponse>()
                .HasOne(qr => qr.User)
                .WithMany(u => u.QuizResponses)
                .HasForeignKey(qr => qr.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Answer>()
                .HasOne(a => a.QuizResponse)
                .WithMany(qr => qr.Answers)
                .HasForeignKey(a => a.QuizResponseId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Answer>()
                .HasOne(a => a.Option)
                .WithMany()
                .HasForeignKey(a => a.OptionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Chat>()
                .HasOne(c => c.Course)
                .WithMany(co => co.Chats)
                .HasForeignKey(c => c.CourseId)
                .OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Chat>()
                .HasOne(c => c.User)
                .WithMany(u => u.Chats)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Assignment>()
                .HasOne(a => a.Course)
                .WithMany(c => c.Assignments)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AssignmentSubmission>()
                .HasOne(asb => asb.Assignment)
                .WithMany(a => a.AssignmentSubmissions)
                .HasForeignKey(asb => asb.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<AssignmentSubmission>()
                .HasOne(asb => asb.SubmittedBy)
                .WithMany(asb => asb.AssignmentSubmissions)
                .HasForeignKey(asb => asb.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Material>()
                .HasOne(m => m.Course)
                .WithMany(c => c.Materials)
                .HasForeignKey(m => m.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
