using LearningManagementSystem.Application.Common;
using LearningManagementSystem.Application.DTOs.Lessons;

namespace LearningManagementSystem.Application.Interfaces.Services;

public interface ILessonService
{
    Task<PagedResult<LessonDto>> GetAllAsync(string? search, string? sortBy, bool sortDescending, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<LessonDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task CreateAsync(CreateLessonDto dto, CancellationToken cancellationToken = default);

    Task UpdateAsync(UpdateLessonDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StudentLessonDto>?> GetStudentLessonsAsync(string studentId, int courseId, CancellationToken cancellationToken = default);

    Task<StudentLessonDto?> GetStudentLessonDetailsAsync(int lessonId, string studentId, CancellationToken cancellationToken = default);
}
