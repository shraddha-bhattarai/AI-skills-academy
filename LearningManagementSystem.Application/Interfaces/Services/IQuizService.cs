using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Quizzes;

namespace LearningManagementSystem.Application.Interfaces.Services;

public interface IQuizService
{
    Task<PagedResult<QuizDto>> GetAllAsync(string? search, int? courseId, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<QuizDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateQuizDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateQuizDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupItemDto>> GetLookupAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QuizDto>> GetStudentQuizzesAsync(string studentId, CancellationToken cancellationToken = default);

    Task<QuizDto?> GetStudentQuizDetailsAsync(int quizId, string studentId, CancellationToken cancellationToken = default);
}
