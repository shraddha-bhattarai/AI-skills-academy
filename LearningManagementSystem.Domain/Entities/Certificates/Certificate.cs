using LearningManagementSystem.Domain.Common;
using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Domain.Entities.Certificates;

public class Certificate : BaseEntity
{
    public string StudentId { get; set; } = string.Empty;

    public string CertificateNumber { get; set; } = Guid.NewGuid().ToString();

    public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;
}