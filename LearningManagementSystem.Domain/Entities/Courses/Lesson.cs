using LearningManagementSystem.Domain.Common;

namespace LearningManagementSystem.Domain.Entities.Courses;

public class Lesson : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? VideoUrl { get; set; }

    public string? NotesUrl { get; set; }

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();

    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
}