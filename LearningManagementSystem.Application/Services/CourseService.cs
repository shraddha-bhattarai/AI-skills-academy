using AutoMapper;
using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Courses;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Application.Services;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _repository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CourseService(ICourseRepository repository, ICategoryRepository categoryRepository, IMapper mapper)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<CourseDto>> GetAllAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        int? categoryId = null,
        int? level = null,
        bool publishedOnly = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetPagedAsync(search, sortBy, sortDescending, page, pageSize, categoryId, level, publishedOnly, cancellationToken);

        return new PagedResult<CourseDto>
        {
            Items = _mapper.Map<IReadOnlyList<CourseDto>>(result.Items),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<CourseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var course = await _repository.GetByIdAsync(id, cancellationToken);
        return course is null ? null : _mapper.Map<CourseDto>(course);
    }

    public async Task CreateAsync(CreateCourseDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _categoryRepository.ExistsAsync(dto.CategoryId, cancellationToken))
        {
            throw new InvalidOperationException("The selected category does not exist.");
        }

        var course = _mapper.Map<Course>(dto);
        course.CreatedAt = DateTime.UtcNow;

        await _repository.AddAsync(course, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UpdateCourseDto dto, CancellationToken cancellationToken = default)
    {
        var course = await _repository.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new NotFoundException($"Course with ID {dto.Id} was not found.");

        if (!await _categoryRepository.ExistsAsync(dto.CategoryId, cancellationToken))
        {
            throw new InvalidOperationException("The selected category does not exist.");
        }

        _mapper.Map(dto, course);
        course.UpdatedAt = DateTime.UtcNow;

        _repository.Update(course);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await _repository.ExistsAsync(id, cancellationToken))
        {
            throw new NotFoundException($"Course with ID {id} was not found.");
        }

        await _repository.SoftDeleteAsync(id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<LookupItemDto>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        return _repository.GetLookupAsync(cancellationToken);
    }
}
