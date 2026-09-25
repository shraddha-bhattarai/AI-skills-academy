using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Application.Interfaces.Repositories;

public interface ILessonRepository
{
    Task<PagedResult<Lesson>> GetPagedAsync(string? search, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Lesson?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Lesson lesson, CancellationToken cancellationToken = default);

    void Update(Lesson lesson);

    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Lesson>> GetByCourseIdAsync(int courseId, CancellationToken cancellationToken = default);

    Task<int> CountByCourseIdAsync(int courseId, CancellationToken cancellationToken = default);

    Task<bool> IsStudentEnrolledInCourseAsync(string studentId, int courseId, CancellationToken cancellationToken = default);
}
