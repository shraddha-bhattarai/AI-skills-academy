using LearningManagementSystem.Domain.Common;
using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Domain.Entities.Quizzes;

public class Quiz : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public int TotalMarks { get; set; }

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public ICollection<Question> Questions { get; set; } = new List<Question>();

    public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
}