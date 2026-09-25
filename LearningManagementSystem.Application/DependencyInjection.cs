using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Application.Mappings;
using LearningManagementSystem.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LearningManagementSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ILessonService, LessonService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<ILessonProgressService, LessonProgressService>();
        services.AddScoped<IResourceService, ResourceService>();
        services.AddScoped<IDiscussionService, DiscussionService>();
        services.AddScoped<IAiToolService, AiToolService>();
        services.AddScoped<IBookmarkService, BookmarkService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IAnnouncementService, AnnouncementService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ICertificateService, CertificateService>();
        services.AddScoped<ICertificateEligibilityService, CertificateEligibilityService>();
        services.AddScoped<IContactMessageService, ContactMessageService>();

        return services;
    }
}
