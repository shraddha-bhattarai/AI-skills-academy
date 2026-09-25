using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Enrollments;

namespace LearningManagementSystem.Application.Interfaces.Services;

public interface IEnrollmentService
{
    Task<PagedResult<EnrollmentDto>> GetAllAsync(string? search, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<EnrollmentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateEnrollmentDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateEnrollmentDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> IsStudentEnrolledAsync(string studentId, int courseId, CancellationToken cancellationToken = default);

    Task<(bool Success, bool AlreadyEnrolled, string Message)> EnrollStudentAsync(string studentId, int courseId, CancellationToken cancellationToken = default);
}
