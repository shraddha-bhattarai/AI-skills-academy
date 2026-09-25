using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Domain.Entities.Courses;
using LearningManagementSystem.Domain.Enums;
using LearningManagementSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Persistence.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly ApplicationDbContext _context;

    public CourseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Course>> GetPagedAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        int? categoryId = null,
        int? level = null,
        bool publishedOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Lessons)
            .Include(c => c.Enrollments)
            .AsQueryable();

        if (publishedOnly)
        {
            query = query.Where(c => c.Status == CourseStatus.Published);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                c.Title.Contains(term) ||
                c.Description.Contains(term) ||
                c.Category.Name.Contains(term));
        }

        if (categoryId is > 0)
        {
            query = query.Where(c => c.CategoryId == categoryId.Value);
        }

        if (level is > 0)
        {
            query = query.Where(c => (int)c.Level == level.Value);
        }

        query = ApplySorting(query, sortBy, sortDescending);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Course>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Course?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Courses.AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task AddAsync(Course course, CancellationToken cancellationToken = default)
    {
        await _context.Courses.AddAsync(course, cancellationToken);
    }

    public void Update(Course course)
    {
        _context.Courses.Update(course);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (course is not null)
        {
            course.IsDeleted = true;
            course.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LookupItemDto>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Courses
            .OrderBy(c => c.Title)
            .Select(c => new LookupItemDto
            {
                Id = c.Id.ToString(),
                Name = c.Title
            })
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Course> ApplySorting(IQueryable<Course> query, string? sortBy, bool sortDescending)
    {
        return (sortBy?.ToLowerInvariant()) switch
        {
            "title" => sortDescending ? query.OrderByDescending(c => c.Title) : query.OrderBy(c => c.Title),
            "price" => sortDescending ? query.OrderByDescending(c => c.Price) : query.OrderBy(c => c.Price),
            "level" => sortDescending ? query.OrderByDescending(c => c.Level) : query.OrderBy(c => c.Level),
            "status" => sortDescending ? query.OrderByDescending(c => c.Status) : query.OrderBy(c => c.Status),
            "category" => sortDescending ? query.OrderByDescending(c => c.Category.Name) : query.OrderBy(c => c.Category.Name),
            "createdat" => sortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => sortDescending ? query.OrderByDescending(c => c.Id) : query.OrderBy(c => c.Id)
        };
    }
}
