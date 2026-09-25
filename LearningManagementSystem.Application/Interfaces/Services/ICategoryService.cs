using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Categories;

namespace LearningManagementSystem.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<PagedResult<CategoryDto>> GetAllAsync(string? search, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateCategoryDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupItemDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
