using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Domain.Entities.Certificates;
using LearningManagementSystem.Persistence.Context;
using LearningManagementSystem.Persistence.Identity;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Persistence.Repositories;

public class CertificateRepository : ICertificateRepository
{
    private readonly ApplicationDbContext _context;

    public CertificateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Certificate>> GetPagedAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Certificates
            .Include(c => c.Course)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var matchingUserIds = await _context.Set<ApplicationUser>()
                .Where(u => (u.FirstName + " " + u.LastName).Contains(term) || u.Email!.Contains(term))
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);

            query = query.Where(c =>
                c.CertificateNumber.Contains(term) ||
                c.Course.Title.Contains(term) ||
                matchingUserIds.Contains(c.StudentId));
        }

        query = ApplySorting(query, sortBy, sortDescending);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Certificate>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Certificate?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Certificates
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Certificate>> GetByStudentIdAsync(string studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Certificates
            .Include(c => c.Course)
            .Where(c => c.StudentId == studentId)
            .OrderByDescending(c => c.IssuedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Certificates.AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsForStudentAndCourseAsync(string studentId, int courseId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId)) return false;
        return await _context.Certificates.AnyAsync(c => c.StudentId == studentId && c.CourseId == courseId, cancellationToken);
    }

    public async Task AddAsync(Certificate certificate, CancellationToken cancellationToken = default)
    {
        await _context.Certificates.AddAsync(certificate, cancellationToken);
    }

    public void Update(Certificate certificate)
    {
        _context.Certificates.Update(certificate);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var certificate = await _context.Certificates.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (certificate is not null)
        {
            certificate.IsDeleted = true;
            certificate.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Certificate> ApplySorting(IQueryable<Certificate> query, string? sortBy, bool sortDescending)
    {
        return (sortBy?.ToLowerInvariant()) switch
        {
            "certificatenumber" => sortDescending ? query.OrderByDescending(c => c.CertificateNumber) : query.OrderBy(c => c.CertificateNumber),
            "issueddate" => sortDescending ? query.OrderByDescending(c => c.IssuedDate) : query.OrderBy(c => c.IssuedDate),
            "course" => sortDescending ? query.OrderByDescending(c => c.Course.Title) : query.OrderBy(c => c.Course.Title),
            _ => sortDescending ? query.OrderByDescending(c => c.Id) : query.OrderBy(c => c.Id)
        };
    }
}
