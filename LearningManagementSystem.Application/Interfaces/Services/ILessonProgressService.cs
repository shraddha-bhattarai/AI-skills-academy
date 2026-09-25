namespace LearningManagementSystem.Application.Interfaces.Services;

public interface ILessonProgressService
{
    Task<(bool Success, string Message)> MarkLessonCompleteAsync(int lessonId, string studentId, CancellationToken cancellationToken = default);
}
