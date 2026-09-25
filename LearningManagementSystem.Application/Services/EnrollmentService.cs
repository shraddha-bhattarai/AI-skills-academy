using AutoMapper;
using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Enrollments;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _repository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public EnrollmentService(
        IEnrollmentRepository repository,
        ICourseRepository courseRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _repository = repository;
        _courseRepository = courseRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<EnrollmentDto>> GetAllAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetPagedAsync(search, sortBy, sortDescending, page, pageSize, cancellationToken);
        var items = _mapper.Map<IReadOnlyList<EnrollmentDto>>(result.Items);
        var users = await _userRepository.GetAllUsersAsync(cancellationToken);
        UserNameHelper.ApplyUserNames(items, users, e => e.StudentId, (e, name) => e.StudentName = name);

        return new PagedResult<EnrollmentDto>
        {
            Items = items,
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<EnrollmentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var enrollment = await _repository.GetByIdAsync(id, cancellationToken);
        if (enrollment is null)
        {
            return null;
        }

        var dto = _mapper.Map<EnrollmentDto>(enrollment);
        var users = await _userRepository.GetAllUsersAsync(cancellationToken);
        dto.StudentName = UserNameHelper.ResolveUserName(users, dto.StudentId) ?? string.Empty;
        return dto;
    }

    public async Task CreateAsync(CreateEnrollmentDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _courseRepository.ExistsAsync(dto.CourseId, cancellationToken))
        {
            throw new InvalidOperationException("The selected course does not exist.");
        }

        var enrollment = _mapper.Map<Enrollment>(dto);
        enrollment.CreatedAt = DateTime.UtcNow;

        await _repository.AddAsync(enrollment, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UpdateEnrollmentDto dto, CancellationToken cancellationToken = default)
    {
        var enrollment = await _repository.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new NotFoundException($"Enrollment with ID {dto.Id} was not found.");

        if (!await _courseRepository.ExistsAsync(dto.CourseId, cancellationToken))
        {
            throw new InvalidOperationException("The selected course does not exist.");
        }

        _mapper.Map(dto, enrollment);
        enrollment.UpdatedAt = DateTime.UtcNow;

        _repository.Update(enrollment);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await _repository.ExistsAsync(id, cancellationToken))
        {
            throw new NotFoundException($"Enrollment with ID {id} was not found.");
        }

        await _repository.SoftDeleteAsync(id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsStudentEnrolledAsync(string studentId, int courseId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId)) return false;
        var enrollment = await _repository.GetByStudentAndCourseAsync(studentId, courseId, cancellationToken);
        return enrollment is not null;
    }

    public async Task<(bool Success, bool AlreadyEnrolled, string Message)> EnrollStudentAsync(string studentId, int courseId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
        {
            return (false, false, "You must be logged in to enroll.");
        }

        if (!await _courseRepository.ExistsAsync(courseId, cancellationToken))
        {
            return (false, false, "The selected course does not exist.");
        }

        var existing = await _repository.GetByStudentAndCourseAsync(studentId, courseId, cancellationToken);
        if (existing is not null)
        {
            return (true, true, "You are already enrolled in this course.");
        }

        var enrollment = new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrollmentDate = DateTime.UtcNow,
            Progress = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(enrollment, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (true, false, "You have successfully enrolled in this course!");
    }
}
