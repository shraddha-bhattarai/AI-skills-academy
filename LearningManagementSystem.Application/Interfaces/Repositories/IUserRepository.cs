using LearningManagementSystem.Application.Common;

namespace LearningManagementSystem.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<IReadOnlyList<LookupItemDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
}
