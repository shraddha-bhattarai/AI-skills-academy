using System.Linq.Expressions;
using LearningManagementSystem.Domain.Common;
using LearningManagementSystem.Domain.Entities.Assignments;
using LearningManagementSystem.Domain.Entities.Certificates;
using LearningManagementSystem.Domain.Entities.Communication;
using LearningManagementSystem.Domain.Entities.Courses;
using LearningManagementSystem.Domain.Entities.Quizzes;
using LearningManagementSystem.Persistence.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Persistence.Context;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Lesson> Lessons => Set<Lesson>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();

    public DbSet<Resource> Resources => Set<Resource>();

    public DbSet<Discussion> Discussions => Set<Discussion>();

    public DbSet<DiscussionReply> DiscussionReplies => Set<DiscussionReply>();

    public DbSet<AITool> AITools => Set<AITool>();

    public DbSet<AIToolFeedback> AIToolFeedback => Set<AIToolFeedback>();

    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();

    public DbSet<Assignment> Assignments => Set<Assignment>();

    public DbSet<Submission> Submissions => Set<Submission>();

    public DbSet<Quiz> Quizzes => Set<Quiz>();

    public DbSet<Question> Questions => Set<Question>();

    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();

    public DbSet<Certificate> Certificates => Set<Certificate>();

    public DbSet<Announcement> Announcements => Set<Announcement>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // SQLite cannot sort decimal values without a conversion. These fields are used
        // by the course and submission sort controls. SQL Server keeps its decimal columns.
        if (Database.IsSqlite())
        {
            builder.Entity<Course>().Property(x => x.Price).HasConversion<double>();
            builder.Entity<Enrollment>().Property(x => x.Progress).HasConversion<double>();
            builder.Entity<Submission>().Property(x => x.Marks).HasConversion<double>();
        }

        builder.Entity<Category>()
            .HasMany(x => x.Courses)
            .WithOne(x => x.Category)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Course>()
            .HasMany(x => x.Lessons)
            .WithOne(x => x.Course)
            .HasForeignKey(x => x.CourseId);

        builder.Entity<Course>()
            .HasMany(x => x.Assignments)
            .WithOne(x => x.Course)
            .HasForeignKey(x => x.CourseId);

        builder.Entity<Course>()
            .HasMany(x => x.Quizzes)
            .WithOne(x => x.Course)
            .HasForeignKey(x => x.CourseId);

        builder.Entity<Course>()
            .HasMany(x => x.Enrollments)
            .WithOne(x => x.Course)
            .HasForeignKey(x => x.CourseId);

        builder.Entity<Assignment>()
            .HasMany(x => x.Submissions)
            .WithOne(x => x.Assignment)
            .HasForeignKey(x => x.AssignmentId);

        builder.Entity<Quiz>()
            .HasMany(x => x.Questions)
            .WithOne(x => x.Quiz)
            .HasForeignKey(x => x.QuizId);

        builder.Entity<Quiz>()
            .HasMany(x => x.Attempts)
            .WithOne(x => x.Quiz)
            .HasForeignKey(x => x.QuizId);

        builder.Entity<Course>()
            .HasMany(x => x.Certificates)
            .WithOne(x => x.Course)
            .HasForeignKey(x => x.CourseId);

        builder.Entity<Lesson>()
            .HasMany(x => x.LessonProgresses)
            .WithOne(x => x.Lesson)
            .HasForeignKey(x => x.LessonId);

        builder.Entity<LessonProgress>()
            .HasIndex(x => new { x.StudentId, x.LessonId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.Entity<Lesson>()
            .HasMany(x => x.Resources)
            .WithOne(x => x.Lesson)
            .HasForeignKey(x => x.LessonId);

        builder.Entity<Course>()
            .HasMany<Discussion>()
            .WithOne(x => x.Course)
            .HasForeignKey(x => x.CourseId);

        builder.Entity<Discussion>()
            .HasMany(x => x.Replies)
            .WithOne(x => x.Discussion)
            .HasForeignKey(x => x.DiscussionId);

        builder.Entity<AITool>()
            .HasMany(x => x.Feedback)
            .WithOne(x => x.AITool)
            .HasForeignKey(x => x.AIToolId);

        builder.Entity<AIToolFeedback>()
            .HasIndex(x => new { x.AIToolId, x.StudentId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.Entity<Course>()
            .HasMany<Bookmark>()
            .WithOne(x => x.Course)
            .HasForeignKey(x => x.CourseId);

        builder.Entity<Bookmark>()
            .HasIndex(x => new { x.StudentId, x.CourseId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "entity");
            var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
            var filter = Expression.Lambda(
                Expression.Equal(property, Expression.Constant(false)),
                parameter);

            builder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }
}
