using LearningManagementSystem.Domain.Entities.Quizzes;

namespace LearningManagementSystem.Application.Interfaces.Repositories;

public interface IQuizAttemptRepository
{
    Task<QuizAttempt?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QuizAttempt>> GetStudentAttemptsAsync(string studentId, CancellationToken cancellationToken = default);

    Task<bool> HasPassingAttemptAsync(string studentId, int quizId, CancellationToken cancellationToken = default);

    Task AddAsync(QuizAttempt attempt, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
