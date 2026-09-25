using AutoMapper;
using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Announcements;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Entities.Communication;

namespace LearningManagementSystem.Application.Services;

public class AnnouncementService : IAnnouncementService
{
    private readonly IAnnouncementRepository _repository;
    private readonly IMapper _mapper;

    public AnnouncementService(IAnnouncementRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<AnnouncementDto>> GetAllAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetPagedAsync(search, sortBy, sortDescending, page, pageSize, cancellationToken);

        return new PagedResult<AnnouncementDto>
        {
            Items = _mapper.Map<IReadOnlyList<AnnouncementDto>>(result.Items),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<AnnouncementDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var announcement = await _repository.GetByIdAsync(id, cancellationToken);
        return announcement is null ? null : _mapper.Map<AnnouncementDto>(announcement);
    }

    public async Task CreateAsync(CreateAnnouncementDto dto, CancellationToken cancellationToken = default)
    {
        var announcement = _mapper.Map<Announcement>(dto);
        announcement.CreatedAt = DateTime.UtcNow;

        await _repository.AddAsync(announcement, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UpdateAnnouncementDto dto, CancellationToken cancellationToken = default)
    {
        var announcement = await _repository.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new NotFoundException($"Announcement with ID {dto.Id} was not found.");

        _mapper.Map(dto, announcement);
        announcement.UpdatedAt = DateTime.UtcNow;

        _repository.Update(announcement);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await _repository.ExistsAsync(id, cancellationToken))
        {
            throw new NotFoundException($"Announcement with ID {id} was not found.");
        }

        await _repository.SoftDeleteAsync(id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
