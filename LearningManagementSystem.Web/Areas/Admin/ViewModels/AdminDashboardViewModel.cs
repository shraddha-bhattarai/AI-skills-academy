using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Web.Areas.Admin.ViewModels;

public class AdminDashboardViewModel
{
    // ── Admin identity ────────────────────────────────────────────────
    public string AdminName { get; set; } = string.Empty;

    // ── Platform statistics ───────────────────────────────────────────
    public int TotalStudents       { get; set; }
    public int TotalCourses        { get; set; }
    public int TotalCategories     { get; set; }
    public int TotalEnrollments    { get; set; }
    public int TotalAssignments    { get; set; }
    public int TotalQuizzes        { get; set; }
    public int TotalCertificates   { get; set; }
    public int PendingSubmissions  { get; set; }

    // ── Recent data ───────────────────────────────────────────────────
    public IReadOnlyList<RecentCourseItem>      RecentCourses       { get; set; } = [];
    public IReadOnlyList<RecentStudentItem>     RecentStudents      { get; set; } = [];
    public IReadOnlyList<RecentEnrollmentItem>  RecentEnrollments   { get; set; } = [];
    public IReadOnlyList<RecentCertificateItem> RecentCertificates  { get; set; } = [];
}

public class RecentCourseItem
{
    public int    Id          { get; set; }
    public string Title       { get; set; } = string.Empty;
    public string Category    { get; set; } = string.Empty;
    public string Status      { get; set; } = string.Empty;
    public string StatusBadge { get; set; } = string.Empty;
    public int    Enrollments { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RecentStudentItem
{
    public string Id        { get; set; } = string.Empty;
    public string FullName  { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public bool   IsActive  { get; set; }
}

public class RecentEnrollmentItem
{
    public string StudentName  { get; set; } = string.Empty;
    public string CourseTitle  { get; set; } = string.Empty;
    public DateTime EnrolledOn { get; set; }
    public decimal Progress    { get; set; }
}

public class RecentCertificateItem
{
    public string StudentName  { get; set; } = string.Empty;
    public string CourseTitle  { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public string CertNumber   { get; set; } = string.Empty;
}
