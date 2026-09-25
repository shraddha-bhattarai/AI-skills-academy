using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Domain.Entities.Communication;

namespace LearningManagementSystem.Application.Interfaces.Repositories;

public interface IContactMessageRepository
{
    Task<PagedResult<ContactMessage>> GetPagedAsync(string? search, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<ContactMessage?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(ContactMessage contactMessage, CancellationToken cancellationToken = default);

    void Update(ContactMessage contactMessage);

    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
