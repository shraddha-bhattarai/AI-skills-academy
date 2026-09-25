using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Domain.Entities.Quizzes;

namespace LearningManagementSystem.Application.Interfaces.Repositories;

public interface IQuestionRepository
{
    Task<PagedResult<Question>> GetPagedAsync(string? search, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Question?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Question question, CancellationToken cancellationToken = default);

    void Update(Question question);

    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
