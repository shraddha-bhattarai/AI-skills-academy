using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Domain.Entities.Courses;
using LearningManagementSystem.Persistence.Context;
using LearningManagementSystem.Persistence.Identity;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Persistence.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly ApplicationDbContext _context;

    public EnrollmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Enrollment>> GetPagedAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Enrollments
            .Include(e => e.Course)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var matchingUserIds = await _context.Set<ApplicationUser>()
                .Where(u => (u.FirstName + " " + u.LastName).Contains(term) || u.Email!.Contains(term))
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);

            query = query.Where(e =>
                e.Course.Title.Contains(term) ||
                matchingUserIds.Contains(e.StudentId));
        }

        query = ApplySorting(query, sortBy, sortDescending);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Enrollment>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Enrollment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Enrollment?> GetByStudentAndCourseAsync(string studentId, int courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId, cancellationToken);
    }

    public async Task<IReadOnlyList<Enrollment>> GetByStudentAsync(string studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
            .Where(e => e.StudentId == studentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments.AnyAsync(e => e.Id == id, cancellationToken);
    }

    public async Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        await _context.Enrollments.AddAsync(enrollment, cancellationToken);
    }

    public void Update(Enrollment enrollment)
    {
        _context.Enrollments.Update(enrollment);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (enrollment is not null)
        {
            enrollment.IsDeleted = true;
            enrollment.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Enrollment> ApplySorting(IQueryable<Enrollment> query, string? sortBy, bool sortDescending)
    {
        return (sortBy?.ToLowerInvariant()) switch
        {
            "course" => sortDescending ? query.OrderByDescending(e => e.Course.Title) : query.OrderBy(e => e.Course.Title),
            "progress" => sortDescending ? query.OrderByDescending(e => e.Progress) : query.OrderBy(e => e.Progress),
            "enrollmentdate" => sortDescending ? query.OrderByDescending(e => e.EnrollmentDate) : query.OrderBy(e => e.EnrollmentDate),
            _ => sortDescending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id)
        };
    }
}
