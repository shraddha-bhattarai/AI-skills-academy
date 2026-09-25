using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Enums;
using LearningManagementSystem.Persistence.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Controllers;

// Public course catalog only. Admin CRUD lives in Areas/Admin/Controllers/CoursesController.
public class CoursesController : Controller
{
    private readonly ICourseService _courseService;
    private readonly ICategoryService _categoryService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IBookmarkService _bookmarkService;
    private readonly UserManager<ApplicationUser> _userManager;
    private const int PageSize = 9;

    public CoursesController(ICourseService courseService, ICategoryService categoryService, IEnrollmentService enrollmentService, IBookmarkService bookmarkService, UserManager<ApplicationUser> userManager)
    {
        _courseService = courseService;
        _categoryService = categoryService;
        _enrollmentService = enrollmentService;
        _bookmarkService = bookmarkService;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? search, int? categoryId, int? level, string? sortBy, bool sortDescending = false, int page = 1)
    {
        // Public catalog: guests/students must only ever see Published courses, regardless of role.
        var result = await _courseService.GetAllAsync(search, sortBy, sortDescending, page, PageSize, categoryId, level, publishedOnly: true);
        var categories = await _categoryService.GetLookupAsync();

        ViewBag.CategoryList = categories;
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.Level = level;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDescending = sortDescending;
        return View(result);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
    {
        var item = await _courseService.GetByIdAsync(id, cancellationToken);
        if (item is null || item.Status != CourseStatus.Published)
        {
            return NotFound();
        }

        ViewBag.IsStudent = false;
        ViewBag.IsEnrolled = false;
        ViewBag.IsBookmarked = false;

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Student"))
                {
                    ViewBag.IsStudent = true;
                    ViewBag.IsEnrolled = await _enrollmentService.IsStudentEnrolledAsync(user.Id, id, cancellationToken);
                    ViewBag.IsBookmarked = await _bookmarkService.IsBookmarkedAsync(user.Id, id, cancellationToken);
                }
            }
        }

        return View(item);
    }
}
