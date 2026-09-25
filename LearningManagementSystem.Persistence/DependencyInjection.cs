using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Persistence.Context;
using LearningManagementSystem.Persistence.Repositories;
using LearningManagementSystem.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LearningManagementSystem.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // SQLite lives in the current user's application data folder. This path does not
        // depend on the location of the ZIP, the IDE, or the current working directory.
        var provider = configuration["Database:Provider"] ?? "Sqlite";
        if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            var dataRoot = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrWhiteSpace(dataRoot))
                dataRoot = AppContext.BaseDirectory;

            var dataDirectory = Path.Combine(dataRoot, "AISkillsAcademy");
            Directory.CreateDirectory(dataDirectory);
            var connection = new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(dataDirectory, "academy.db"),
                ForeignKeys = true
            }.ToString();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
        }
        else if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            var connection = configuration.GetConnectionString("SqlServer");
            if (string.IsNullOrWhiteSpace(connection))
                throw new InvalidOperationException("Set ConnectionStrings:SqlServer before selecting the SqlServer database provider.");
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection));
        }
        else
        {
            throw new InvalidOperationException($"Unknown database provider '{provider}'. Choose Sqlite or SqlServer.");
        }

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<ILessonProgressRepository, LessonProgressRepository>();
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IDiscussionRepository, DiscussionRepository>();
        services.AddScoped<IAiToolRepository, AiToolRepository>();
        services.AddScoped<IBookmarkRepository, BookmarkRepository>();
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ICertificateRepository, CertificateRepository>();
        services.AddScoped<IContactMessageRepository, ContactMessageRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
