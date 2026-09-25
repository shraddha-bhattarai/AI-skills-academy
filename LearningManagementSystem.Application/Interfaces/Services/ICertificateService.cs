using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Certificates;

namespace LearningManagementSystem.Application.Interfaces.Services;

public interface ICertificateService
{
    Task<PagedResult<CertificateDto>> GetAllAsync(string? search, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<CertificateDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CertificateDto>> GetStudentCertificatesAsync(string studentId, CancellationToken cancellationToken = default);

    Task<CertificateDto?> GetStudentCertificateByIdAsync(int id, string studentId, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateCertificateDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateCertificateDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
