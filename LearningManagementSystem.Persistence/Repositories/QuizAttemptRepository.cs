using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Domain.Entities.Quizzes;
using LearningManagementSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Persistence.Repositories;

public class QuizAttemptRepository : IQuizAttemptRepository
{
    private readonly ApplicationDbContext _context;

    public QuizAttemptRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<QuizAttempt?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.QuizAttempts
            .Include(a => a.Quiz)
                .ThenInclude(q => q.Course)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<QuizAttempt>> GetStudentAttemptsAsync(string studentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId)) return Array.Empty<QuizAttempt>();

        return await _context.QuizAttempts
            .Include(a => a.Quiz)
                .ThenInclude(q => q.Course)
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.AttemptedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasPassingAttemptAsync(string studentId, int quizId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId)) return false;
        return await _context.QuizAttempts
            .AnyAsync(a => a.StudentId == studentId && a.QuizId == quizId && a.IsPassed, cancellationToken);
    }

    public async Task AddAsync(QuizAttempt attempt, CancellationToken cancellationToken = default)
    {
        await _context.QuizAttempts.AddAsync(attempt, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
