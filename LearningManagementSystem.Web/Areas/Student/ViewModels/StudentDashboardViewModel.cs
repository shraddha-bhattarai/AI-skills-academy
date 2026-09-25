using System;
using System.Collections.Generic;

namespace LearningManagementSystem.Web.Areas.Student.ViewModels
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; } = "Student";
        public string GreetingTitle { get; set; } = "Welcome Back 👋";
        public string GreetingSubtitle { get; set; } = "Continue your learning journey and achieve your goals today.";

        public bool HasEnrollments { get; set; }

        public DashboardStatsViewModel Stats { get; set; } = new();

        public ContinueLearningViewModel? ContinueLearning { get; set; }

        public List<EnrolledCourseSummaryViewModel> EnrolledCourses { get; set; } = new();

        public List<UpcomingAssignmentViewModel> UpcomingAssignments { get; set; } = new();

        public List<AvailableQuizViewModel> AvailableQuizzes { get; set; } = new();

        public LearningProgressViewModel LearningProgress { get; set; } = new();

        public List<RecentActivityViewModel> RecentActivities { get; set; } = new();
    }

    public class DashboardStatsViewModel
    {
        public int EnrolledCourses { get; set; }
        public int CompletedCourses { get; set; }
        public int PendingAssignments { get; set; }
        public int CertificatesEarned { get; set; }
    }

    public class ContinueLearningViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; }
        public string? NextLessonTitle { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
    }

    public class EnrolledCourseSummaryViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
    }

    public class UpcomingAssignmentViewModel
    {
        public int AssignmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string PriorityClass { get; set; } = "bg-warning";
    }

    public class AvailableQuizViewModel
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
    }

    public class LearningProgressViewModel
    {
        public int OverallProgressPercentage { get; set; }
        public int LessonsCompletedThisWeek { get; set; }
        public int TotalLessonsCompleted { get; set; }
    }

    public class RecentActivityViewModel
    {
        public string ActivityType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime TimeStamp { get; set; }
        public string IconClass { get; set; } = string.Empty;
        public string BadgeBgClass { get; set; } = string.Empty;
    }
}
