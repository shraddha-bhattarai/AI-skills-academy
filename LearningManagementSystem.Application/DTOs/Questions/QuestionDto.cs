using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Application.DTOs.Questions;

public class QuestionDto
{
    public int Id { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public string OptionA { get; set; } = string.Empty;

    public string OptionB { get; set; } = string.Empty;

    public string OptionC { get; set; } = string.Empty;

    public string OptionD { get; set; } = string.Empty;

    public string CorrectAnswer { get; set; } = string.Empty;

    public int QuizId { get; set; }

    public string QuizTitle { get; set; } = string.Empty;
}

public class CreateQuestionDto
{
    [Required(ErrorMessage = "Question text is required.")]
    [Display(Name = "Question")]
    public string QuestionText { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option A is required.")]
    [Display(Name = "Option A")]
    public string OptionA { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option B is required.")]
    [Display(Name = "Option B")]
    public string OptionB { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option C is required.")]
    [Display(Name = "Option C")]
    public string OptionC { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option D is required.")]
    [Display(Name = "Option D")]
    public string OptionD { get; set; } = string.Empty;

    [Required(ErrorMessage = "Correct answer is required.")]
    [Display(Name = "Correct Answer")]
    public string CorrectAnswer { get; set; } = string.Empty;

    [Required(ErrorMessage = "Quiz is required.")]
    [Display(Name = "Quiz")]
    public int QuizId { get; set; }
}

public class UpdateQuestionDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Question text is required.")]
    [Display(Name = "Question")]
    public string QuestionText { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option A is required.")]
    [Display(Name = "Option A")]
    public string OptionA { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option B is required.")]
    [Display(Name = "Option B")]
    public string OptionB { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option C is required.")]
    [Display(Name = "Option C")]
    public string OptionC { get; set; } = string.Empty;

    [Required(ErrorMessage = "Option D is required.")]
    [Display(Name = "Option D")]
    public string OptionD { get; set; } = string.Empty;

    [Required(ErrorMessage = "Correct answer is required.")]
    [Display(Name = "Correct Answer")]
    public string CorrectAnswer { get; set; } = string.Empty;

    [Required(ErrorMessage = "Quiz is required.")]
    [Display(Name = "Quiz")]
    public int QuizId { get; set; }
}
