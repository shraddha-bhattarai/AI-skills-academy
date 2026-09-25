using AutoMapper;
using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Lessons;
using LearningManagementSystem.Application.DTOs.Resources;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Application.Services;

public class LessonService : ILessonService
{
    private readonly ILessonRepository _repository;
    private readonly ICourseRepository _courseRepository;
    private readonly ILessonProgressRepository _lessonProgressRepository;
    private readonly IResourceRepository _resourceRepository;
    private readonly IMapper _mapper;

    public LessonService(
        ILessonRepository repository,
        ICourseRepository courseRepository,
        ILessonProgressRepository lessonProgressRepository,
        IResourceRepository resourceRepository,
        IMapper mapper)
    {
        _repository = repository;
        _courseRepository = courseRepository;
        _lessonProgressRepository = lessonProgressRepository;
        _resourceRepository = resourceRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<LessonDto>> GetAllAsync(
        string? search,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.GetPagedAsync(search, sortBy, sortDescending, page, pageSize, cancellationToken);

        return new PagedResult<LessonDto>
        {
            Items = _mapper.Map<IReadOnlyList<LessonDto>>(result.Items),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<LessonDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var lesson = await _repository.GetByIdAsync(id, cancellationToken);
        return lesson is null ? null : _mapper.Map<LessonDto>(lesson);
    }

    public async Task CreateAsync(CreateLessonDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _courseRepository.ExistsAsync(dto.CourseId, cancellationToken))
        {
            throw new InvalidOperationException("The selected course does not exist.");
        }

        var lesson = _mapper.Map<Lesson>(dto);
        lesson.CreatedAt = DateTime.UtcNow;

        await _repository.AddAsync(lesson, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UpdateLessonDto dto, CancellationToken cancellationToken = default)
    {
        var lesson = await _repository.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new NotFoundException($"Lesson with ID {dto.Id} was not found.");

        if (!await _courseRepository.ExistsAsync(dto.CourseId, cancellationToken))
        {
            throw new InvalidOperationException("The selected course does not exist.");
        }

        _mapper.Map(dto, lesson);
        lesson.UpdatedAt = DateTime.UtcNow;

        _repository.Update(lesson);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (!await _repository.ExistsAsync(id, cancellationToken))
        {
            throw new NotFoundException($"Lesson with ID {id} was not found.");
        }

        await _repository.SoftDeleteAsync(id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StudentLessonDto>?> GetStudentLessonsAsync(string studentId, int courseId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId)) return null;

        bool isEnrolled = await _repository.IsStudentEnrolledInCourseAsync(studentId, courseId, cancellationToken);
        if (!isEnrolled) return null;

        var lessons = await _repository.GetByCourseIdAsync(courseId, cancellationToken);
        var completedLessonIds = (await _lessonProgressRepository.GetCompletedLessonIdsAsync(studentId, courseId, cancellationToken)).ToHashSet();

        return lessons.Select(l => MapToStudentDto(l, completedLessonIds.Contains(l.Id))).ToList();
    }

    public async Task<StudentLessonDto?> GetStudentLessonDetailsAsync(int lessonId, string studentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId)) return null;

        var lesson = await _repository.GetByIdAsync(lessonId, cancellationToken);
        if (lesson is null) return null;

        bool isEnrolled = await _repository.IsStudentEnrolledInCourseAsync(studentId, lesson.CourseId, cancellationToken);
        if (!isEnrolled) return null;

        var progress = await _lessonProgressRepository.GetAsync(studentId, lessonId, cancellationToken);
        var resources = await _resourceRepository.GetByLessonIdAsync(lessonId, cancellationToken);

        var dto = MapToStudentDto(lesson, progress is not null);
        dto.CompletedAt = progress?.CompletedAt;
        dto.Resources = resources.Select(r => new StudentResourceDto
        {
            Id = r.Id,
            Title = r.Title,
            Description = r.Description,
            FileUrl = r.FileUrl
        }).ToList();
        return dto;
    }

    private static StudentLessonDto MapToStudentDto(Lesson lesson, bool isCompleted)
    {
        return new StudentLessonDto
        {
            Id = lesson.Id,
            Title = lesson.Title,
            Content = lesson.Content,
            VideoUrl = UrlSafety.SanitizeOrNull(lesson.VideoUrl),
            NotesUrl = UrlSafety.SanitizeOrNull(lesson.NotesUrl),
            CourseId = lesson.CourseId,
            CourseTitle = lesson.Course?.Title ?? string.Empty,
            IsCompleted = isCompleted
        };
    }
}
