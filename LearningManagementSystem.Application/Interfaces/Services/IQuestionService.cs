using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Questions;

namespace LearningManagementSystem.Application.Interfaces.Services;

public interface IQuestionService
{
    Task<PagedResult<QuestionDto>> GetAllAsync(string? search, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<QuestionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateQuestionDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateQuestionDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
