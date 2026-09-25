using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Announcements;

namespace LearningManagementSystem.Application.Interfaces.Services;

public interface IAnnouncementService
{
    Task<PagedResult<AnnouncementDto>> GetAllAsync(string? search, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<AnnouncementDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateAnnouncementDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateAnnouncementDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
