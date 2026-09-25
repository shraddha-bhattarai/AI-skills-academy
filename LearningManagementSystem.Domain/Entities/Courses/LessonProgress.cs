using LearningManagementSystem.Domain.Common;

namespace LearningManagementSystem.Domain.Entities.Courses;

public class LessonProgress : BaseEntity
{
    public string StudentId { get; set; } = string.Empty;

    public int LessonId { get; set; }

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    public Lesson Lesson { get; set; } = null!;
}
