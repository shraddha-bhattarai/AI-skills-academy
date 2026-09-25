using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Application.Interfaces.Repositories;

public interface IAiToolRepository
{
    Task<PagedResult<AITool>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AITool>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<AITool?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(AITool tool, CancellationToken cancellationToken = default);

    void Update(AITool tool);

    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<AIToolFeedback?> GetFeedbackAsync(int toolId, string studentId, CancellationToken cancellationToken = default);

    Task AddFeedbackAsync(AIToolFeedback feedback, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AIToolFeedback>> GetFeedbackForToolAsync(int toolId, CancellationToken cancellationToken = default);
}
