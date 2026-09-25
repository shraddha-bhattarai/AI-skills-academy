using LearningManagementSystem.Application.DTOs.Bookmarks;

namespace LearningManagementSystem.Application.Interfaces.Services;

public interface IBookmarkService
{
    Task<IReadOnlyList<BookmarkedCourseDto>> GetStudentBookmarksAsync(string studentId, CancellationToken cancellationToken = default);

    Task<bool> IsBookmarkedAsync(string studentId, int courseId, CancellationToken cancellationToken = default);

    Task<(bool IsBookmarked, string Message)> ToggleAsync(string studentId, int courseId, CancellationToken cancellationToken = default);
}
