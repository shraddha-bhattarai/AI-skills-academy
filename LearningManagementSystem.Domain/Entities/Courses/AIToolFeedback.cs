using LearningManagementSystem.Domain.Common;

namespace LearningManagementSystem.Domain.Entities.Courses;

public class AIToolFeedback : BaseEntity
{
    public int AIToolId { get; set; }

    public AITool AITool { get; set; } = null!;

    public string StudentId { get; set; } = string.Empty;

    public bool IsUseful { get; set; }

    public string? Comment { get; set; }
}
