using System.ComponentModel.DataAnnotations;
using LearningManagementSystem.Application.DTOs.Resources;

namespace LearningManagementSystem.Application.DTOs.Lessons;

public class LessonDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? VideoUrl { get; set; }

    public string? NotesUrl { get; set; }

    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public class CreateLessonDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required.")]
    [Display(Name = "Content")]
    public string Content { get; set; } = string.Empty;

    [Url(ErrorMessage = "Please enter a valid URL.")]
    [Display(Name = "Video URL")]
    public string? VideoUrl { get; set; }

    [Url(ErrorMessage = "Please enter a valid URL.")]
    [Display(Name = "Notes URL")]
    public string? NotesUrl { get; set; }

    [Required(ErrorMessage = "Course is required.")]
    [Display(Name = "Course")]
    public int CourseId { get; set; }
}

public class StudentLessonDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? VideoUrl { get; set; }

    public string? NotesUrl { get; set; }

    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }

    public IReadOnlyList<StudentResourceDto> Resources { get; set; } = Array.Empty<StudentResourceDto>();
}

public class UpdateLessonDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required.")]
    [Display(Name = "Content")]
    public string Content { get; set; } = string.Empty;

    [Url(ErrorMessage = "Please enter a valid URL.")]
    [Display(Name = "Video URL")]
    public string? VideoUrl { get; set; }

    [Url(ErrorMessage = "Please enter a valid URL.")]
    [Display(Name = "Notes URL")]
    public string? NotesUrl { get; set; }

    [Required(ErrorMessage = "Course is required.")]
    [Display(Name = "Course")]
    public int CourseId { get; set; }
}
