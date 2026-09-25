using LearningManagementSystem.Domain.Common;

namespace LearningManagementSystem.Domain.Entities.Courses;

public class Bookmark : BaseEntity
{
    public string StudentId { get; set; } = string.Empty;

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;
}
