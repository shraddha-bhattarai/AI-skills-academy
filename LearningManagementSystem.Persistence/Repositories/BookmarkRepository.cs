using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Domain.Entities.Courses;
using LearningManagementSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Persistence.Repositories;

public class BookmarkRepository : IBookmarkRepository
{
    private readonly ApplicationDbContext _context;

    public BookmarkRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Bookmark>> GetByStudentIdAsync(string studentId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookmarks
            .Include(b => b.Course)
            .Where(b => b.StudentId == studentId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Bookmark?> GetAsync(string studentId, int courseId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookmarks
            .FirstOrDefaultAsync(b => b.StudentId == studentId && b.CourseId == courseId, cancellationToken);
    }

    public async Task AddAsync(Bookmark bookmark, CancellationToken cancellationToken = default)
    {
        await _context.Bookmarks.AddAsync(bookmark, cancellationToken);
    }

    public Task RemoveAsync(Bookmark bookmark, CancellationToken cancellationToken = default)
    {
        bookmark.IsDeleted = true;
        bookmark.UpdatedAt = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
