using LearningManagementSystem.Domain.Common;

namespace LearningManagementSystem.Domain.Entities.Quizzes;

public class Question : BaseEntity
{
    public string QuestionText { get; set; } = string.Empty;

    public string OptionA { get; set; } = string.Empty;

    public string OptionB { get; set; } = string.Empty;

    public string OptionC { get; set; } = string.Empty;

    public string OptionD { get; set; } = string.Empty;

    public string CorrectAnswer { get; set; } = string.Empty;

    public int QuizId { get; set; }

    public Quiz Quiz { get; set; } = null!;
}