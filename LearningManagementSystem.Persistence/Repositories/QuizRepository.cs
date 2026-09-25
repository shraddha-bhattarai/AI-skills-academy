using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Domain.Entities.Quizzes;
using LearningManagementSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Persistence.Repositories;

public class QuizRepository : IQuizRepository
{
    private readonly ApplicationDbContext _context;

    public QuizRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Quiz>> GetPagedAsync(
        string? search,
        int? courseId,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Quizzes
            .Include(q => q.Course)
            .Include(q => q.Questions)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(q =>
                q.Title.Contains(term) ||
                q.Course.Title.Contains(term));
        }

        if (courseId.HasValue)
        {
            query = query.Where(q => q.CourseId == courseId.Value);
        }

        query = ApplySorting(query, sortBy, sortDescending);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Quiz>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Quiz?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Quizzes
            .Include(q => q.Course)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Quizzes.AnyAsync(q => q.Id == id, cancellationToken);
    }

    public Task<bool> HasQuestionsAsync(int id, CancellationToken cancellationToken = default)
    {
        return _context.Questions.AnyAsync(q => q.QuizId == id, cancellationToken);
    }

    public async Task AddAsync(Quiz quiz, CancellationToken cancellationToken = default)
    {
        await _context.Quizzes.AddAsync(quiz, cancellationToken);
    }

    public void Update(Quiz quiz)
    {
        _context.Quizzes.Update(quiz);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
        if (quiz is not null)
        {
            quiz.IsDeleted = true;
            quiz.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LookupItemDto>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Quizzes
            .OrderBy(q => q.Title)
            .Select(q => new LookupItemDto
            {
                Id = q.Id.ToString(),
                Name = q.Title
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Quiz>> GetQuizzesByCourseIdsAsync(IEnumerable<int> courseIds, CancellationToken cancellationToken = default)
    {
        var idList = courseIds.ToList();
        if (!idList.Any()) return Array.Empty<Quiz>();

        return await _context.Quizzes
            .Include(q => q.Course)
            .Include(q => q.Questions)
            .Where(q => idList.Contains(q.CourseId))
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Quiz?> GetQuizWithQuestionsAsync(int quizId, CancellationToken cancellationToken = default)
    {
        return await _context.Quizzes
            .Include(q => q.Course)
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == quizId, cancellationToken);
    }

    public async Task<bool> IsStudentEnrolledInQuizCourseAsync(string studentId, int quizId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId)) return false;

        var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == quizId, cancellationToken);
        if (quiz is null) return false;

        return await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == quiz.CourseId, cancellationToken);
    }

    private static IQueryable<Quiz> ApplySorting(IQueryable<Quiz> query, string? sortBy, bool sortDescending)
    {
        return (sortBy?.ToLowerInvariant()) switch
        {
            "title" => sortDescending ? query.OrderByDescending(q => q.Title) : query.OrderBy(q => q.Title),
            "totalmarks" => sortDescending ? query.OrderByDescending(q => q.TotalMarks) : query.OrderBy(q => q.TotalMarks),
            "course" => sortDescending ? query.OrderByDescending(q => q.Course.Title) : query.OrderBy(q => q.Course.Title),
            _ => sortDescending ? query.OrderByDescending(q => q.Id) : query.OrderBy(q => q.Id)
        };
    }
}
