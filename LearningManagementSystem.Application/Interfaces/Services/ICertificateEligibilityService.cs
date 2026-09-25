namespace LearningManagementSystem.Application.Interfaces.Services;

public interface ICertificateEligibilityService
{
    /// <summary>
    /// Checks whether the student has met every completion requirement for the course
    /// (all lessons completed, every course quiz passed at least once, every course
    /// assignment submission graded) and, if so, auto-issues a Certificate unless one
    /// already exists for this student+course. Returns true only if a certificate was
    /// newly created by this call.
    /// </summary>
    Task<bool> CheckAndIssueAsync(string studentId, int courseId, CancellationToken cancellationToken = default);
}
