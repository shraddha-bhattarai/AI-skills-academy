using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Submissions;
using LearningManagementSystem.Domain.Enums;

namespace LearningManagementSystem.Application.Interfaces.Services;

public interface ISubmissionService
{
    Task<PagedResult<SubmissionDto>> GetAllAsync(string? search, SubmissionStatus? status, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<SubmissionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateSubmissionDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateSubmissionDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task GradeAsync(GradeSubmissionDto dto, CancellationToken cancellationToken = default);
}
