using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Assignments;

namespace LearningManagementSystem.Application.Interfaces.Services;

public interface IAssignmentService
{
    Task<PagedResult<AssignmentDto>> GetAllAsync(string? search, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<AssignmentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateAssignmentDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateAssignmentDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupItemDto>> GetLookupAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentAssignmentDto>> GetStudentAssignmentsAsync(string studentId, CancellationToken cancellationToken = default);

    Task<StudentAssignmentDto?> GetStudentAssignmentDetailsAsync(int assignmentId, string studentId, CancellationToken cancellationToken = default);

    Task<bool> SubmitAssignmentAsync(int assignmentId, string studentId, string? filePath, CancellationToken cancellationToken = default);
}
