namespace LearningManagementSystem.Web.Areas.Admin.ViewModels;

public class EnrollmentReportItemViewModel
{
    public string StudentName { get; set; } = string.Empty;

    public string StudentEmail { get; set; } = string.Empty;

    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;

    public DateTime EnrollmentDate { get; set; }

    public decimal Progress { get; set; }

    public bool IsCompleted { get; set; }
}

public class CourseProgressReportItemViewModel
{
    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;

    public int TotalLessons { get; set; }

    public int EnrolledStudents { get; set; }

    public decimal AverageProgress { get; set; }

    public int CompletedCount { get; set; }
}

public class QuizResultReportItemViewModel
{
    public int QuizId { get; set; }

    public string QuizTitle { get; set; } = string.Empty;

    public string CourseTitle { get; set; } = string.Empty;

    public int TotalAttempts { get; set; }

    public int PassCount { get; set; }

    public double PassRatePercent { get; set; }

    public double AverageScorePercent { get; set; }
}
