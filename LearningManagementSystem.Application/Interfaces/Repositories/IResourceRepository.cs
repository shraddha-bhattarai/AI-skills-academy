using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Application.Interfaces.Repositories;

public interface IResourceRepository
{
    Task<IReadOnlyList<Resource>> GetByLessonIdAsync(int lessonId, CancellationToken cancellationToken = default);

    Task<Resource?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Resource resource, CancellationToken cancellationToken = default);

    void Update(Resource resource);

    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
