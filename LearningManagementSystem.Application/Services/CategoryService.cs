using AutoMapper;
using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Categories;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<CategoryDto>> GetAllAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetPagedAsync(search, sortBy, sortDescending, page, pageSize, cancellationToken);

        return new PagedResult<CategoryDto>
        {
            Items = _mapper.Map<IReadOnlyList<CategoryDto>>(result.Items),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _repository.GetByIdAsync(id, cancellationToken);
        return category is null ? null : _mapper.Map<CategoryDto>(category);
    }

    public async Task CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        if (await _repository.NameExistsAsync(dto.Name, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("A category with this name already exists.");
        }

        var category = _mapper.Map<Category>(dto);
        category.CreatedAt = DateTime.UtcNow;

        await _repository.AddAsync(category, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = await _repository.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new NotFoundException($"Category with ID {dto.Id} was not found.");

        if (await _repository.NameExistsAsync(dto.Name, dto.Id, cancellationToken))
        {
            throw new InvalidOperationException("A category with this name already exists.");
        }

        _mapper.Map(dto, category);
        category.UpdatedAt = DateTime.UtcNow;

        _repository.Update(category);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await _repository.ExistsAsync(id, cancellationToken))
        {
            throw new NotFoundException($"Category with ID {id} was not found.");
        }

        await _repository.SoftDeleteAsync(id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<LookupItemDto>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        return _repository.GetLookupAsync(cancellationToken);
    }
}
