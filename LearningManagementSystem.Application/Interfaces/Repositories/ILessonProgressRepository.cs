using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Application.Interfaces.Repositories;

public interface ILessonProgressRepository
{
    Task<LessonProgress?> GetAsync(string studentId, int lessonId, CancellationToken cancellationToken = default);

    Task AddAsync(LessonProgress progress, CancellationToken cancellationToken = default);

    Task<int> CountCompletedForStudentCourseAsync(string studentId, int courseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetCompletedLessonIdsAsync(string studentId, int courseId, CancellationToken cancellationToken = default);

    Task<int> CountCompletedThisWeekAsync(string studentId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
