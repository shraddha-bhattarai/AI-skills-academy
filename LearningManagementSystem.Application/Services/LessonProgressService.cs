using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Entities.Courses;

namespace LearningManagementSystem.Application.Services;

public class LessonProgressService : ILessonProgressService
{
    private readonly ILessonProgressRepository _progressRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ICertificateEligibilityService _certificateEligibilityService;

    public LessonProgressService(
        ILessonProgressRepository progressRepository,
        ILessonRepository lessonRepository,
        IEnrollmentRepository enrollmentRepository,
        ICertificateEligibilityService certificateEligibilityService)
    {
        _progressRepository = progressRepository;
        _lessonRepository = lessonRepository;
        _enrollmentRepository = enrollmentRepository;
        _certificateEligibilityService = certificateEligibilityService;
    }

    public async Task<(bool Success, string Message)> MarkLessonCompleteAsync(int lessonId, string studentId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
        {
            return (false, "You must be logged in to complete a lesson.");
        }

        var lesson = await _lessonRepository.GetByIdAsync(lessonId, cancellationToken);
        if (lesson is null)
        {
            return (false, "Lesson not found.");
        }

        bool isEnrolled = await _lessonRepository.IsStudentEnrolledInCourseAsync(studentId, lesson.CourseId, cancellationToken);
        if (!isEnrolled)
        {
            return (false, "You are not enrolled in the course for this lesson.");
        }

        var existingProgress = await _progressRepository.GetAsync(studentId, lessonId, cancellationToken);
        if (existingProgress is null)
        {
            await _progressRepository.AddAsync(new LessonProgress
            {
                StudentId = studentId,
                LessonId = lessonId,
                CompletedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);
            await _progressRepository.SaveChangesAsync(cancellationToken);
        }

        await RecalculateEnrollmentProgressAsync(studentId, lesson.CourseId, cancellationToken);
        await _certificateEligibilityService.CheckAndIssueAsync(studentId, lesson.CourseId, cancellationToken);

        return (true, "Lesson marked as complete!");
    }

    private async Task RecalculateEnrollmentProgressAsync(string studentId, int courseId, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentRepository.GetByStudentAndCourseAsync(studentId, courseId, cancellationToken);
        if (enrollment is null)
        {
            return;
        }

        var totalLessons = await _lessonRepository.CountByCourseIdAsync(courseId, cancellationToken);
        var completedLessons = await _progressRepository.CountCompletedForStudentCourseAsync(studentId, courseId, cancellationToken);

        enrollment.Progress = totalLessons == 0
            ? 0
            : Math.Round((decimal)completedLessons / totalLessons * 100, 2);
        enrollment.UpdatedAt = DateTime.UtcNow;

        _enrollmentRepository.Update(enrollment);
        await _enrollmentRepository.SaveChangesAsync(cancellationToken);
    }
}
