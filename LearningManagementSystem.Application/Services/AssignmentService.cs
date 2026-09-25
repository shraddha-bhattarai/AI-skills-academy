using AutoMapper;
using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Assignments;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Entities.Assignments;

namespace LearningManagementSystem.Application.Services;

public class AssignmentService : IAssignmentService
{
    private readonly IAssignmentRepository _repository;
    private readonly ICourseRepository _courseRepository;
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IMapper _mapper;

    public AssignmentService(
        IAssignmentRepository repository,
        ICourseRepository courseRepository,
        ISubmissionRepository submissionRepository,
        IMapper mapper)
    {
        _repository = repository;
        _courseRepository = courseRepository;
        _submissionRepository = submissionRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<AssignmentDto>> GetAllAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetPagedAsync(search, sortBy, sortDescending, page, pageSize, cancellationToken);

        return new PagedResult<AssignmentDto>
        {
            Items = _mapper.Map<IReadOnlyList<AssignmentDto>>(result.Items),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<AssignmentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var assignment = await _repository.GetByIdAsync(id, cancellationToken);
        return assignment is null ? null : _mapper.Map<AssignmentDto>(assignment);
    }

    public async Task CreateAsync(CreateAssignmentDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _courseRepository.ExistsAsync(dto.CourseId, cancellationToken))
        {
            throw new InvalidOperationException("The selected course does not exist.");
        }

        var assignment = _mapper.Map<Assignment>(dto);
        assignment.CreatedAt = DateTime.UtcNow;

        await _repository.AddAsync(assignment, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UpdateAssignmentDto dto, CancellationToken cancellationToken = default)
    {
        var assignment = await _repository.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new NotFoundException($"Assignment with ID {dto.Id} was not found.");

        if (!await _courseRepository.ExistsAsync(dto.CourseId, cancellationToken))
        {
            throw new InvalidOperationException("The selected course does not exist.");
        }

        _mapper.Map(dto, assignment);
        assignment.UpdatedAt = DateTime.UtcNow;

        _repository.Update(assignment);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await _repository.ExistsAsync(id, cancellationToken))
        {
            throw new NotFoundException($"Assignment with ID {id} was not found.");
        }

        await _repository.SoftDeleteAsync(id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<LookupItemDto>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        return _repository.GetLookupAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StudentAssignmentDto>> GetStudentAssignmentsAsync(string studentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId)) return Array.Empty<StudentAssignmentDto>();

        var enrolledCourseIds = await _repository.GetStudentEnrolledCourseIdsAsync(studentId, cancellationToken);
        if (!enrolledCourseIds.Any())
        {
            return Array.Empty<StudentAssignmentDto>();
        }

        var assignments = await _repository.GetAssignmentsByCourseIdsAsync(enrolledCourseIds, cancellationToken);
        if (!assignments.Any())
        {
            return Array.Empty<StudentAssignmentDto>();
        }

        var assignmentIds = assignments.Select(a => a.Id).ToList();
        var submissions = await _repository.GetStudentSubmissionsForAssignmentsAsync(studentId, assignmentIds, cancellationToken);
        var submissionMap = submissions.ToDictionary(s => s.AssignmentId, s => s);

        var result = new List<StudentAssignmentDto>();
        foreach (var assignment in assignments)
        {
            submissionMap.TryGetValue(assignment.Id, out var submission);
            result.Add(new StudentAssignmentDto
            {
                Id = assignment.Id,
                Title = assignment.Title,
                Description = assignment.Description,
                DueDate = assignment.DueDate,
                Status = assignment.Status,
                CourseId = assignment.CourseId,
                CourseTitle = assignment.Course?.Title ?? string.Empty,
                SubmissionId = submission?.Id,
                SubmissionStatus = submission?.Status,
                FilePath = submission?.FilePath,
                Marks = submission?.Marks,
                Feedback = submission?.Feedback,
                SubmittedAt = submission?.CreatedAt,
                IsEnrolled = true
            });
        }

        return result;
    }

    public async Task<StudentAssignmentDto?> GetStudentAssignmentDetailsAsync(int assignmentId, string studentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId)) return null;

        var assignment = await _repository.GetByIdAsync(assignmentId, cancellationToken);
        if (assignment is null) return null;

        bool isEnrolled = await _repository.IsStudentEnrolledInCourseAsync(studentId, assignment.CourseId, cancellationToken);
        if (!isEnrolled) return null;

        var submission = await _repository.GetStudentSubmissionAsync(assignmentId, studentId, cancellationToken);

        return new StudentAssignmentDto
        {
            Id = assignment.Id,
            Title = assignment.Title,
            Description = assignment.Description,
            DueDate = assignment.DueDate,
            Status = assignment.Status,
            CourseId = assignment.CourseId,
            CourseTitle = assignment.Course?.Title ?? string.Empty,
            SubmissionId = submission?.Id,
            SubmissionStatus = submission?.Status,
            FilePath = submission?.FilePath,
            Marks = submission?.Marks,
            Feedback = submission?.Feedback,
            SubmittedAt = submission?.CreatedAt,
            IsEnrolled = true
        };
    }

    public async Task<bool> SubmitAssignmentAsync(int assignmentId, string studentId, string? filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId)) return false;

        var assignment = await _repository.GetByIdAsync(assignmentId, cancellationToken)
            ?? throw new NotFoundException($"Assignment with ID {assignmentId} was not found.");

        bool isEnrolled = await _repository.IsStudentEnrolledInCourseAsync(studentId, assignment.CourseId, cancellationToken);
        if (!isEnrolled)
        {
            throw new InvalidOperationException("You are not enrolled in the course for this assignment.");
        }

        var existingSubmission = await _repository.GetStudentSubmissionAsync(assignmentId, studentId, cancellationToken);

        if (existingSubmission is not null)
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                existingSubmission.FilePath = filePath;
            }
            existingSubmission.Status = Domain.Enums.SubmissionStatus.Submitted;
            existingSubmission.UpdatedAt = DateTime.UtcNow;

            _submissionRepository.Update(existingSubmission);
        }
        else
        {
            var newSubmission = new Submission
            {
                AssignmentId = assignmentId,
                StudentId = studentId,
                FilePath = filePath,
                Status = Domain.Enums.SubmissionStatus.Submitted,
                CreatedAt = DateTime.UtcNow
            };

            await _submissionRepository.AddAsync(newSubmission, cancellationToken);
        }

        await _submissionRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
