using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Application.DTOs.Enrollments;

public class EnrollmentDto
{
    public int Id { get; set; }

    public string StudentId { get; set; } = string.Empty;

    public string StudentName { get; set; } = string.Empty;

    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;

    public DateTime EnrollmentDate { get; set; }

    public decimal Progress { get; set; }
}

public class CreateEnrollmentDto
{
    [Required(ErrorMessage = "Student is required.")]
    [Display(Name = "Student")]
    public string StudentId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Course is required.")]
    [Display(Name = "Course")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Enrollment date is required.")]
    [Display(Name = "Enrollment Date")]
    [DataType(DataType.Date)]
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow.Date;

    [Required(ErrorMessage = "Progress is required.")]
    [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100.")]
    [Display(Name = "Progress (%)")]
    public decimal Progress { get; set; }
}

public class UpdateEnrollmentDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Student is required.")]
    [Display(Name = "Student")]
    public string StudentId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Course is required.")]
    [Display(Name = "Course")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Enrollment date is required.")]
    [Display(Name = "Enrollment Date")]
    [DataType(DataType.Date)]
    public DateTime EnrollmentDate { get; set; }

    [Required(ErrorMessage = "Progress is required.")]
    [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100.")]
    [Display(Name = "Progress (%)")]
    public decimal Progress { get; set; }
}
