using LearningManagementSystem.Domain.Common;

namespace LearningManagementSystem.Domain.Entities.Courses;

public class Enrollment : BaseEntity
{
    public string StudentId { get; set; } = string.Empty;

    public int CourseId { get; set; }

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    public decimal Progress { get; set; }

    public Course Course { get; set; } = null!;
}