using AutoMapper;
using LearningManagementSystem.Application.DTOs.Resources;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Application.Services;

public class ResourceService : IResourceService
{
    private readonly IResourceRepository _repository;
    private readonly ILessonRepository _lessonRepository;
    private readonly IMapper _mapper;

    public ResourceService(IResourceRepository repository, ILessonRepository lessonRepository, IMapper mapper)
    {
        _repository = repository;
        _lessonRepository = lessonRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ResourceDto>> GetByLessonIdAsync(int lessonId, CancellationToken cancellationToken = default)
    {
        var resources = await _repository.GetByLessonIdAsync(lessonId, cancellationToken);
        return _mapper.Map<IReadOnlyList<ResourceDto>>(resources);
    }

    public async Task<ResourceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var resource = await _repository.GetByIdAsync(id, cancellationToken);
        return resource is null ? null : _mapper.Map<ResourceDto>(resource);
    }

    public async Task CreateAsync(CreateResourceDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _lessonRepository.ExistsAsync(dto.LessonId, cancellationToken))
        {
            throw new InvalidOperationException("The selected lesson does not exist.");
        }

        var resource = _mapper.Map<Resource>(dto);
        resource.CreatedAt = DateTime.UtcNow;

        await _repository.AddAsync(resource, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UpdateResourceDto dto, CancellationToken cancellationToken = default)
    {
        var resource = await _repository.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new NotFoundException($"Resource with ID {dto.Id} was not found.");

        if (!await _lessonRepository.ExistsAsync(dto.LessonId, cancellationToken))
        {
            throw new InvalidOperationException("The selected lesson does not exist.");
        }

        _mapper.Map(dto, resource);
        resource.UpdatedAt = DateTime.UtcNow;

        _repository.Update(resource);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var resource = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Resource with ID {id} was not found.");

        await _repository.SoftDeleteAsync(id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
