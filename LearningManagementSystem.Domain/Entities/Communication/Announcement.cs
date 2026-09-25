using LearningManagementSystem.Domain.Common;

namespace LearningManagementSystem.Domain.Entities.Communication;

public class Announcement : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime PublishDate { get; set; } = DateTime.UtcNow;
}