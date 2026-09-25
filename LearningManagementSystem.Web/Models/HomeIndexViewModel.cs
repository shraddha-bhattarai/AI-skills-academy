using LearningManagementSystem.Application.DTOs.Categories;
using LearningManagementSystem.Application.DTOs.Courses;

namespace LearningManagementSystem.Web.Models;

public class HomeIndexViewModel
{
    public int TotalPublishedCourses { get; set; }

    public int TotalStudents { get; set; }

    public int TotalCertificatesIssued { get; set; }

    public IReadOnlyList<CourseDto> FeaturedCourses { get; set; } = Array.Empty<CourseDto>();

    public IReadOnlyList<CategoryDto> Categories { get; set; } = Array.Empty<CategoryDto>();
}
