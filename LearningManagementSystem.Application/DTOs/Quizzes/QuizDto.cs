using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Application.DTOs.Quizzes;

public class QuizDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public int TotalMarks { get; set; }

    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public int QuestionCount { get; set; }
}

public class CreateQuizDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Total marks is required.")]
    [Range(1, 1000, ErrorMessage = "Total marks must be between 1 and 1000.")]
    [Display(Name = "Total Marks")]
    public int TotalMarks { get; set; }

    [Required(ErrorMessage = "Course is required.")]
    [Display(Name = "Course")]
    public int CourseId { get; set; }
}

public class UpdateQuizDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Total marks is required.")]
    [Range(1, 1000, ErrorMessage = "Total marks must be between 1 and 1000.")]
    [Display(Name = "Total Marks")]
    public int TotalMarks { get; set; }

    [Required(ErrorMessage = "Course is required.")]
    [Display(Name = "Course")]
    public int CourseId { get; set; }
}
