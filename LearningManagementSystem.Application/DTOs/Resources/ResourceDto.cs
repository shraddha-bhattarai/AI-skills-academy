using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Application.DTOs.Resources;

public class ResourceDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string FileUrl { get; set; } = string.Empty;

    public int LessonId { get; set; }

    public string LessonTitle { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public class CreateResourceDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "File URL is required.")]
    [Url(ErrorMessage = "Please enter a valid URL.")]
    [Display(Name = "File URL")]
    public string FileUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lesson is required.")]
    public int LessonId { get; set; }
}

public class UpdateResourceDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "File URL is required.")]
    [Url(ErrorMessage = "Please enter a valid URL.")]
    [Display(Name = "File URL")]
    public string FileUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lesson is required.")]
    public int LessonId { get; set; }
}

public class StudentResourceDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string FileUrl { get; set; } = string.Empty;
}
