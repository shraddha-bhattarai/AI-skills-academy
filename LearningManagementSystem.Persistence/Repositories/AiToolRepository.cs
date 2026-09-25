using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Domain.Entities.Courses;
using LearningManagementSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Persistence.Repositories;

public class AiToolRepository : IAiToolRepository
{
    private readonly ApplicationDbContext _context;

    public AiToolRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AITool>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.AITools.Include(t => t.Feedback).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(t => t.Name.Contains(term) || t.Description.Contains(term) || (t.Category != null && t.Category.Contains(term)));
        }

        query = query.OrderBy(t => t.Name);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<AITool> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<IReadOnlyList<AITool>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AITools.Include(t => t.Feedback).OrderBy(t => t.Name).ToListAsync(cancellationToken);
    }

    public async Task<AITool?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.AITools.Include(t => t.Feedback).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.AITools.AnyAsync(t => t.Id == id, cancellationToken);
    }

    public async Task AddAsync(AITool tool, CancellationToken cancellationToken = default)
    {
        await _context.AITools.AddAsync(tool, cancellationToken);
    }

    public void Update(AITool tool)
    {
        _context.AITools.Update(tool);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var tool = await _context.AITools.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (tool is not null)
        {
            tool.IsDeleted = true;
            tool.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<AIToolFeedback?> GetFeedbackAsync(int toolId, string studentId, CancellationToken cancellationToken = default)
    {
        return await _context.AIToolFeedback.FirstOrDefaultAsync(f => f.AIToolId == toolId && f.StudentId == studentId, cancellationToken);
    }

    public async Task AddFeedbackAsync(AIToolFeedback feedback, CancellationToken cancellationToken = default)
    {
        await _context.AIToolFeedback.AddAsync(feedback, cancellationToken);
    }

    public async Task<IReadOnlyList<AIToolFeedback>> GetFeedbackForToolAsync(int toolId, CancellationToken cancellationToken = default)
    {
        return await _context.AIToolFeedback
            .Where(f => f.AIToolId == toolId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
