using LearningManagementSystem.Domain.Common;

namespace LearningManagementSystem.Domain.Entities.Courses;

public class Discussion : BaseEntity
{
    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public string StudentId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public ICollection<DiscussionReply> Replies { get; set; } = new List<DiscussionReply>();
}
