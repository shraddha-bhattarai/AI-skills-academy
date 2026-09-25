using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Domain.Entities.Assignments;
using LearningManagementSystem.Domain.Enums;

namespace LearningManagementSystem.Application.Interfaces.Repositories;

public interface ISubmissionRepository
{
    Task<PagedResult<Submission>> GetPagedAsync(string? search, SubmissionStatus? status, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Submission?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Submission submission, CancellationToken cancellationToken = default);

    void Update(Submission submission);

    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
