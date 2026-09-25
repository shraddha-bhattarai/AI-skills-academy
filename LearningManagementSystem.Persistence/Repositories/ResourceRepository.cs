using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Domain.Entities.Courses;
using LearningManagementSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Persistence.Repositories;

public class ResourceRepository : IResourceRepository
{
    private readonly ApplicationDbContext _context;

    public ResourceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Resource>> GetByLessonIdAsync(int lessonId, CancellationToken cancellationToken = default)
    {
        return await _context.Resources
            .Include(r => r.Lesson)
            .Where(r => r.LessonId == lessonId)
            .OrderBy(r => r.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Resource?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Resources
            .Include(r => r.Lesson)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task AddAsync(Resource resource, CancellationToken cancellationToken = default)
    {
        await _context.Resources.AddAsync(resource, cancellationToken);
    }

    public void Update(Resource resource)
    {
        _context.Resources.Update(resource);
    }

    public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var resource = await _context.Resources.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (resource is not null)
        {
            resource.IsDeleted = true;
            resource.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
