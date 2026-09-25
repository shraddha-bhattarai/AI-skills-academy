using LearningManagementSystem.Domain.Common;

namespace LearningManagementSystem.Domain.Entities.Quizzes;

public class QuizAttempt : BaseEntity
{
    public string StudentId { get; set; } = string.Empty;

    public int QuizId { get; set; }

    public Quiz Quiz { get; set; } = null!;

    public int Score { get; set; }

    public int TotalMarks { get; set; }

    public double Percentage { get; set; }

    public bool IsPassed { get; set; }

    public int CorrectCount { get; set; }

    public int IncorrectCount { get; set; }

    public int UnansweredCount { get; set; }

    public int TotalQuestions { get; set; }

    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    public string? AnswersJson { get; set; }
}
