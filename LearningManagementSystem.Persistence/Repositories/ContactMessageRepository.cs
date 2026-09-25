using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Domain.Entities.Communication;
using LearningManagementSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Persistence.Repositories;

public class ContactMessageRepository : IContactMessageRepository
{
    private readonly ApplicationDbContext _context;

    public ContactMessageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ContactMessage>> GetPagedAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ContactMessages.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                c.Name.Contains(term) ||
                c.Email.Contains(term) ||
                c.Subject.Contains(term) ||
                c.Message.Contains(term));
        }

        query = ApplySorting(query, sortBy, sortDescending);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ContactMessage>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ContactMessage?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.ContactMessages.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.ContactMessages.AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task AddAsync(ContactMessage contactMessage, CancellationToken cancellationToken = default)
    {
        await _context.ContactMessages.AddAsync(contactMessage, cancellationToken);
    }

    public void Update(ContactMessage contactMessage)
    {
        _context.ContactMessages.Update(contactMessage);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var contactMessage = await _context.ContactMessages.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (contactMessage is not null)
        {
            contactMessage.IsDeleted = true;
            contactMessage.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<ContactMessage> ApplySorting(IQueryable<ContactMessage> query, string? sortBy, bool sortDescending)
    {
        return (sortBy?.ToLowerInvariant()) switch
        {
            "name" => sortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "email" => sortDescending ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email),
            "subject" => sortDescending ? query.OrderByDescending(c => c.Subject) : query.OrderBy(c => c.Subject),
            "isresolved" => sortDescending ? query.OrderByDescending(c => c.IsResolved) : query.OrderBy(c => c.IsResolved),
            "createdat" => sortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => sortDescending ? query.OrderByDescending(c => c.Id) : query.OrderBy(c => c.Id)
        };
    }
}
