using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Entities.Courses;
using LearningManagementSystem.Domain.Enums;
using LearningManagementSystem.Persistence.Context;
using LearningManagementSystem.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace LearningManagementSystem.Tests;

public class EnrollmentAndLessonTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public EnrollmentAndLessonTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<(string StudentId, Course Course)> SeedCourseAsync(bool enrollStudent)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var student = await userManager.FindByEmailAsync("student@lms.com")
            ?? throw new InvalidOperationException("Seeded student account not found — UserSeeder should have created it on startup.");

        var category = db.Categories.FirstOrDefault() ?? new Category { Name = "Test Category-" + Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
        if (category.Id == 0)
        {
            db.Categories.Add(category);
            await db.SaveChangesAsync();
        }

        var course = new Course
        {
            Title = "Test Course " + Guid.NewGuid(),
            Description = "Seeded for automated tests.",
            Price = 0,
            Level = CourseLevel.Beginner,
            Status = CourseStatus.Published,
            CategoryId = category.Id,
            CreatedAt = DateTime.UtcNow
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();

        if (enrollStudent)
        {
            db.Enrollments.Add(new Enrollment
            {
                StudentId = student.Id,
                CourseId = course.Id,
                EnrollmentDate = DateTime.UtcNow,
                Progress = 0
            });
            await db.SaveChangesAsync();
        }

        return (student.Id, course);
    }

    [Fact]
    public async Task SelfEnroll_CreatesEnrollment_AndPreventsDuplicateOnSecondAttempt()
    {
        var (_, course) = await SeedCourseAsync(enrollStudent: false);

        var client = _factory.CreateClient();
        await TestHelpers.LoginAsync(client, "student@lms.com", "Student@123");

        var detailsPage = await client.GetAsync($"/Courses/Details/{course.Id}");
        var detailsHtml = await detailsPage.Content.ReadAsStringAsync();
        var token = TestHelpers.ExtractAntiForgeryToken(detailsHtml);

        var form = new Dictionary<string, string>
        {
            ["courseId"] = course.Id.ToString(),
            ["__RequestVerificationToken"] = token
        };

        await client.PostAsync("/Student/Courses/Enroll", new FormUrlEncodedContent(form));
        await client.PostAsync("/Student/Courses/Enroll", new FormUrlEncodedContent(form)); // duplicate attempt

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var enrollmentCount = db.Enrollments.Count(e => e.CourseId == course.Id);

        Assert.Equal(1, enrollmentCount);
    }

    [Fact]
    public async Task Student_CannotAccessLesson_FromCourseTheyAreNotEnrolledIn()
    {
        var (_, course) = await SeedCourseAsync(enrollStudent: false); // deliberately NOT enrolled

        int lessonId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var lesson = new Lesson { Title = "Off-limits lesson", Content = "secret", CourseId = course.Id, CreatedAt = DateTime.UtcNow };
            db.Lessons.Add(lesson);
            await db.SaveChangesAsync();
            lessonId = lesson.Id;
        }

        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await TestHelpers.LoginAsync(client, "student@lms.com", "Student@123");

        var response = await client.GetAsync($"/Student/Lessons/Details/{lessonId}");

        // Ownership check rejects and redirects back to the lesson index, never serving the content.
        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);

        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.False(verifyDb.LessonProgresses.Any(lp => lp.LessonId == lessonId));
    }

    [Fact]
    public async Task MarkLessonComplete_RecalculatesRealEnrollmentProgress()
    {
        var (studentId, course) = await SeedCourseAsync(enrollStudent: true);

        int lesson1Id, lesson2Id;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var l1 = new Lesson { Title = "Lesson 1", Content = "x", CourseId = course.Id, CreatedAt = DateTime.UtcNow };
            var l2 = new Lesson { Title = "Lesson 2", Content = "y", CourseId = course.Id, CreatedAt = DateTime.UtcNow };
            db.Lessons.AddRange(l1, l2);
            await db.SaveChangesAsync();
            lesson1Id = l1.Id;
            lesson2Id = l2.Id;
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var progressService = scope.ServiceProvider.GetRequiredService<ILessonProgressService>();
            var result = await progressService.MarkLessonCompleteAsync(lesson1Id, studentId);
            Assert.True(result.Success, result.Message);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var enrollment = db.Enrollments.First(e => e.StudentId == studentId && e.CourseId == course.Id);
            Assert.Equal(50m, enrollment.Progress); // 1 of 2 lessons completed
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var progressService = scope.ServiceProvider.GetRequiredService<ILessonProgressService>();
            await progressService.MarkLessonCompleteAsync(lesson2Id, studentId);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var enrollment = db.Enrollments.First(e => e.StudentId == studentId && e.CourseId == course.Id);
            Assert.Equal(100m, enrollment.Progress); // both lessons completed
        }
    }
}
