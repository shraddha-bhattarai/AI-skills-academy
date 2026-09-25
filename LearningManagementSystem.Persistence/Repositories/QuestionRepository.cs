using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Domain.Entities.Quizzes;
using LearningManagementSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Persistence.Repositories;

public class QuestionRepository : IQuestionRepository
{
    private readonly ApplicationDbContext _context;

    public QuestionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Question>> GetPagedAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Questions
            .Include(q => q.Quiz)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(q =>
                q.QuestionText.Contains(term) ||
                q.Quiz.Title.Contains(term));
        }

        query = ApplySorting(query, sortBy, sortDescending);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Question>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Question?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Questions
            .Include(q => q.Quiz)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Questions.AnyAsync(q => q.Id == id, cancellationToken);
    }

    public async Task AddAsync(Question question, CancellationToken cancellationToken = default)
    {
        await _context.Questions.AddAsync(question, cancellationToken);
    }

    public void Update(Question question)
    {
        _context.Questions.Update(question);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var question = await _context.Questions.FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
        if (question is not null)
        {
            question.IsDeleted = true;
            question.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Question> ApplySorting(IQueryable<Question> query, string? sortBy, bool sortDescending)
    {
        return (sortBy?.ToLowerInvariant()) switch
        {
            "question" => sortDescending ? query.OrderByDescending(q => q.QuestionText) : query.OrderBy(q => q.QuestionText),
            "quiz" => sortDescending ? query.OrderByDescending(q => q.Quiz.Title) : query.OrderBy(q => q.Quiz.Title),
            _ => sortDescending ? query.OrderByDescending(q => q.Id) : query.OrderBy(q => q.Id)
        };
    }
}
