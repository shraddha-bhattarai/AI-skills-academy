using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Domain.Entities.Assignments;
using LearningManagementSystem.Domain.Enums;
using LearningManagementSystem.Persistence.Context;
using LearningManagementSystem.Persistence.Identity;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Persistence.Repositories;

public class SubmissionRepository : ISubmissionRepository
{
    private readonly ApplicationDbContext _context;

    public SubmissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Submission>> GetPagedAsync(
        string? search,
        SubmissionStatus? status,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Submissions
            .Include(s => s.Assignment)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var matchingUserIds = await _context.Set<ApplicationUser>()
                .Where(u => (u.FirstName + " " + u.LastName).Contains(term) || u.Email!.Contains(term))
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);

            query = query.Where(s =>
                s.Assignment.Title.Contains(term) ||
                matchingUserIds.Contains(s.StudentId) ||
                (s.FilePath != null && s.FilePath.Contains(term)));
        }

        query = ApplySorting(query, sortBy, sortDescending);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Submission>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Submission?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Submissions
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Submissions.AnyAsync(s => s.Id == id, cancellationToken);
    }

    public async Task AddAsync(Submission submission, CancellationToken cancellationToken = default)
    {
        await _context.Submissions.AddAsync(submission, cancellationToken);
    }

    public void Update(Submission submission)
    {
        _context.Submissions.Update(submission);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var submission = await _context.Submissions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (submission is not null)
        {
            submission.IsDeleted = true;
            submission.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Submission> ApplySorting(IQueryable<Submission> query, string? sortBy, bool sortDescending)
    {
        return (sortBy?.ToLowerInvariant()) switch
        {
            "assignment" => sortDescending ? query.OrderByDescending(s => s.Assignment.Title) : query.OrderBy(s => s.Assignment.Title),
            "marks" => sortDescending ? query.OrderByDescending(s => s.Marks) : query.OrderBy(s => s.Marks),
            "status" => sortDescending ? query.OrderByDescending(s => s.Status) : query.OrderBy(s => s.Status),
            "createdat" => sortDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
            _ => sortDescending ? query.OrderByDescending(s => s.Id) : query.OrderBy(s => s.Id)
        };
    }
}
