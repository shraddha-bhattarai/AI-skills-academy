using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Enums;
using LearningManagementSystem.Persistence.Context;
using LearningManagementSystem.Persistence.Identity;
using LearningManagementSystem.Web.Areas.Student.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LearningManagementSystem.Web.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IAssignmentService _assignmentService;
        private readonly ICertificateService _certificateService;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext dbContext,
            IAssignmentService assignmentService,
            ICertificateService certificateService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _assignmentService = assignmentService;
            _certificateService = certificateService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var studentName = !string.IsNullOrWhiteSpace(user.FirstName) || !string.IsNullOrWhiteSpace(user.LastName)
                ? $"{user.FirstName} {user.LastName}".Trim()
                : (user.UserName ?? "Student");

            var enrollments = await _dbContext.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Category)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Lessons)
                .Where(e => e.StudentId == user.Id)
                .OrderByDescending(e => e.EnrollmentDate)
                .ToListAsync(cancellationToken);

            var enrolledCourseIds = enrollments.Select(e => e.CourseId).ToList();

            var completedLessonIds = (await _dbContext.LessonProgresses
                .Where(p => p.StudentId == user.Id)
                .Select(p => p.LessonId)
                .ToListAsync(cancellationToken)).ToHashSet();

            var assignments = (await _assignmentService.GetStudentAssignmentsAsync(user.Id, cancellationToken)).ToList();
            var certificates = await _certificateService.GetStudentCertificatesAsync(user.Id, cancellationToken);

            var now = DateTime.UtcNow;
            var pendingAssignments = assignments
                .Where(a => a.SubmissionStatus == null || a.SubmissionStatus == SubmissionStatus.Pending)
                .ToList();

            var viewModel = new StudentDashboardViewModel
            {
                StudentName = studentName,
                HasEnrollments = enrollments.Any(),

                Stats = new DashboardStatsViewModel
                {
                    EnrolledCourses = enrollments.Count,
                    CompletedCourses = enrollments.Count(e => e.Progress >= 100),
                    PendingAssignments = pendingAssignments.Count,
                    CertificatesEarned = certificates.Count
                },

                EnrolledCourses = enrollments.Take(4).Select(e => new EnrolledCourseSummaryViewModel
                {
                    CourseId = e.CourseId,
                    Title = e.Course.Title,
                    Category = e.Course.Category?.Name ?? "General",
                    ImageUrl = e.Course.ThumbnailUrl ?? string.Empty,
                    ProgressPercentage = (int)e.Progress,
                    CompletedLessons = e.Course.Lessons.Count(l => completedLessonIds.Contains(l.Id)),
                    TotalLessons = e.Course.Lessons.Count
                }).ToList(),

                UpcomingAssignments = pendingAssignments
                    .OrderBy(a => a.DueDate)
                    .Take(3)
                    .Select(a => new UpcomingAssignmentViewModel
                    {
                        AssignmentId = a.Id,
                        Title = a.Title,
                        CourseTitle = a.CourseTitle,
                        DueDate = a.DueDate,
                        Status = a.DueDate < now ? "Overdue" : "Pending",
                        PriorityClass = a.DueDate < now ? "bg-danger" : "bg-warning text-dark"
                    }).ToList(),

                LearningProgress = new LearningProgressViewModel
                {
                    OverallProgressPercentage = enrollments.Any() ? (int)Math.Round(enrollments.Average(e => e.Progress)) : 0,
                    LessonsCompletedThisWeek = await _dbContext.LessonProgresses
                        .CountAsync(p => p.StudentId == user.Id && p.CompletedAt >= now.AddDays(-7), cancellationToken),
                    TotalLessonsCompleted = completedLessonIds.Count
                }
            };

            // Continue Learning: most recently enrolled course that isn't finished yet
            var continueCourse = enrollments.FirstOrDefault(e => e.Progress < 100);
            if (continueCourse != null)
            {
                var nextLesson = continueCourse.Course.Lessons
                    .Where(l => !completedLessonIds.Contains(l.Id))
                    .OrderBy(l => l.Id)
                    .FirstOrDefault();

                viewModel.ContinueLearning = new ContinueLearningViewModel
                {
                    CourseId = continueCourse.CourseId,
                    Title = continueCourse.Course.Title,
                    Category = continueCourse.Course.Category?.Name ?? "General",
                    ImageUrl = continueCourse.Course.ThumbnailUrl ?? string.Empty,
                    ProgressPercentage = (int)continueCourse.Progress,
                    NextLessonTitle = nextLesson?.Title,
                    CompletedLessons = continueCourse.Course.Lessons.Count(l => completedLessonIds.Contains(l.Id)),
                    TotalLessons = continueCourse.Course.Lessons.Count
                };
            }

            // Available quizzes for enrolled courses
            if (enrolledCourseIds.Any())
            {
                viewModel.AvailableQuizzes = await _dbContext.Quizzes
                    .Include(q => q.Course)
                    .Include(q => q.Questions)
                    .Where(q => enrolledCourseIds.Contains(q.CourseId))
                    .OrderByDescending(q => q.CreatedAt)
                    .Take(3)
                    .Select(q => new AvailableQuizViewModel
                    {
                        QuizId = q.Id,
                        Title = q.Title,
                        CourseTitle = q.Course.Title,
                        TotalQuestions = q.Questions.Count
                    })
                    .ToListAsync(cancellationToken);
            }

            // Recent activity: merge real timestamped events from lessons completed, assignments submitted, and certificates earned
            var recentActivities = new List<RecentActivityViewModel>();

            recentActivities.AddRange(await _dbContext.LessonProgresses
                .Include(p => p.Lesson)
                .Where(p => p.StudentId == user.Id)
                .OrderByDescending(p => p.CompletedAt)
                .Take(5)
                .Select(p => new RecentActivityViewModel
                {
                    ActivityType = "Lesson Completed",
                    Title = $"Completed '{p.Lesson.Title}'",
                    TimeStamp = p.CompletedAt,
                    IconClass = "bi-play-circle-fill",
                    BadgeBgClass = "bg-primary"
                })
                .ToListAsync(cancellationToken));

            recentActivities.AddRange(assignments
                .Where(a => a.SubmittedAt.HasValue)
                .OrderByDescending(a => a.SubmittedAt)
                .Take(5)
                .Select(a => new RecentActivityViewModel
                {
                    ActivityType = "Assignment Submitted",
                    Title = $"Submitted '{a.Title}'",
                    TimeStamp = a.SubmittedAt!.Value,
                    IconClass = "bi-file-earmark-check-fill",
                    BadgeBgClass = "bg-success"
                }));

            recentActivities.AddRange(certificates
                .OrderByDescending(c => c.IssuedDate)
                .Take(3)
                .Select(c => new RecentActivityViewModel
                {
                    ActivityType = "Certificate Earned",
                    Title = $"Earned Certificate: '{c.CourseTitle}'",
                    TimeStamp = c.IssuedDate,
                    IconClass = "bi-award-fill",
                    BadgeBgClass = "bg-warning text-dark"
                }));

            viewModel.RecentActivities = recentActivities
                .OrderByDescending(a => a.TimeStamp)
                .Take(5)
                .ToList();

            return View(viewModel);
        }
    }
}
