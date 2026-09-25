using AutoMapper;
using LearningManagementSystem.Application.DTOs.Announcements;
using LearningManagementSystem.Application.DTOs.Assignments;
using LearningManagementSystem.Application.DTOs.Categories;
using LearningManagementSystem.Application.DTOs.Certificates;
using LearningManagementSystem.Application.DTOs.ContactMessages;
using LearningManagementSystem.Application.DTOs.Courses;
using LearningManagementSystem.Application.DTOs.Enrollments;
using LearningManagementSystem.Application.DTOs.Lessons;
using LearningManagementSystem.Application.DTOs.Notifications;
using LearningManagementSystem.Application.DTOs.Questions;
using LearningManagementSystem.Application.DTOs.Quizzes;
using LearningManagementSystem.Application.DTOs.Resources;
using LearningManagementSystem.Application.DTOs.Submissions;
using LearningManagementSystem.Domain.Entities.Assignments;
using LearningManagementSystem.Domain.Entities.Certificates;
using LearningManagementSystem.Domain.Entities.Communication;
using LearningManagementSystem.Domain.Entities.Courses;
using LearningManagementSystem.Domain.Entities.Quizzes;

namespace LearningManagementSystem.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.CourseCount, opt => opt.MapFrom(src => src.Courses.Count));

        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>();

        CreateMap<Course, CourseDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.LessonCount, opt => opt.MapFrom(src => src.Lessons.Count))
            .ForMember(dest => dest.EnrollmentCount, opt => opt.MapFrom(src => src.Enrollments.Count));

        CreateMap<CreateCourseDto, Course>();
        CreateMap<UpdateCourseDto, Course>();

        CreateMap<Lesson, LessonDto>()
            .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.Title));

        CreateMap<CreateLessonDto, Lesson>();
        CreateMap<UpdateLessonDto, Lesson>();

        CreateMap<Resource, ResourceDto>()
            .ForMember(dest => dest.LessonTitle, opt => opt.MapFrom(src => src.Lesson.Title));

        CreateMap<CreateResourceDto, Resource>();
        CreateMap<UpdateResourceDto, Resource>();

        CreateMap<Enrollment, EnrollmentDto>()
            .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.Title))
            .ForMember(dest => dest.StudentName, opt => opt.Ignore());

        CreateMap<CreateEnrollmentDto, Enrollment>();
        CreateMap<UpdateEnrollmentDto, Enrollment>();

        CreateMap<Assignment, AssignmentDto>()
            .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.Title));

        CreateMap<CreateAssignmentDto, Assignment>();
        CreateMap<UpdateAssignmentDto, Assignment>();

        CreateMap<Submission, SubmissionDto>()
            .ForMember(dest => dest.AssignmentTitle, opt => opt.MapFrom(src => src.Assignment.Title))
            .ForMember(dest => dest.StudentName, opt => opt.Ignore());

        CreateMap<CreateSubmissionDto, Submission>();
        CreateMap<UpdateSubmissionDto, Submission>();

        CreateMap<Quiz, QuizDto>()
            .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.Title))
            .ForMember(dest => dest.QuestionCount, opt => opt.MapFrom(src => src.Questions.Count));

        CreateMap<CreateQuizDto, Quiz>();
        CreateMap<UpdateQuizDto, Quiz>();

        CreateMap<Question, QuestionDto>()
            .ForMember(dest => dest.QuizTitle, opt => opt.MapFrom(src => src.Quiz.Title));

        CreateMap<CreateQuestionDto, Question>();
        CreateMap<UpdateQuestionDto, Question>();

        CreateMap<Announcement, AnnouncementDto>();
        CreateMap<CreateAnnouncementDto, Announcement>();
        CreateMap<UpdateAnnouncementDto, Announcement>();

        CreateMap<Notification, NotificationDto>()
            .ForMember(dest => dest.UserName, opt => opt.Ignore());

        CreateMap<CreateNotificationDto, Notification>();
        CreateMap<UpdateNotificationDto, Notification>();

        CreateMap<Certificate, CertificateDto>()
            .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.Title))
            .ForMember(dest => dest.StudentName, opt => opt.Ignore());

        CreateMap<CreateCertificateDto, Certificate>()
            .ForMember(dest => dest.CertificateNumber, opt => opt.MapFrom(src =>
                string.IsNullOrWhiteSpace(src.CertificateNumber)
                    ? Guid.NewGuid().ToString()
                    : src.CertificateNumber));

        CreateMap<UpdateCertificateDto, Certificate>();

        CreateMap<ContactMessage, ContactMessageDto>();
        CreateMap<CreateContactMessageDto, ContactMessage>();
        CreateMap<UpdateContactMessageDto, ContactMessage>();
    }
}
