using AutoMapper;
using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Notifications;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Entities.Communication;

namespace LearningManagementSystem.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public NotificationService(INotificationRepository repository, IUserRepository userRepository, IMapper mapper)
    {
        _repository = repository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<NotificationDto>> GetAllAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetPagedAsync(search, sortBy, sortDescending, page, pageSize, cancellationToken);
        var items = _mapper.Map<IReadOnlyList<NotificationDto>>(result.Items);
        var users = await _userRepository.GetAllUsersAsync(cancellationToken);
        UserNameHelper.ApplyUserNames(items, users, n => n.UserId, (n, name) => n.UserName = name);

        return new PagedResult<NotificationDto>
        {
            Items = items,
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<NotificationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var notification = await _repository.GetByIdAsync(id, cancellationToken);
        if (notification is null)
        {
            return null;
        }

        var dto = _mapper.Map<NotificationDto>(notification);
        var users = await _userRepository.GetAllUsersAsync(cancellationToken);
        dto.UserName = UserNameHelper.ResolveUserName(users, dto.UserId) ?? string.Empty;
        return dto;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetStudentNotificationsAsync(string userId, string? filter = "All", CancellationToken cancellationToken = default)
    {
        var list = await _repository.GetByUserIdAsync(userId, filter, cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<NotificationDto>>(list);
        var users = await _userRepository.GetAllUsersAsync(cancellationToken);
        UserNameHelper.ApplyUserNames(dtos, users, n => n.UserId, (n, name) => n.UserName = name);
        return dtos;
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetUnreadCountByUserIdAsync(userId, cancellationToken);
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, string userId, CancellationToken cancellationToken = default)
    {
        return await _repository.MarkAsReadAsync(notificationId, userId, cancellationToken);
    }

    public async Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _repository.MarkAllAsReadAsync(userId, cancellationToken);
    }

    public async Task<NotificationDto?> GetStudentNotificationByIdAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var notification = await _repository.GetByIdAsync(id, cancellationToken);
        if (notification is null || notification.UserId != userId || notification.IsDeleted)
        {
            return null;
        }

        var dto = _mapper.Map<NotificationDto>(notification);
        var users = await _userRepository.GetAllUsersAsync(cancellationToken);
        dto.UserName = UserNameHelper.ResolveUserName(users, dto.UserId) ?? string.Empty;
        return dto;
    }

    public async Task CreateAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default)
    {
        var notification = _mapper.Map<Notification>(dto);
        notification.CreatedAt = DateTime.UtcNow;

        await _repository.AddAsync(notification, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UpdateNotificationDto dto, CancellationToken cancellationToken = default)
    {
        var notification = await _repository.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new NotFoundException($"Notification with ID {dto.Id} was not found.");

        _mapper.Map(dto, notification);
        notification.UpdatedAt = DateTime.UtcNow;

        _repository.Update(notification);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await _repository.ExistsAsync(id, cancellationToken))
        {
            throw new NotFoundException($"Notification with ID {id} was not found.");
        }

        await _repository.SoftDeleteAsync(id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
