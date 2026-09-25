using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Domain.Entities.Courses;
using LearningManagementSystem.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Persistence.Repositories;

public class LessonProgressRepository : ILessonProgressRepository
{
    private readonly ApplicationDbContext _context;

    public LessonProgressRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LessonProgress?> GetAsync(string studentId, int lessonId, CancellationToken cancellationToken = default)
    {
        return await _context.LessonProgresses
            .FirstOrDefaultAsync(p => p.StudentId == studentId && p.LessonId == lessonId, cancellationToken);
    }

    public async Task AddAsync(LessonProgress progress, CancellationToken cancellationToken = default)
    {
        await _context.LessonProgresses.AddAsync(progress, cancellationToken);
    }

    public async Task<int> CountCompletedForStudentCourseAsync(string studentId, int courseId, CancellationToken cancellationToken = default)
    {
        return await _context.LessonProgresses
            .CountAsync(p => p.StudentId == studentId && p.Lesson.CourseId == courseId, cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetCompletedLessonIdsAsync(string studentId, int courseId, CancellationToken cancellationToken = default)
    {
        return await _context.LessonProgresses
            .Where(p => p.StudentId == studentId && p.Lesson.CourseId == courseId)
            .Select(p => p.LessonId)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountCompletedThisWeekAsync(string studentId, CancellationToken cancellationToken = default)
    {
        var weekAgo = DateTime.UtcNow.AddDays(-7);
        return await _context.LessonProgresses
            .CountAsync(p => p.StudentId == studentId && p.CompletedAt >= weekAgo, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
