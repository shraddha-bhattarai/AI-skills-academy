using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Domain.Entities.Communication;

namespace LearningManagementSystem.Application.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<PagedResult<Notification>> GetPagedAsync(string? search, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Notification?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Notification>> GetByUserIdAsync(string userId, string? filter = "All", CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<bool> MarkAsReadAsync(int notificationId, string userId, CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

    void Update(Notification notification);

    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
