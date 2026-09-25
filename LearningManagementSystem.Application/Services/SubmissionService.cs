using AutoMapper;
using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Submissions;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Entities.Assignments;
using LearningManagementSystem.Domain.Enums;

namespace LearningManagementSystem.Application.Services;

public class SubmissionService : ISubmissionService
{
    private readonly ISubmissionRepository _repository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICertificateEligibilityService _certificateEligibilityService;
    private readonly IMapper _mapper;

    public SubmissionService(
        ISubmissionRepository repository,
        IAssignmentRepository assignmentRepository,
        IUserRepository userRepository,
        ICertificateEligibilityService certificateEligibilityService,
        IMapper mapper)
    {
        _repository = repository;
        _assignmentRepository = assignmentRepository;
        _userRepository = userRepository;
        _certificateEligibilityService = certificateEligibilityService;
        _mapper = mapper;
    }

    public async Task<PagedResult<SubmissionDto>> GetAllAsync(
        string? search,
        SubmissionStatus? status,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetPagedAsync(search, status, sortBy, sortDescending, page, pageSize, cancellationToken);
        var items = _mapper.Map<IReadOnlyList<SubmissionDto>>(result.Items);
        var users = await _userRepository.GetAllUsersAsync(cancellationToken);
        UserNameHelper.ApplyUserNames(items, users, s => s.StudentId, (s, name) => s.StudentName = name);

        return new PagedResult<SubmissionDto>
        {
            Items = items,
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<SubmissionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var submission = await _repository.GetByIdAsync(id, cancellationToken);
        if (submission is null)
        {
            return null;
        }

        var dto = _mapper.Map<SubmissionDto>(submission);
        var users = await _userRepository.GetAllUsersAsync(cancellationToken);
        dto.StudentName = UserNameHelper.ResolveUserName(users, dto.StudentId) ?? string.Empty;
        return dto;
    }

    public async Task CreateAsync(CreateSubmissionDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _assignmentRepository.ExistsAsync(dto.AssignmentId, cancellationToken))
        {
            throw new InvalidOperationException("The selected assignment does not exist.");
        }

        var submission = _mapper.Map<Submission>(dto);
        submission.CreatedAt = DateTime.UtcNow;

        await _repository.AddAsync(submission, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UpdateSubmissionDto dto, CancellationToken cancellationToken = default)
    {
        var submission = await _repository.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new NotFoundException($"Submission with ID {dto.Id} was not found.");

        if (!await _assignmentRepository.ExistsAsync(dto.AssignmentId, cancellationToken))
        {
            throw new InvalidOperationException("The selected assignment does not exist.");
        }

        _mapper.Map(dto, submission);
        submission.UpdatedAt = DateTime.UtcNow;

        _repository.Update(submission);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await _repository.ExistsAsync(id, cancellationToken))
        {
            throw new NotFoundException($"Submission with ID {id} was not found.");
        }

        await _repository.SoftDeleteAsync(id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task GradeAsync(GradeSubmissionDto dto, CancellationToken cancellationToken = default)
    {
        var submission = await _repository.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new NotFoundException($"Submission with ID {dto.Id} was not found.");

        submission.Marks = dto.Marks;
        submission.Status = dto.Status;
        submission.Feedback = dto.Feedback;
        submission.UpdatedAt = DateTime.UtcNow;

        _repository.Update(submission);
        await _repository.SaveChangesAsync(cancellationToken);

        if (dto.Status == SubmissionStatus.Graded)
        {
            await _certificateEligibilityService.CheckAndIssueAsync(submission.StudentId, submission.Assignment.CourseId, cancellationToken);
        }
    }
}
