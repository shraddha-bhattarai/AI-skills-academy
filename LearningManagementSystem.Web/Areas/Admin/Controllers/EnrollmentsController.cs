using LearningManagementSystem.Application.DTOs.Enrollments;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class EnrollmentsController : Controller
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly ICourseService _courseService;
    private readonly IUserRepository _userRepository;
    private const int PageSize = 10;

    public EnrollmentsController(
        IEnrollmentService enrollmentService,
        ICourseService courseService,
        IUserRepository userRepository)
    {
        _enrollmentService = enrollmentService;
        _courseService = courseService;
        _userRepository = userRepository;
    }

    public async Task<IActionResult> Index(string? search, string? sortBy, bool sortDescending = false, int page = 1)
    {
        var result = await _enrollmentService.GetAllAsync(search, sortBy, sortDescending, page, PageSize);
        ViewBag.Search = search;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDescending = sortDescending;
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _enrollmentService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        var course = await _courseService.GetByIdAsync(item.CourseId);
        ViewBag.Course = course;

        return View(item);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        return View(new CreateEnrollmentDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEnrollmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(dto.StudentId, dto.CourseId);
            return View(dto);
        }

        try
        {
            await _enrollmentService.CreateAsync(dto);
            TempData["Success"] = "Enrollment created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateLookupsAsync(dto.StudentId, dto.CourseId);
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _enrollmentService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        await PopulateLookupsAsync(item.StudentId, item.CourseId);
        return View(new UpdateEnrollmentDto
        {
            Id = item.Id,
            StudentId = item.StudentId,
            CourseId = item.CourseId,
            EnrollmentDate = item.EnrollmentDate,
            Progress = item.Progress
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateEnrollmentDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(dto.StudentId, dto.CourseId);
            return View(dto);
        }

        try
        {
            await _enrollmentService.UpdateAsync(dto);
            TempData["Success"] = "Enrollment updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateLookupsAsync(dto.StudentId, dto.CourseId);
            return View(dto);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var item = await _enrollmentService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        var course = await _courseService.GetByIdAsync(item.CourseId);
        ViewBag.Course = course;

        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _enrollmentService.DeleteAsync(id);
            TempData["Success"] = "Enrollment deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    private async Task PopulateLookupsAsync(string? selectedStudentId = null, int? selectedCourseId = null)
    {
        var users = await _userRepository.GetAllUsersAsync();
        var courses = await _courseService.GetLookupAsync();
        ViewBag.StudentId = users.ToSelectList(selectedStudentId);
        ViewBag.CourseId = courses.ToSelectList(selectedCourseId);
    }
}
