namespace LearningManagementSystem.Application.DTOs.Bookmarks;

public class BookmarkedCourseDto
{
    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;

    public string CourseDescription { get; set; } = string.Empty;

    public string? ThumbnailUrl { get; set; }

    public DateTime BookmarkedAt { get; set; }
}
