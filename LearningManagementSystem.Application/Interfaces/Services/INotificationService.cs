using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Notifications;

namespace LearningManagementSystem.Application.Interfaces.Services;

public interface INotificationService
{
    Task<PagedResult<NotificationDto>> GetAllAsync(string? search, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<NotificationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationDto>> GetStudentNotificationsAsync(string userId, string? filter = "All", CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);

    Task<bool> MarkAsReadAsync(int notificationId, string userId, CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);

    Task<NotificationDto?> GetStudentNotificationByIdAsync(int id, string userId, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateNotificationDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
