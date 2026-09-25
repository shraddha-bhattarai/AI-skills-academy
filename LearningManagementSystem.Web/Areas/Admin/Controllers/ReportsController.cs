using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Persistence.Context;
using LearningManagementSystem.Persistence.Identity;
using LearningManagementSystem.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ReportsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ICourseService _courseService;
    private readonly IQuizService _quizService;

    public ReportsController(ApplicationDbContext db, ICourseService courseService, IQuizService quizService)
    {
        _db = db;
        _courseService = courseService;
        _quizService = quizService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Enrollments(int? courseId, CancellationToken ct = default)
    {
        var query = _db.Enrollments.Include(e => e.Course).AsQueryable();
        if (courseId.HasValue)
        {
            query = query.Where(e => e.CourseId == courseId.Value);
        }

        var enrollments = await query.OrderByDescending(e => e.EnrollmentDate).ToListAsync(ct);
        var userMap = await _db.Set<ApplicationUser>()
            .ToDictionaryAsync(u => u.Id, u => new { Name = $"{u.FirstName} {u.LastName}".Trim(), Email = u.Email ?? string.Empty }, ct);

        var items = enrollments.Select(e =>
        {
            userMap.TryGetValue(e.StudentId, out var user);
            return new EnrollmentReportItemViewModel
            {
                StudentName = user?.Name is { Length: > 0 } n ? n : "Unknown Student",
                StudentEmail = user?.Email ?? string.Empty,
                CourseId = e.CourseId,
                CourseTitle = e.Course?.Title ?? string.Empty,
                EnrollmentDate = e.EnrollmentDate,
                Progress = e.Progress,
                IsCompleted = e.Progress >= 100
            };
        }).ToList();

        ViewBag.Courses = await _courseService.GetLookupAsync(ct);
        ViewBag.CourseId = courseId;
        ViewBag.TotalCount = items.Count;
        ViewBag.CompletedCount = items.Count(i => i.IsCompleted);

        return View(items);
    }

    public async Task<IActionResult> Progress(int? courseId, CancellationToken ct = default)
    {
        var query = _db.Courses
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
            .AsQueryable();

        if (courseId.HasValue)
        {
            query = query.Where(c => c.Id == courseId.Value);
        }

        var courses = await query.OrderBy(c => c.Title).ToListAsync(ct);

        var items = courses.Select(c => new CourseProgressReportItemViewModel
        {
            CourseId = c.Id,
            CourseTitle = c.Title,
            TotalLessons = c.Lessons.Count,
            EnrolledStudents = c.Enrollments.Count,
            AverageProgress = c.Enrollments.Any() ? Math.Round(c.Enrollments.Average(e => e.Progress), 1) : 0,
            CompletedCount = c.Enrollments.Count(e => e.Progress >= 100)
        }).ToList();

        ViewBag.Courses = await _courseService.GetLookupAsync(ct);
        ViewBag.CourseId = courseId;

        return View(items);
    }

    public async Task<IActionResult> QuizResults(int? quizId, CancellationToken ct = default)
    {
        var query = _db.Quizzes.Include(q => q.Course).Include(q => q.Attempts).AsQueryable();
        if (quizId.HasValue)
        {
            query = query.Where(q => q.Id == quizId.Value);
        }

        var quizzes = await query.OrderBy(q => q.Title).ToListAsync(ct);

        var items = quizzes.Select(q => new QuizResultReportItemViewModel
        {
            QuizId = q.Id,
            QuizTitle = q.Title,
            CourseTitle = q.Course?.Title ?? string.Empty,
            TotalAttempts = q.Attempts.Count,
            PassCount = q.Attempts.Count(a => a.IsPassed),
            PassRatePercent = q.Attempts.Any() ? Math.Round(q.Attempts.Count(a => a.IsPassed) * 100.0 / q.Attempts.Count, 1) : 0,
            AverageScorePercent = q.Attempts.Any() ? Math.Round(q.Attempts.Average(a => a.Percentage), 1) : 0
        }).ToList();

        ViewBag.Quizzes = await _quizService.GetLookupAsync(ct);
        ViewBag.QuizId = quizId;

        return View(items);
    }
}
