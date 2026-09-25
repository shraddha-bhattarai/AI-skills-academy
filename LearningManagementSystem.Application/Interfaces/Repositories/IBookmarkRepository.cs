using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Application.Interfaces.Repositories;

public interface IBookmarkRepository
{
    Task<IReadOnlyList<Bookmark>> GetByStudentIdAsync(string studentId, CancellationToken cancellationToken = default);

    Task<Bookmark?> GetAsync(string studentId, int courseId, CancellationToken cancellationToken = default);

    Task AddAsync(Bookmark bookmark, CancellationToken cancellationToken = default);

    Task RemoveAsync(Bookmark bookmark, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
