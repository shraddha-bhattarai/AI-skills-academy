using LearningManagementSystem.Application.DTOs.Bookmarks;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Application.Services;

public class BookmarkService : IBookmarkService
{
    private readonly IBookmarkRepository _repository;
    private readonly ICourseRepository _courseRepository;

    public BookmarkService(IBookmarkRepository repository, ICourseRepository courseRepository)
    {
        _repository = repository;
        _courseRepository = courseRepository;
    }

    public async Task<IReadOnlyList<BookmarkedCourseDto>> GetStudentBookmarksAsync(string studentId, CancellationToken cancellationToken = default)
    {
        var bookmarks = await _repository.GetByStudentIdAsync(studentId, cancellationToken);

        return bookmarks.Select(b => new BookmarkedCourseDto
        {
            CourseId = b.CourseId,
            CourseTitle = b.Course?.Title ?? string.Empty,
            CourseDescription = b.Course?.Description ?? string.Empty,
            ThumbnailUrl = b.Course?.ThumbnailUrl,
            BookmarkedAt = b.CreatedAt
        }).ToList();
    }

    public async Task<bool> IsBookmarkedAsync(string studentId, int courseId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
        {
            return false;
        }

        return await _repository.GetAsync(studentId, courseId, cancellationToken) is not null;
    }

    public async Task<(bool IsBookmarked, string Message)> ToggleAsync(string studentId, int courseId, CancellationToken cancellationToken = default)
    {
        if (!await _courseRepository.ExistsAsync(courseId, cancellationToken))
        {
            return (false, "Course not found.");
        }

        var existing = await _repository.GetAsync(studentId, courseId, cancellationToken);
        if (existing is not null)
        {
            await _repository.RemoveAsync(existing, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            return (false, "Removed from bookmarks.");
        }

        await _repository.AddAsync(new Bookmark
        {
            StudentId = studentId,
            CourseId = courseId,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (true, "Added to bookmarks.");
    }
}
