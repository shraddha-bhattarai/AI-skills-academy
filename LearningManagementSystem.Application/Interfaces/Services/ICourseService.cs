using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Courses;

namespace LearningManagementSystem.Application.Interfaces.Services;

public interface ICourseService
{
    Task<PagedResult<CourseDto>> GetAllAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        int? categoryId = null,
        int? level = null,
        bool publishedOnly = false,
        CancellationToken cancellationToken = default);

    Task<CourseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateCourseDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateCourseDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupItemDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
