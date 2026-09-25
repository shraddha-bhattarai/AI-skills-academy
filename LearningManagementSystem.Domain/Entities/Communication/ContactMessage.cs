using LearningManagementSystem.Domain.Common;

namespace LearningManagementSystem.Domain.Entities.Communication;

public class ContactMessage : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsResolved { get; set; }
}