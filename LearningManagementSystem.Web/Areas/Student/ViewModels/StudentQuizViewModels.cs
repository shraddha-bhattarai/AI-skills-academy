using System;
using System.Collections.Generic;

namespace LearningManagementSystem.Web.Areas.Student.ViewModels
{
    public class StudentQuizItemViewModel
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public int TotalMarks { get; set; }
        public int QuestionCount { get; set; }
        public int EstimatedDurationMinutes { get; set; } = 15;
    }

    public class StudentQuizDetailsViewModel
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public int TotalMarks { get; set; }
        public int QuestionCount { get; set; }
        public int EstimatedDurationMinutes { get; set; } = 15;
        public int PassingMarks { get; set; }
    }

    public class TakeQuizViewModel
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public int TotalMarks { get; set; }
        public int DurationMinutes { get; set; } = 15;
        public List<QuizQuestionItemViewModel> Questions { get; set; } = new();
    }

    public class QuizQuestionItemViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
    }

    public class QuizResultViewModel
    {
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public int Score { get; set; }
        public int TotalMarks { get; set; }
        public double Percentage { get; set; }
        public bool IsPassed { get; set; }
        public int CorrectCount { get; set; }
        public int IncorrectCount { get; set; }
        public int UnansweredCount { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    public class QuizHistoryItemViewModel
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public int Score { get; set; }
        public int TotalMarks { get; set; }
        public double Percentage { get; set; }
        public bool IsPassed { get; set; }
        public DateTime AttemptedAt { get; set; }
    }
}
