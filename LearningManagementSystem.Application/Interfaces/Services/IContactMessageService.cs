using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.ContactMessages;

namespace LearningManagementSystem.Application.Interfaces.Services;

public interface IContactMessageService
{
    Task<PagedResult<ContactMessageDto>> GetAllAsync(string? search, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<ContactMessageDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateContactMessageDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateContactMessageDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
