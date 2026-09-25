using LearningManagementSystem.Domain.Common;

namespace LearningManagementSystem.Domain.Entities.Courses;

public class AITool : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string? Category { get; set; }

    public ICollection<AIToolFeedback> Feedback { get; set; } = new List<AIToolFeedback>();
}
