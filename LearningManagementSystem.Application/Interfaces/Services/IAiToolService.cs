using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.AiTools;

namespace LearningManagementSystem.Application.Interfaces.Services;

public interface IAiToolService
{
    Task<IReadOnlyList<AiToolDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<AiToolDto>> GetAllForAdminAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<AiToolDetailsDto?> GetByIdAsync(int id, string? studentId, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateAiToolDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateAiToolDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<(bool Success, string Message)> MarkFeedbackAsync(MarkAiToolFeedbackDto dto, string studentId, CancellationToken cancellationToken = default);
}
