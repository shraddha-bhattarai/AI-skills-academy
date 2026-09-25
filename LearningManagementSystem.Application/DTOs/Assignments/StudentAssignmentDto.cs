using System;
using LearningManagementSystem.Domain.Enums;

namespace LearningManagementSystem.Application.DTOs.Assignments;

public class StudentAssignmentDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime DueDate { get; set; }

    public AssignmentStatus Status { get; set; }

    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;

    public int? SubmissionId { get; set; }

    public SubmissionStatus? SubmissionStatus { get; set; }

    public string? FilePath { get; set; }

    public decimal? Marks { get; set; }

    public string? Feedback { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public bool IsEnrolled { get; set; } = true;
}
