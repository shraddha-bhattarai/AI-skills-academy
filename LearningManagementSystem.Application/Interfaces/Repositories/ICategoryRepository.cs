using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<PagedResult<Category>> GetPagedAsync(string? search, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);

    Task AddAsync(Category category, CancellationToken cancellationToken = default);

    void Update(Category category);

    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupItemDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
