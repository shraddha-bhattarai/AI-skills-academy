using System.ComponentModel.DataAnnotations;
using LearningManagementSystem.Domain.Enums;

namespace LearningManagementSystem.Application.DTOs.Submissions;

public class SubmissionDto
{
    public int Id { get; set; }

    public string StudentId { get; set; } = string.Empty;

    public string StudentName { get; set; } = string.Empty;

    public string? FilePath { get; set; }

    public decimal? Marks { get; set; }

    public string? Feedback { get; set; }

    public SubmissionStatus Status { get; set; }

    public int AssignmentId { get; set; }

    public string AssignmentTitle { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public class GradeSubmissionDto
{
    public int Id { get; set; }

    [Range(0, 1000, ErrorMessage = "Marks must be between 0 and 1000.")]
    [Display(Name = "Marks")]
    public decimal? Marks { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [Display(Name = "Status")]
    public SubmissionStatus Status { get; set; }

    [StringLength(2000, ErrorMessage = "Feedback cannot exceed 2000 characters.")]
    [Display(Name = "Feedback")]
    public string? Feedback { get; set; }
}

public class CreateSubmissionDto
{
    [Required(ErrorMessage = "Student is required.")]
    [Display(Name = "Student")]
    public string StudentId { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "File path cannot exceed 500 characters.")]
    [Display(Name = "File Path")]
    public string? FilePath { get; set; }

    [Range(0, 1000, ErrorMessage = "Marks must be between 0 and 1000.")]
    [Display(Name = "Marks")]
    public decimal? Marks { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [Display(Name = "Status")]
    public SubmissionStatus Status { get; set; }

    [Required(ErrorMessage = "Assignment is required.")]
    [Display(Name = "Assignment")]
    public int AssignmentId { get; set; }
}

public class UpdateSubmissionDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Student is required.")]
    [Display(Name = "Student")]
    public string StudentId { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "File path cannot exceed 500 characters.")]
    [Display(Name = "File Path")]
    public string? FilePath { get; set; }

    [Range(0, 1000, ErrorMessage = "Marks must be between 0 and 1000.")]
    [Display(Name = "Marks")]
    public decimal? Marks { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [Display(Name = "Status")]
    public SubmissionStatus Status { get; set; }

    [Required(ErrorMessage = "Assignment is required.")]
    [Display(Name = "Assignment")]
    public int AssignmentId { get; set; }
}
