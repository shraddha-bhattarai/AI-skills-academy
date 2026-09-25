using LearningManagementSystem.Domain.Common;

namespace LearningManagementSystem.Domain.Entities.Courses;

public class DiscussionReply : BaseEntity
{
    public int DiscussionId { get; set; }

    public Discussion Discussion { get; set; } = null!;

    public string StudentId { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}
