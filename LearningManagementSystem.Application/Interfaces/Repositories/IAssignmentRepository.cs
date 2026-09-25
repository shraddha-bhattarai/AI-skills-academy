using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Domain.Entities.Assignments;

namespace LearningManagementSystem.Application.Interfaces.Repositories;

public interface IAssignmentRepository
{
    Task<PagedResult<Assignment>> GetPagedAsync(string? search, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Assignment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Assignment assignment, CancellationToken cancellationToken = default);

    void Update(Assignment assignment);

    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupItemDto>> GetLookupAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Assignment>> GetAssignmentsByCourseIdsAsync(IEnumerable<int> courseIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Submission>> GetStudentSubmissionsForAssignmentsAsync(string studentId, IEnumerable<int> assignmentIds, CancellationToken cancellationToken = default);

    Task<bool> IsStudentEnrolledInCourseAsync(string studentId, int courseId, CancellationToken cancellationToken = default);

    Task<Submission?> GetStudentSubmissionAsync(int assignmentId, string studentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetStudentEnrolledCourseIdsAsync(string studentId, CancellationToken cancellationToken = default);
}
