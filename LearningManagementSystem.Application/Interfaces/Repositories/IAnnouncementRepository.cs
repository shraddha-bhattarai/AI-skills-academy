using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Domain.Entities.Communication;

namespace LearningManagementSystem.Application.Interfaces.Repositories;

public interface IAnnouncementRepository
{
    Task<PagedResult<Announcement>> GetPagedAsync(string? search, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Announcement?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Announcement announcement, CancellationToken cancellationToken = default);

    void Update(Announcement announcement);

    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
