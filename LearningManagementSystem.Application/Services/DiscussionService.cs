using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Discussions;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Application.Services;

public class DiscussionService : IDiscussionService
{
    private readonly IDiscussionRepository _repository;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IUserRepository _userRepository;

    public DiscussionService(IDiscussionRepository repository, IEnrollmentService enrollmentService, IUserRepository userRepository)
    {
        _repository = repository;
        _enrollmentService = enrollmentService;
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<DiscussionDto>?> GetCourseDiscussionsAsync(int courseId, string studentId, CancellationToken cancellationToken = default)
    {
        if (!await _enrollmentService.IsStudentEnrolledAsync(studentId, courseId, cancellationToken))
        {
            return null;
        }

        var discussions = await _repository.GetByCourseIdAsync(courseId, cancellationToken);
        var users = await _userRepository.GetAllUsersAsync(cancellationToken);

        return discussions.Select(d => MapToDto(d, users)).ToList();
    }

    public async Task<DiscussionDetailsDto?> GetDiscussionDetailsAsync(int discussionId, string studentId, CancellationToken cancellationToken = default)
    {
        var discussion = await _repository.GetByIdWithRepliesAsync(discussionId, cancellationToken);
        if (discussion is null)
        {
            return null;
        }

        if (!await _enrollmentService.IsStudentEnrolledAsync(studentId, discussion.CourseId, cancellationToken))
        {
            return null;
        }

        var users = await _userRepository.GetAllUsersAsync(cancellationToken);
        return MapToDetailsDto(discussion, users);
    }

    public async Task<(bool Success, string Message)> CreateDiscussionAsync(CreateDiscussionDto dto, string studentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
        {
            return (false, "You must be logged in to start a discussion.");
        }

        if (!await _enrollmentService.IsStudentEnrolledAsync(studentId, dto.CourseId, cancellationToken))
        {
            return (false, "You must be enrolled in this course to start a discussion.");
        }

        var discussion = new Discussion
        {
            CourseId = dto.CourseId,
            StudentId = studentId,
            Title = dto.Title.Trim(),
            Content = dto.Content.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(discussion, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (true, "Discussion posted.");
    }

    public async Task<(bool Success, string Message)> CreateReplyAsync(CreateDiscussionReplyDto dto, string studentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
        {
            return (false, "You must be logged in to reply.");
        }

        var discussion = await _repository.GetByIdAsync(dto.DiscussionId, cancellationToken);
        if (discussion is null)
        {
            return (false, "Discussion not found.");
        }

        if (!await _enrollmentService.IsStudentEnrolledAsync(studentId, discussion.CourseId, cancellationToken))
        {
            return (false, "You must be enrolled in this course to reply.");
        }

        var reply = new DiscussionReply
        {
            DiscussionId = dto.DiscussionId,
            StudentId = studentId,
            Content = dto.Content.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddReplyAsync(reply, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (true, "Reply posted.");
    }

    public async Task<PagedResult<DiscussionDto>> GetAllForModerationAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetPagedForModerationAsync(search, page, pageSize, cancellationToken);
        var users = await _userRepository.GetAllUsersAsync(cancellationToken);

        return new PagedResult<DiscussionDto>
        {
            Items = result.Items.Select(d => MapToDto(d, users)).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<DiscussionDetailsDto?> GetForModerationAsync(int discussionId, CancellationToken cancellationToken = default)
    {
        var discussion = await _repository.GetByIdWithRepliesAsync(discussionId, cancellationToken);
        if (discussion is null)
        {
            return null;
        }

        var users = await _userRepository.GetAllUsersAsync(cancellationToken);
        return MapToDetailsDto(discussion, users);
    }

    public async Task DeleteDiscussionAsync(int id, CancellationToken cancellationToken = default)
    {
        await _repository.SoftDeleteAsync(id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteReplyAsync(int replyId, CancellationToken cancellationToken = default)
    {
        await _repository.SoftDeleteReplyAsync(replyId, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private static DiscussionDto MapToDto(Discussion discussion, IReadOnlyList<LookupItemDto> users)
    {
        return new DiscussionDto
        {
            Id = discussion.Id,
            CourseId = discussion.CourseId,
            CourseTitle = discussion.Course?.Title ?? string.Empty,
            StudentId = discussion.StudentId,
            StudentName = UserNameHelper.ResolveUserName(users, discussion.StudentId) ?? "Unknown Student",
            Title = discussion.Title,
            Content = discussion.Content,
            ReplyCount = discussion.Replies?.Count(r => !r.IsDeleted) ?? 0,
            CreatedAt = discussion.CreatedAt
        };
    }

    private static DiscussionDetailsDto MapToDetailsDto(Discussion discussion, IReadOnlyList<LookupItemDto> users)
    {
        var dto = MapToDto(discussion, users);
        return new DiscussionDetailsDto
        {
            Id = dto.Id,
            CourseId = dto.CourseId,
            CourseTitle = dto.CourseTitle,
            StudentId = dto.StudentId,
            StudentName = dto.StudentName,
            Title = dto.Title,
            Content = dto.Content,
            ReplyCount = dto.ReplyCount,
            CreatedAt = dto.CreatedAt,
            Replies = discussion.Replies
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.CreatedAt)
                .Select(r => new DiscussionReplyDto
                {
                    Id = r.Id,
                    DiscussionId = r.DiscussionId,
                    StudentId = r.StudentId,
                    StudentName = UserNameHelper.ResolveUserName(users, r.StudentId) ?? "Unknown Student",
                    Content = r.Content,
                    CreatedAt = r.CreatedAt
                }).ToList()
        };
    }
}
