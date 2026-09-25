using System;
using System.Collections.Generic;

namespace LearningManagementSystem.Web.Areas.Student.ViewModels
{
    public class StudentCoursesViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public string SearchTerm { get; set; } = string.Empty;
        public string ActiveFilter { get; set; } = "All";

        public CourseStatsViewModel Stats { get; set; } = new();

        public List<StudentCourseItemViewModel> Courses { get; set; } = new();
    }

    public class CourseStatsViewModel
    {
        public int TotalEnrolled { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedCount { get; set; }
        public int NotStartedCount { get; set; }
        public double AverageProgress { get; set; }
    }

    public class StudentCourseItemViewModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int ProgressPercentage { get; set; }
        public int CompletedLessons { get; set; }
        public int TotalLessons { get; set; }
        public DateTime EnrollmentDate { get; set; }

        public bool IsCompleted => ProgressPercentage >= 100;
        public bool IsNotStarted => ProgressPercentage == 0;
        public bool IsInProgress => ProgressPercentage > 0 && ProgressPercentage < 100;

        public string StatusText => IsCompleted ? "Completed" : (IsNotStarted ? "Not Started" : "In Progress");
        public string StatusBadgeClass => IsCompleted ? "bg-success" : (IsNotStarted ? "bg-secondary" : "bg-primary");
    }
}
