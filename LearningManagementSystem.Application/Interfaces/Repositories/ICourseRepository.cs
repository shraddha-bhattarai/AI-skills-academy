using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Application.Interfaces.Repositories;

public interface ICourseRepository
{
    Task<PagedResult<Course>> GetPagedAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        int? categoryId = null,
        int? level = null,
        bool publishedOnly = false,
        CancellationToken cancellationToken = default);

    Task<Course?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Course course, CancellationToken cancellationToken = default);

    void Update(Course course);

    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupItemDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
