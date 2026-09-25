using LearningManagementSystem.Application.DTOs.Resources;

namespace LearningManagementSystem.Application.Interfaces.Services;

public interface IResourceService
{
    Task<IReadOnlyList<ResourceDto>> GetByLessonIdAsync(int lessonId, CancellationToken cancellationToken = default);

    Task<ResourceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateResourceDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateResourceDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
