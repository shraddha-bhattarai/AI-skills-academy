using AutoMapper;
using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.ContactMessages;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Entities.Communication;

namespace LearningManagementSystem.Application.Services;

public class ContactMessageService : IContactMessageService
{
    private readonly IContactMessageRepository _repository;
    private readonly IMapper _mapper;

    public ContactMessageService(IContactMessageRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<ContactMessageDto>> GetAllAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetPagedAsync(search, sortBy, sortDescending, page, pageSize, cancellationToken);

        return new PagedResult<ContactMessageDto>
        {
            Items = _mapper.Map<IReadOnlyList<ContactMessageDto>>(result.Items),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<ContactMessageDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var message = await _repository.GetByIdAsync(id, cancellationToken);
        return message is null ? null : _mapper.Map<ContactMessageDto>(message);
    }

    public async Task CreateAsync(CreateContactMessageDto dto, CancellationToken cancellationToken = default)
    {
        var message = _mapper.Map<ContactMessage>(dto);
        message.CreatedAt = DateTime.UtcNow;

        await _repository.AddAsync(message, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UpdateContactMessageDto dto, CancellationToken cancellationToken = default)
    {
        var message = await _repository.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new NotFoundException($"Contact message with ID {dto.Id} was not found.");

        _mapper.Map(dto, message);
        message.UpdatedAt = DateTime.UtcNow;

        _repository.Update(message);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await _repository.ExistsAsync(id, cancellationToken))
        {
            throw new NotFoundException($"Contact message with ID {id} was not found.");
        }

        await _repository.SoftDeleteAsync(id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
