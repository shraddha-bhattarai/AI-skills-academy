using AutoMapper;
using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Quizzes;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Entities.Quizzes;

namespace LearningManagementSystem.Application.Services;

public class QuizService : IQuizService
{
    private readonly IQuizRepository _repository;
    private readonly ICourseRepository _courseRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IMapper _mapper;

    public QuizService(
        IQuizRepository repository,
        ICourseRepository courseRepository,
        IAssignmentRepository assignmentRepository,
        IMapper mapper)
    {
        _repository = repository;
        _courseRepository = courseRepository;
        _assignmentRepository = assignmentRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<QuizDto>> GetAllAsync(
        string? search,
        int? courseId,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetPagedAsync(search, courseId, sortBy, sortDescending, page, pageSize, cancellationToken);

        return new PagedResult<QuizDto>
        {
            Items = _mapper.Map<IReadOnlyList<QuizDto>>(result.Items),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<QuizDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var quiz = await _repository.GetByIdAsync(id, cancellationToken);
        return quiz is null ? null : _mapper.Map<QuizDto>(quiz);
    }

    public async Task CreateAsync(CreateQuizDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _courseRepository.ExistsAsync(dto.CourseId, cancellationToken))
        {
            throw new InvalidOperationException("The selected course does not exist.");
        }

        var quiz = _mapper.Map<Quiz>(dto);
        quiz.CreatedAt = DateTime.UtcNow;

        await _repository.AddAsync(quiz, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UpdateQuizDto dto, CancellationToken cancellationToken = default)
    {
        var quiz = await _repository.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new NotFoundException($"Quiz with ID {dto.Id} was not found.");

        if (!await _courseRepository.ExistsAsync(dto.CourseId, cancellationToken))
        {
            throw new InvalidOperationException("The selected course does not exist.");
        }

        _mapper.Map(dto, quiz);
        quiz.UpdatedAt = DateTime.UtcNow;

        _repository.Update(quiz);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await _repository.ExistsAsync(id, cancellationToken))
        {
            throw new NotFoundException($"Quiz with ID {id} was not found.");
        }

        if (await _repository.HasQuestionsAsync(id, cancellationToken))
        {
            throw new InvalidOperationException("This quiz cannot be deleted while it still has questions. Remove its questions first.");
        }

        await _repository.SoftDeleteAsync(id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<LookupItemDto>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        return _repository.GetLookupAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<QuizDto>> GetStudentQuizzesAsync(string studentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId)) return Array.Empty<QuizDto>();

        var enrolledCourseIds = await _assignmentRepository.GetStudentEnrolledCourseIdsAsync(studentId, cancellationToken);
        if (!enrolledCourseIds.Any())
        {
            return Array.Empty<QuizDto>();
        }

        var quizzes = await _repository.GetQuizzesByCourseIdsAsync(enrolledCourseIds, cancellationToken);
        return _mapper.Map<IReadOnlyList<QuizDto>>(quizzes);
    }

    public async Task<QuizDto?> GetStudentQuizDetailsAsync(int quizId, string studentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId)) return null;

        bool isEnrolled = await _repository.IsStudentEnrolledInQuizCourseAsync(studentId, quizId, cancellationToken);
        if (!isEnrolled) return null;

        var quiz = await _repository.GetByIdAsync(quizId, cancellationToken);
        return quiz is null ? null : _mapper.Map<QuizDto>(quiz);
    }
}
