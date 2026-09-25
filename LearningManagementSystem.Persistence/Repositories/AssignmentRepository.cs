using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Domain.Entities.Assignments;
using LearningManagementSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Persistence.Repositories;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly ApplicationDbContext _context;

    public AssignmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Assignment>> GetPagedAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Assignments
            .Include(a => a.Course)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a =>
                a.Title.Contains(term) ||
                a.Description.Contains(term) ||
                a.Course.Title.Contains(term));
        }

        query = ApplySorting(query, sortBy, sortDescending);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Assignment>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Assignment?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Assignments
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Assignments.AnyAsync(a => a.Id == id, cancellationToken);
    }

    public async Task AddAsync(Assignment assignment, CancellationToken cancellationToken = default)
    {
        await _context.Assignments.AddAsync(assignment, cancellationToken);
    }

    public void Update(Assignment assignment)
    {
        _context.Assignments.Update(assignment);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var assignment = await _context.Assignments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (assignment is not null)
        {
            assignment.IsDeleted = true;
            assignment.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LookupItemDto>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Assignments
            .OrderBy(a => a.Title)
            .Select(a => new LookupItemDto
            {
                Id = a.Id.ToString(),
                Name = a.Title
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Assignment>> GetAssignmentsByCourseIdsAsync(IEnumerable<int> courseIds, CancellationToken cancellationToken = default)
    {
        var courseIdList = courseIds.ToList();
        if (!courseIdList.Any()) return Array.Empty<Assignment>();

        return await _context.Assignments
            .Include(a => a.Course)
            .Where(a => courseIdList.Contains(a.CourseId))
            .OrderBy(a => a.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Submission>> GetStudentSubmissionsForAssignmentsAsync(string studentId, IEnumerable<int> assignmentIds, CancellationToken cancellationToken = default)
    {
        var assignmentIdList = assignmentIds.ToList();
        if (!assignmentIdList.Any() || string.IsNullOrWhiteSpace(studentId)) return Array.Empty<Submission>();

        return await _context.Submissions
            .Where(s => s.StudentId == studentId && assignmentIdList.Contains(s.AssignmentId))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsStudentEnrolledInCourseAsync(string studentId, int courseId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId)) return false;
        return await _context.Enrollments.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId, cancellationToken);
    }

    public async Task<Submission?> GetStudentSubmissionAsync(int assignmentId, string studentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId)) return null;
        return await _context.Submissions.FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId, cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetStudentEnrolledCourseIdsAsync(string studentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId)) return Array.Empty<int>();
        return await _context.Enrollments
            .Where(e => e.StudentId == studentId)
            .Select(e => e.CourseId)
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Assignment> ApplySorting(IQueryable<Assignment> query, string? sortBy, bool sortDescending)
    {
        return (sortBy?.ToLowerInvariant()) switch
        {
            "title" => sortDescending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
            "duedate" => sortDescending ? query.OrderByDescending(a => a.DueDate) : query.OrderBy(a => a.DueDate),
            "status" => sortDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
            "course" => sortDescending ? query.OrderByDescending(a => a.Course.Title) : query.OrderBy(a => a.Course.Title),
            _ => sortDescending ? query.OrderByDescending(a => a.Id) : query.OrderBy(a => a.Id)
        };
    }
}
