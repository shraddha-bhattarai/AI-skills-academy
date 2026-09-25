using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Domain.Entities.Quizzes;

namespace LearningManagementSystem.Application.Interfaces.Repositories;

public interface IQuizRepository
{
    Task<PagedResult<Quiz>> GetPagedAsync(string? search, int? courseId, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Quiz?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> HasQuestionsAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Quiz quiz, CancellationToken cancellationToken = default);

    void Update(Quiz quiz);

    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupItemDto>> GetLookupAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Quiz>> GetQuizzesByCourseIdsAsync(IEnumerable<int> courseIds, CancellationToken cancellationToken = default);

    Task<Quiz?> GetQuizWithQuestionsAsync(int quizId, CancellationToken cancellationToken = default);

    Task<bool> IsStudentEnrolledInQuizCourseAsync(string studentId, int quizId, CancellationToken cancellationToken = default);
}
