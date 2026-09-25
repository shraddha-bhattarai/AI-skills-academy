using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Discussions;

namespace LearningManagementSystem.Application.Interfaces.Services;

public interface IDiscussionService
{
    Task<IReadOnlyList<DiscussionDto>?> GetCourseDiscussionsAsync(int courseId, string studentId, CancellationToken cancellationToken = default);

    Task<DiscussionDetailsDto?> GetDiscussionDetailsAsync(int discussionId, string studentId, CancellationToken cancellationToken = default);

    Task<(bool Success, string Message)> CreateDiscussionAsync(CreateDiscussionDto dto, string studentId, CancellationToken cancellationToken = default);

    Task<(bool Success, string Message)> CreateReplyAsync(CreateDiscussionReplyDto dto, string studentId, CancellationToken cancellationToken = default);

    Task<PagedResult<DiscussionDto>> GetAllForModerationAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<DiscussionDetailsDto?> GetForModerationAsync(int discussionId, CancellationToken cancellationToken = default);

    Task DeleteDiscussionAsync(int id, CancellationToken cancellationToken = default);

    Task DeleteReplyAsync(int replyId, CancellationToken cancellationToken = default);
}
