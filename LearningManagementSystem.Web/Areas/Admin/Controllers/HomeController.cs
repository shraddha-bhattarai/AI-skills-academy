using LearningManagementSystem.Domain.Enums;
using LearningManagementSystem.Persistence.Context;
using LearningManagementSystem.Persistence.Identity;
using LearningManagementSystem.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        // ── Admin name ────────────────────────────────────────────────
        var adminUser = await _userManager.GetUserAsync(User);
        var adminName = adminUser != null
            && (!string.IsNullOrWhiteSpace(adminUser.FirstName) || !string.IsNullOrWhiteSpace(adminUser.LastName))
            ? $"{adminUser.FirstName} {adminUser.LastName}".Trim()
            : (adminUser?.UserName ?? "Administrator");

        // ── Students: users in "Student" role ────────────────────────
        var studentRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "Student", ct);
        int totalStudents = 0;
        if (studentRole != null)
        {
            totalStudents = await _db.UserRoles
                .CountAsync(ur => ur.RoleId == studentRole.Id, ct);
        }

        // ── Platform counts ───────────────────────────────────────────
        var totalCourses       = await _db.Courses.CountAsync(ct);
        var totalCategories    = await _db.Categories.CountAsync(ct);
        var totalEnrollments   = await _db.Enrollments.CountAsync(ct);
        var totalAssignments   = await _db.Assignments.CountAsync(ct);
        var totalQuizzes       = await _db.Quizzes.CountAsync(ct);
        var totalCertificates  = await _db.Certificates.CountAsync(ct);
        var pendingSubmissions = await _db.Submissions
            .CountAsync(s => s.Status == SubmissionStatus.Submitted, ct);

        // ── Recent courses (last 5 created) ───────────────────────────
        var recentCourses = await _db.Courses
            .Include(c => c.Category)
            .Include(c => c.Enrollments)
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .Select(c => new RecentCourseItem
            {
                Id          = c.Id,
                Title       = c.Title,
                Category    = c.Category.Name,
                Status      = c.Status.ToString(),
                StatusBadge = c.Status == CourseStatus.Published ? "bg-success"
                            : c.Status == CourseStatus.Draft     ? "bg-warning text-dark"
                            : "bg-secondary",
                Enrollments = c.Enrollments.Count,
                CreatedAt   = c.CreatedAt
            })
            .ToListAsync(ct);

        // ── Recent students (last 5 registered in Student role) ───────
        List<RecentStudentItem> recentStudents = new();
        if (studentRole != null)
        {
            var studentUserIds = await _db.UserRoles
                .Where(ur => ur.RoleId == studentRole.Id)
                .Select(ur => ur.UserId)
                .ToListAsync(ct);

            recentStudents = await _db.Set<ApplicationUser>()
                .Where(u => studentUserIds.Contains(u.Id))
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new RecentStudentItem
                {
                    Id       = u.Id,
                    FullName = (u.FirstName + " " + u.LastName).Trim(),
                    Email    = u.Email ?? string.Empty,
                    JoinedAt = u.CreatedAt,
                    IsActive = u.IsActive
                })
                .ToListAsync(ct);
        }

        // ── Recent enrollments (last 5) ───────────────────────────────
        var recentEnrollmentsRaw = await _db.Enrollments
            .Include(e => e.Course)
            .OrderByDescending(e => e.EnrollmentDate)
            .Take(5)
            .ToListAsync(ct);

        // ── Recent certificates (last 5) ──────────────────────────────
        var recentCertificatesRaw = await _db.Certificates
            .Include(c => c.Course)
            .OrderByDescending(c => c.IssuedDate)
            .Take(5)
            .ToListAsync(ct);

        // Resolve real student names instead of showing raw Identity user IDs.
        var relevantUserIds = recentEnrollmentsRaw.Select(e => e.StudentId)
            .Concat(recentCertificatesRaw.Select(c => c.StudentId))
            .Distinct()
            .ToList();
        var userNameMap = await _db.Set<ApplicationUser>()
            .Where(u => relevantUserIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim(), ct);

        var recentEnrollments = recentEnrollmentsRaw.Select(e => new RecentEnrollmentItem
        {
            StudentName = userNameMap.TryGetValue(e.StudentId, out var n1) && n1.Length > 0 ? n1 : "Unknown Student",
            CourseTitle = e.Course.Title,
            EnrolledOn = e.EnrollmentDate,
            Progress = e.Progress
        }).ToList();

        var recentCertificates = recentCertificatesRaw.Select(c => new RecentCertificateItem
        {
            StudentName = userNameMap.TryGetValue(c.StudentId, out var n2) && n2.Length > 0 ? n2 : "Unknown Student",
            CourseTitle = c.Course.Title,
            IssuedDate = c.IssuedDate,
            CertNumber = c.CertificateNumber
        }).ToList();

        var vm = new AdminDashboardViewModel
        {
            AdminName          = adminName,
            TotalStudents      = totalStudents,
            TotalCourses       = totalCourses,
            TotalCategories    = totalCategories,
            TotalEnrollments   = totalEnrollments,
            TotalAssignments   = totalAssignments,
            TotalQuizzes       = totalQuizzes,
            TotalCertificates  = totalCertificates,
            PendingSubmissions = pendingSubmissions,
            RecentCourses      = recentCourses,
            RecentStudents     = recentStudents,
            RecentEnrollments  = recentEnrollments,
            RecentCertificates = recentCertificates
        };

        return View(vm);
    }
}
