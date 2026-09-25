using LearningManagementSystem.Application.DTOs.Certificates;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Enums;

namespace LearningManagementSystem.Application.Services;

public class CertificateEligibilityService : ICertificateEligibilityService
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ILessonProgressRepository _lessonProgressRepository;
    private readonly IQuizRepository _quizRepository;
    private readonly IQuizAttemptRepository _quizAttemptRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ICertificateRepository _certificateRepository;
    private readonly ICertificateService _certificateService;

    public CertificateEligibilityService(
        ILessonRepository lessonRepository,
        ILessonProgressRepository lessonProgressRepository,
        IQuizRepository quizRepository,
        IQuizAttemptRepository quizAttemptRepository,
        IAssignmentRepository assignmentRepository,
        ICertificateRepository certificateRepository,
        ICertificateService certificateService)
    {
        _lessonRepository = lessonRepository;
        _lessonProgressRepository = lessonProgressRepository;
        _quizRepository = quizRepository;
        _quizAttemptRepository = quizAttemptRepository;
        _assignmentRepository = assignmentRepository;
        _certificateRepository = certificateRepository;
        _certificateService = certificateService;
    }

    public async Task<bool> CheckAndIssueAsync(string studentId, int courseId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(studentId))
        {
            return false;
        }

        if (await _certificateRepository.ExistsForStudentAndCourseAsync(studentId, courseId, cancellationToken))
        {
            return false;
        }

        // A course with no lessons yet has nothing a student can "finish" — don't auto-certify it.
        var totalLessons = await _lessonRepository.CountByCourseIdAsync(courseId, cancellationToken);
        if (totalLessons == 0)
        {
            return false;
        }

        var completedLessons = await _lessonProgressRepository.CountCompletedForStudentCourseAsync(studentId, courseId, cancellationToken);
        if (completedLessons < totalLessons)
        {
            return false;
        }

        var quizzes = await _quizRepository.GetQuizzesByCourseIdsAsync(new[] { courseId }, cancellationToken);
        foreach (var quiz in quizzes)
        {
            var hasPassed = await _quizAttemptRepository.HasPassingAttemptAsync(studentId, quiz.Id, cancellationToken);
            if (!hasPassed)
            {
                return false;
            }
        }

        var assignments = await _assignmentRepository.GetAssignmentsByCourseIdsAsync(new[] { courseId }, cancellationToken);
        foreach (var assignment in assignments)
        {
            var submission = await _assignmentRepository.GetStudentSubmissionAsync(assignment.Id, studentId, cancellationToken);
            if (submission is null || submission.Status != SubmissionStatus.Graded)
            {
                return false;
            }
        }

        await _certificateService.CreateAsync(new CreateCertificateDto
        {
            StudentId = studentId,
            CourseId = courseId,
            IssuedDate = DateTime.UtcNow
        }, cancellationToken);

        return true;
    }
}
