using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Domain.Entities.Communication;
using LearningManagementSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Persistence.Repositories;

public class AnnouncementRepository : IAnnouncementRepository
{
    private readonly ApplicationDbContext _context;

    public AnnouncementRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Announcement>> GetPagedAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Announcements.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a =>
                a.Title.Contains(term) ||
                a.Message.Contains(term));
        }

        query = ApplySorting(query, sortBy, sortDescending);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Announcement>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Announcement?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Announcements.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Announcements.AnyAsync(a => a.Id == id, cancellationToken);
    }

    public async Task AddAsync(Announcement announcement, CancellationToken cancellationToken = default)
    {
        await _context.Announcements.AddAsync(announcement, cancellationToken);
    }

    public void Update(Announcement announcement)
    {
        _context.Announcements.Update(announcement);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var announcement = await _context.Announcements.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (announcement is not null)
        {
            announcement.IsDeleted = true;
            announcement.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Announcement> ApplySorting(IQueryable<Announcement> query, string? sortBy, bool sortDescending)
    {
        return (sortBy?.ToLowerInvariant()) switch
        {
            "title" => sortDescending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
            "publishdate" => sortDescending ? query.OrderByDescending(a => a.PublishDate) : query.OrderBy(a => a.PublishDate),
            "createdat" => sortDescending ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt),
            _ => sortDescending ? query.OrderByDescending(a => a.Id) : query.OrderBy(a => a.Id)
        };
    }
}
