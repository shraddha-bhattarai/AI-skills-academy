using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Domain.Entities.Certificates;

namespace LearningManagementSystem.Application.Interfaces.Repositories;

public interface ICertificateRepository
{
    Task<PagedResult<Certificate>> GetPagedAsync(string? search, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<Certificate?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Certificate>> GetByStudentIdAsync(string studentId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExistsForStudentAndCourseAsync(string studentId, int courseId, CancellationToken cancellationToken = default);

    Task AddAsync(Certificate certificate, CancellationToken cancellationToken = default);

    void Update(Certificate certificate);

    Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
