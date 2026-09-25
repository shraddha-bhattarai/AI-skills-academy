using AutoMapper;
using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Certificates;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Entities.Certificates;

namespace LearningManagementSystem.Application.Services;

public class CertificateService : ICertificateService
{
    private readonly ICertificateRepository _repository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public CertificateService(
        ICertificateRepository repository,
        ICourseRepository courseRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _repository = repository;
        _courseRepository = courseRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<CertificateDto>> GetAllAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetPagedAsync(search, sortBy, sortDescending, page, pageSize, cancellationToken);
        var items = _mapper.Map<IReadOnlyList<CertificateDto>>(result.Items);
        var users = await _userRepository.GetAllUsersAsync(cancellationToken);
        UserNameHelper.ApplyUserNames(items, users, c => c.StudentId, (c, name) => c.StudentName = name);

        return new PagedResult<CertificateDto>
        {
            Items = items,
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<CertificateDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var certificate = await _repository.GetByIdAsync(id, cancellationToken);
        if (certificate is null)
        {
            return null;
        }

        var dto = _mapper.Map<CertificateDto>(certificate);
        var users = await _userRepository.GetAllUsersAsync(cancellationToken);
        dto.StudentName = UserNameHelper.ResolveUserName(users, dto.StudentId) ?? string.Empty;
        return dto;
    }

    public async Task<IReadOnlyList<CertificateDto>> GetStudentCertificatesAsync(string studentId, CancellationToken cancellationToken = default)
    {
        var certificates = await _repository.GetByStudentIdAsync(studentId, cancellationToken);
        var dtos = _mapper.Map<IReadOnlyList<CertificateDto>>(certificates);
        var users = await _userRepository.GetAllUsersAsync(cancellationToken);
        UserNameHelper.ApplyUserNames(dtos, users, c => c.StudentId, (c, name) => c.StudentName = name);
        return dtos;
    }

    public async Task<CertificateDto?> GetStudentCertificateByIdAsync(int id, string studentId, CancellationToken cancellationToken = default)
    {
        var certificate = await _repository.GetByIdAsync(id, cancellationToken);
        if (certificate is null || certificate.StudentId != studentId)
        {
            return null;
        }

        var dto = _mapper.Map<CertificateDto>(certificate);
        var users = await _userRepository.GetAllUsersAsync(cancellationToken);
        dto.StudentName = UserNameHelper.ResolveUserName(users, dto.StudentId) ?? string.Empty;
        return dto;
    }

    public async Task CreateAsync(CreateCertificateDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _courseRepository.ExistsAsync(dto.CourseId, cancellationToken))
        {
            throw new InvalidOperationException("The selected course does not exist.");
        }

        var certificate = _mapper.Map<Certificate>(dto);
        certificate.CreatedAt = DateTime.UtcNow;

        await _repository.AddAsync(certificate, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UpdateCertificateDto dto, CancellationToken cancellationToken = default)
    {
        var certificate = await _repository.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new NotFoundException($"Certificate with ID {dto.Id} was not found.");

        if (!await _courseRepository.ExistsAsync(dto.CourseId, cancellationToken))
        {
            throw new InvalidOperationException("The selected course does not exist.");
        }

        _mapper.Map(dto, certificate);
        certificate.UpdatedAt = DateTime.UtcNow;

        _repository.Update(certificate);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await _repository.ExistsAsync(id, cancellationToken))
        {
            throw new NotFoundException($"Certificate with ID {id} was not found.");
        }

        await _repository.SoftDeleteAsync(id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
