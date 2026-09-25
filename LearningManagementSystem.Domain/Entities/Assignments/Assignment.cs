using LearningManagementSystem.Domain.Common;
using LearningManagementSystem.Domain.Enums;
using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Domain.Entities.Assignments;

public class Assignment : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }

    public AssignmentStatus Status { get; set; }

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}