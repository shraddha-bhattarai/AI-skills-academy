using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Application.Interfaces.Repositories;

public interface IEnrollmentRepository
{
    Task<PagedResult<Enrollment>> GetPagedAsync(string? search, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Enrollment?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Enrollment?> GetByStudentAndCourseAsync(string studentId, int courseId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Enrollment>> GetByStudentAsync(string studentId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default);

    void Update(Enrollment enrollment);

    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
