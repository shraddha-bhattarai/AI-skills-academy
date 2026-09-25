using LearningManagementSystem.Domain.Common;
using LearningManagementSystem.Domain.Enums;

namespace LearningManagementSystem.Domain.Entities.Assignments;

public class Submission : BaseEntity
{
    public string StudentId { get; set; } = string.Empty;

    public string? FilePath { get; set; }

    public decimal? Marks { get; set; }

    public string? Feedback { get; set; }

    public SubmissionStatus Status { get; set; }

    public int AssignmentId { get; set; }

    public Assignment Assignment { get; set; } = null!;
}