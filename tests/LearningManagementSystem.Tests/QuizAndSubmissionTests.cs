using LearningManagementSystem.Application.DTOs.Submissions;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Entities.Assignments;
using LearningManagementSystem.Domain.Entities.Courses;
using LearningManagementSystem.Domain.Entities.Quizzes;
using LearningManagementSystem.Domain.Enums;
using LearningManagementSystem.Persistence.Context;
using LearningManagementSystem.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace LearningManagementSystem.Tests;

public class QuizAndSubmissionTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public QuizAndSubmissionTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(string StudentId, Course Course)> SeedEnrolledCourseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var student = await userManager.FindByEmailAsync("student@lms.com")
            ?? throw new InvalidOperationException("Seeded student account not found.");

        var category = db.Categories.FirstOrDefault();
        if (category is null)
        {
            category = new Category { Name = "Test Category-" + Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
            db.Categories.Add(category);
            await db.SaveChangesAsync();
        }

        var course = new Course
        {
            Title = "Quiz/Submission Test Course " + Guid.NewGuid(),
            Description = "Seeded for automated tests.",
            Price = 0,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Published,
            CategoryId = category.Id,
            CreatedAt = DateTime.UtcNow
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();

        db.Enrollments.Add(new Enrollment
        {
            StudentId = student.Id,
            CourseId = course.Id,
            EnrollmentDate = DateTime.UtcNow,
            Progress = 0
        });
        await db.SaveChangesAsync();

        return (student.Id, course);
    }

    [Fact]
    public async Task QuizAttempt_PersistsToDatabase_AndIsCorrectWhenReadFromAFreshDbContext()
    {
        var (studentId, course) = await SeedEnrolledCourseAsync();

        int quizId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var quiz = new Quiz { Title = "Test Quiz", TotalMarks = 10, CourseId = course.Id, CreatedAt = DateTime.UtcNow };
            db.Quizzes.Add(quiz);
            await db.SaveChangesAsync();
            quizId = quiz.Id;
        }

        // Write the attempt using one DbContext instance (simulates the request that submits the quiz)...
        using (var writeScope = _factory.Services.CreateScope())
        {
            var repo = writeScope.ServiceProvider.GetRequiredService<IQuizAttemptRepository>();
            await repo.AddAsync(new QuizAttempt
            {
                StudentId = studentId,
                QuizId = quizId,
                Score = 8,
                TotalMarks = 10,
                Percentage = 80,
                IsPassed = true,
                CorrectCount = 4,
                IncorrectCount = 1,
                UnansweredCount = 0,
                TotalQuestions = 5,
                AttemptedAt = DateTime.UtcNow
            });
            await repo.SaveChangesAsync();
        }

        // ...then read it back using a BRAND NEW DbContext instance (simulates a fresh request after
        // logout/login — proves the result lives in the database, not in ephemeral Session state).
        using (var readScope = _factory.Services.CreateScope())
        {
            var repo = readScope.ServiceProvider.GetRequiredService<IQuizAttemptRepository>();
            var attempts = await repo.GetStudentAttemptsAsync(studentId);
            var attempt = Assert.Single(attempts.Where(a => a.QuizId == quizId));

            Assert.Equal(8, attempt.Score);
            Assert.Equal(80, attempt.Percentage);
            Assert.True(attempt.IsPassed);
        }
    }

    [Fact]
    public async Task GradedSubmission_FeedbackIsVisibleToTheStudent()
    {
        var (studentId, course) = await SeedEnrolledCourseAsync();

        int assignmentId, submissionId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var assignment = new Assignment
            {
                Title = "Test Assignment",
                Description = "desc",
                DueDate = DateTime.UtcNow.AddDays(7),
                Status = AssignmentStatus.Published,
                CourseId = course.Id,
                CreatedAt = DateTime.UtcNow
            };
            db.Assignments.Add(assignment);
            await db.SaveChangesAsync();
            assignmentId = assignment.Id;

            var submission = new Submission
            {
                StudentId = studentId,
                AssignmentId = assignmentId,
                FilePath = "/uploads/submissions/test.pdf",
                Status = SubmissionStatus.Submitted,
                CreatedAt = DateTime.UtcNow
            };
            db.Submissions.Add(submission);
            await db.SaveChangesAsync();
            submissionId = submission.Id;
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var submissionService = scope.ServiceProvider.GetRequiredService<ISubmissionService>();
            await submissionService.GradeAsync(new GradeSubmissionDto
            {
                Id = submissionId,
                Marks = 92,
                Status = SubmissionStatus.Graded,
                Feedback = "Excellent work — well-structured and thoroughly tested."
            });
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var assignmentService = scope.ServiceProvider.GetRequiredService<IAssignmentService>();
            var studentView = await assignmentService.GetStudentAssignmentDetailsAsync(assignmentId, studentId);

            Assert.NotNull(studentView);
            Assert.Equal(SubmissionStatus.Graded, studentView!.SubmissionStatus);
            Assert.Equal(92, studentView.Marks);
            Assert.Equal("Excellent work — well-structured and thoroughly tested.", studentView.Feedback);
        }
    }
}
