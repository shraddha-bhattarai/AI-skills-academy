using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Domain.Entities.Courses;
using LearningManagementSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Persistence.Repositories;

public class DiscussionRepository : IDiscussionRepository
{
    private readonly ApplicationDbContext _context;

    public DiscussionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Discussion>> GetByCourseIdAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Discussions
            .Include(d => d.Course)
            .Include(d => d.Replies)
            .Where(d => d.CourseId == courseId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Discussion?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Discussions
            .Include(d => d.Course)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Discussion?> GetByIdWithRepliesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Discussions
            .Include(d => d.Course)
            .Include(d => d.Replies.OrderBy(r => r.CreatedAt))
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task AddAsync(Discussion discussion, CancellationToken cancellationToken = default)
    {
        await _context.Discussions.AddAsync(discussion, cancellationToken);
    }

    public async Task AddReplyAsync(DiscussionReply reply, CancellationToken cancellationToken = default)
    {
        await _context.DiscussionReplies.AddAsync(reply, cancellationToken);
    }

    public async Task<DiscussionReply?> GetReplyByIdAsync(int replyId, CancellationToken cancellationToken = default)
    {
        return await _context.DiscussionReplies.FirstOrDefaultAsync(r => r.Id == replyId, cancellationToken);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var discussion = await _context.Discussions.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (discussion is not null)
        {
            discussion.IsDeleted = true;
            discussion.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task SoftDeleteReplyAsync(int replyId, CancellationToken cancellationToken = default)
    {
        var reply = await _context.DiscussionReplies.FirstOrDefaultAsync(r => r.Id == replyId, cancellationToken);
        if (reply is not null)
        {
            reply.IsDeleted = true;
            reply.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task<PagedResult<Discussion>> GetPagedForModerationAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Discussions
            .Include(d => d.Course)
            .Include(d => d.Replies)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(d => d.Title.Contains(term) || d.Content.Contains(term) || d.Course.Title.Contains(term));
        }

        query = query.OrderByDescending(d => d.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Discussion>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
