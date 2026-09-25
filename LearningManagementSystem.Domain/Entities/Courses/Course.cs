using LearningManagementSystem.Domain.Common;
using LearningManagementSystem.Domain.Enums;
using LearningManagementSystem.Domain.Entities.Assignments;
using LearningManagementSystem.Domain.Entities.Quizzes;
using LearningManagementSystem.Domain.Entities.Certificates;

namespace LearningManagementSystem.Domain.Entities.Courses;

public class Course : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? LearningOutcomes { get; set; }

    public decimal Price { get; set; }

    public CourseLevel Level { get; set; }

    public CourseStatus Status { get; set; }

    public string? ThumbnailUrl { get; set; }

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
}