using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Application.Interfaces.Repositories;

public interface IDiscussionRepository
{
    Task<IReadOnlyList<Discussion>> GetByCourseIdAsync(int courseId, CancellationToken cancellationToken = default);

    Task<Discussion?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Discussion?> GetByIdWithRepliesAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Discussion discussion, CancellationToken cancellationToken = default);

    Task AddReplyAsync(DiscussionReply reply, CancellationToken cancellationToken = default);

    Task<DiscussionReply?> GetReplyByIdAsync(int replyId, CancellationToken cancellationToken = default);

    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    Task SoftDeleteReplyAsync(int replyId, CancellationToken cancellationToken = default);

    Task<PagedResult<Discussion>> GetPagedForModerationAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
