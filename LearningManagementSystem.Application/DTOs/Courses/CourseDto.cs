using System.ComponentModel.DataAnnotations;
using LearningManagementSystem.Domain.Enums;

namespace LearningManagementSystem.Application.DTOs.Courses;

public class CourseDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? LearningOutcomes { get; set; }

    public decimal Price { get; set; }

    public CourseLevel Level { get; set; }

    public CourseStatus Status { get; set; }

    public string? ThumbnailUrl { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public int LessonCount { get; set; }

    public int EnrollmentCount { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreateCourseDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Learning outcomes cannot exceed 2000 characters.")]
    [Display(Name = "Learning Outcomes")]
    public string? LearningOutcomes { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0, 999999.99, ErrorMessage = "Price must be a positive value.")]
    [Display(Name = "Price")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Level is required.")]
    [Display(Name = "Level")]
    public CourseLevel Level { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [Display(Name = "Status")]
    public CourseStatus Status { get; set; }

    [Url(ErrorMessage = "Please enter a valid URL.")]
    [Display(Name = "Thumbnail URL")]
    public string? ThumbnailUrl { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }
}

public class UpdateCourseDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Learning outcomes cannot exceed 2000 characters.")]
    [Display(Name = "Learning Outcomes")]
    public string? LearningOutcomes { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0, 999999.99, ErrorMessage = "Price must be a positive value.")]
    [Display(Name = "Price")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Level is required.")]
    [Display(Name = "Level")]
    public CourseLevel Level { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [Display(Name = "Status")]
    public CourseStatus Status { get; set; }

    [Url(ErrorMessage = "Please enter a valid URL.")]
    [Display(Name = "Thumbnail URL")]
    public string? ThumbnailUrl { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }
}
