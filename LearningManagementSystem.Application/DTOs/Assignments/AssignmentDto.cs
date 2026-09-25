using System.ComponentModel.DataAnnotations;
using LearningManagementSystem.Domain.Enums;

namespace LearningManagementSystem.Application.DTOs.Assignments;

public class AssignmentDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }

    public AssignmentStatus Status { get; set; }

    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public class CreateAssignmentDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Due date is required.")]
    [Display(Name = "Due Date")]
    [DataType(DataType.DateTime)]
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);

    [Required(ErrorMessage = "Status is required.")]
    [Display(Name = "Status")]
    public AssignmentStatus Status { get; set; }

    [Required(ErrorMessage = "Course is required.")]
    [Display(Name = "Course")]
    public int CourseId { get; set; }
}

public class UpdateAssignmentDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Due date is required.")]
    [Display(Name = "Due Date")]
    [DataType(DataType.DateTime)]
    public DateTime DueDate { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [Display(Name = "Status")]
    public AssignmentStatus Status { get; set; }

    [Required(ErrorMessage = "Course is required.")]
    [Display(Name = "Course")]
    public int CourseId { get; set; }
}
