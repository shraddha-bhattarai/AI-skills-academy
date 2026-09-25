using LearningManagementSystem.Application.Interfaces.Services;
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
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEnrollmentService _enrollmentService;

        public CoursesController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IEnrollmentService enrollmentService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _enrollmentService = enrollmentService;
        }

        public async Task<IActionResult> Index(string? search, string filter = "All", CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            var studentName = user != null && (!string.IsNullOrWhiteSpace(user.FirstName) || !string.IsNullOrWhiteSpace(user.LastName))
                ? $"{user.FirstName} {user.LastName}".Trim()
                : (user?.UserName ?? "Student");

            List<StudentCourseItemViewModel> allStudentCourses = new();

            if (user != null)
            {
                var dbEnrollments = await _dbContext.Enrollments
                    .Include(e => e.Course)
                        .ThenInclude(c => c.Category)
                    .Include(e => e.Course)
                        .ThenInclude(c => c.Lessons)
                    .Where(e => e.StudentId == user.Id)
                    .ToListAsync(cancellationToken);

                var completedLessonIds = await _dbContext.LessonProgresses
                    .Where(p => p.StudentId == user.Id)
                    .Select(p => p.LessonId)
                    .ToListAsync(cancellationToken);
                var completedLessonIdSet = completedLessonIds.ToHashSet();

                if (dbEnrollments.Any())
                {
                    allStudentCourses = dbEnrollments.Select(e => new StudentCourseItemViewModel
                    {
                        CourseId = e.CourseId,
                        Title = e.Course.Title,
                        ShortDescription = e.Course.Description.Length > 120 ? e.Course.Description.Substring(0, 120) + "..." : e.Course.Description,
                        Category = e.Course.Category?.Name ?? "General",
                        ImageUrl = e.Course.ThumbnailUrl ?? string.Empty,
                        ProgressPercentage = (int)e.Progress,
                        CompletedLessons = e.Course.Lessons.Count(l => completedLessonIdSet.Contains(l.Id)),
                        TotalLessons = e.Course.Lessons.Count,
                        EnrollmentDate = e.EnrollmentDate
                    }).ToList();
                }
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                var query = search.Trim().ToLower();
                allStudentCourses = allStudentCourses.Where(c =>
                    c.Title.ToLower().Contains(query) ||
                    c.Category.ToLower().Contains(query) ||
                    c.ShortDescription.ToLower().Contains(query)
                ).ToList();
            }

            // Calculate stats before filtering by status tab
            var stats = new CourseStatsViewModel
            {
                TotalEnrolled = allStudentCourses.Count,
                InProgressCount = allStudentCourses.Count(c => c.IsInProgress),
                CompletedCount = allStudentCourses.Count(c => c.IsCompleted),
                NotStartedCount = allStudentCourses.Count(c => c.IsNotStarted),
                AverageProgress = allStudentCourses.Any() ? Math.Round(allStudentCourses.Average(c => c.ProgressPercentage), 1) : 0
            };

            // Filter by active tab status
            var filteredCourses = filter switch
            {
                "InProgress" => allStudentCourses.Where(c => c.IsInProgress).ToList(),
                "Completed" => allStudentCourses.Where(c => c.IsCompleted).ToList(),
                "NotStarted" => allStudentCourses.Where(c => c.IsNotStarted).ToList(),
                _ => allStudentCourses
            };

            var viewModel = new StudentCoursesViewModel
            {
                StudentName = studentName,
                SearchTerm = search ?? string.Empty,
                ActiveFilter = filter,
                Stats = stats,
                Courses = filteredCourses
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId, string? returnUrl, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var (success, alreadyEnrolled, message) = await _enrollmentService.EnrollStudentAsync(user.Id, courseId, cancellationToken);
            TempData[success || alreadyEnrolled ? "Success" : "Error"] = message;

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Details", "Courses", new { area = "", id = courseId });
        }
    }
}
