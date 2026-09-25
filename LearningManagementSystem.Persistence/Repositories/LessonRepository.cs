using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Domain.Entities.Courses;
using LearningManagementSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Persistence.Repositories;

public class LessonRepository : ILessonRepository
{
    private readonly ApplicationDbContext _context;

    public LessonRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Lesson>> GetPagedAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Lessons
            .Include(l => l.Course)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(l =>
                l.Title.Contains(term) ||
                l.Content.Contains(term) ||
                l.Course.Title.Contains(term));
        }

        query = ApplySorting(query, sortBy, sortDescending);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Lesson>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Lesson?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Lessons
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Lessons.AnyAsync(l => l.Id == id, cancellationToken);
    }

    public async Task AddAsync(Lesson lesson, CancellationToken cancellationToken = default)
    {
        await _context.Lessons.AddAsync(lesson, cancellationToken);
    }

    public void Update(Lesson lesson)
    {
        _context.Lessons.Update(lesson);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (lesson is not null)
        {
            lesson.IsDeleted = true;
            lesson.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Lesson>> GetByCourseIdAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Lessons
            .Include(l => l.Course)
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByCourseIdAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Lessons.CountAsync(l => l.CourseId == courseId, cancellationToken);
    }

    public async Task<bool> IsStudentEnrolledInCourseAsync(string studentId, int courseId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId)) return false;
        return await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId, cancellationToken);
    }

    private static IQueryable<Lesson> ApplySorting(IQueryable<Lesson> query, string? sortBy, bool sortDescending)
    {
        return (sortBy?.ToLowerInvariant()) switch
        {
            "title" => sortDescending ? query.OrderByDescending(l => l.Title) : query.OrderBy(l => l.Title),
            "course" => sortDescending ? query.OrderByDescending(l => l.Course.Title) : query.OrderBy(l => l.Course.Title),
            "createdat" => sortDescending ? query.OrderByDescending(l => l.CreatedAt) : query.OrderBy(l => l.CreatedAt),
            _ => sortDescending ? query.OrderByDescending(l => l.Id) : query.OrderBy(l => l.Id)
        };
    }
}
