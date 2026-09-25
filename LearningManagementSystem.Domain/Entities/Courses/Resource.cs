using LearningManagementSystem.Domain.Common;

namespace LearningManagementSystem.Domain.Entities.Courses;

public class Resource : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string FileUrl { get; set; } = string.Empty;

    public int LessonId { get; set; }

    public Lesson Lesson { get; set; } = null!;
}
