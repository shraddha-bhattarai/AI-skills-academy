using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.AiTools;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Application.Services;

public class AiToolService : IAiToolService
{
    private readonly IAiToolRepository _repository;
    private readonly IUserRepository _userRepository;

    public AiToolService(IAiToolRepository repository, IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<AiToolDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tools = await _repository.GetAllAsync(cancellationToken);
        return tools.Select(MapToDto).ToList();
    }

    public async Task<PagedResult<AiToolDto>> GetAllForAdminAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetPagedAsync(search, page, pageSize, cancellationToken);
        return new PagedResult<AiToolDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<AiToolDetailsDto?> GetByIdAsync(int id, string? studentId, CancellationToken cancellationToken = default)
    {
        var tool = await _repository.GetByIdAsync(id, cancellationToken);
        if (tool is null)
        {
            return null;
        }

        var feedback = await _repository.GetFeedbackForToolAsync(id, cancellationToken);
        var users = await _userRepository.GetAllUsersAsync(cancellationToken);

        var markedUseful = !string.IsNullOrWhiteSpace(studentId) &&
            feedback.Any(f => f.StudentId == studentId && f.IsUseful);

        var baseDto = MapToDto(tool);

        return new AiToolDetailsDto
        {
            Id = baseDto.Id,
            Name = baseDto.Name,
            Description = baseDto.Description,
            Url = baseDto.Url,
            Category = baseDto.Category,
            UsefulCount = baseDto.UsefulCount,
            CreatedAt = baseDto.CreatedAt,
            MarkedUsefulByCurrentUser = markedUseful,
            Comments = feedback
                .Where(f => !string.IsNullOrWhiteSpace(f.Comment))
                .Select(f => new AiToolFeedbackDto
                {
                    StudentName = UserNameHelper.ResolveUserName(users, f.StudentId) ?? "Anonymous Student",
                    IsUseful = f.IsUseful,
                    Comment = f.Comment,
                    CreatedAt = f.CreatedAt
                }).ToList()
        };
    }

    public async Task CreateAsync(CreateAiToolDto dto, CancellationToken cancellationToken = default)
    {
        var tool = new AITool
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            Url = dto.Url.Trim(),
            Category = dto.Category?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(tool, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UpdateAiToolDto dto, CancellationToken cancellationToken = default)
    {
        var tool = await _repository.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new NotFoundException($"AI tool with ID {dto.Id} was not found.");

        tool.Name = dto.Name.Trim();
        tool.Description = dto.Description.Trim();
        tool.Url = dto.Url.Trim();
        tool.Category = dto.Category?.Trim();
        tool.UpdatedAt = DateTime.UtcNow;

        _repository.Update(tool);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await _repository.ExistsAsync(id, cancellationToken))
        {
            throw new NotFoundException($"AI tool with ID {id} was not found.");
        }

        await _repository.SoftDeleteAsync(id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<(bool Success, string Message)> MarkFeedbackAsync(MarkAiToolFeedbackDto dto, string studentId, CancellationToken cancellationToken = default)
    {
        if (!await _repository.ExistsAsync(dto.AIToolId, cancellationToken))
        {
            return (false, "AI tool not found.");
        }

        var existing = await _repository.GetFeedbackAsync(dto.AIToolId, studentId, cancellationToken);
        if (existing is not null)
        {
            existing.IsUseful = dto.IsUseful;
            existing.Comment = string.IsNullOrWhiteSpace(dto.Comment) ? existing.Comment : dto.Comment.Trim();
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            await _repository.AddFeedbackAsync(new AIToolFeedback
            {
                AIToolId = dto.AIToolId,
                StudentId = studentId,
                IsUseful = dto.IsUseful,
                Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim(),
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return (true, "Thanks for your feedback!");
    }

    private static AiToolDto MapToDto(AITool tool)
    {
        return new AiToolDto
        {
            Id = tool.Id,
            Name = tool.Name,
            Description = tool.Description,
            Url = tool.Url,
            Category = tool.Category,
            UsefulCount = tool.Feedback?.Count(f => f.IsUseful && !f.IsDeleted) ?? 0,
            CreatedAt = tool.CreatedAt
        };
    }
}
