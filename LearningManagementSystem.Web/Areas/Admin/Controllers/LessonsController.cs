using LearningManagementSystem.Application.DTOs.Lessons;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class LessonsController : Controller
{
    private readonly ILessonService _lessonService;
    private readonly ICourseService _courseService;
    private const int PageSize = 10;

    public LessonsController(ILessonService lessonService, ICourseService courseService)
    {
        _lessonService = lessonService;
        _courseService = courseService;
    }

    public async Task<IActionResult> Index(string? search, string? sortBy, bool sortDescending = false, int page = 1)
    {
        var result = await _lessonService.GetAllAsync(search, sortBy, sortDescending, page, PageSize);
        ViewBag.Search = search;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDescending = sortDescending;
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _lessonService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateCoursesAsync();
        return View(new CreateLessonDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateLessonDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCoursesAsync(dto.CourseId);
            return View(dto);
        }

        try
        {
            await _lessonService.CreateAsync(dto);
            TempData["Success"] = "Lesson created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateCoursesAsync(dto.CourseId);
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _lessonService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        await PopulateCoursesAsync(item.CourseId);
        return View(new UpdateLessonDto
        {
            Id = item.Id,
            Title = item.Title,
            Content = item.Content,
            VideoUrl = item.VideoUrl,
            NotesUrl = item.NotesUrl,
            CourseId = item.CourseId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateLessonDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateCoursesAsync(dto.CourseId);
            return View(dto);
        }

        try
        {
            await _lessonService.UpdateAsync(dto);
            TempData["Success"] = "Lesson updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateCoursesAsync(dto.CourseId);
            return View(dto);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var item = await _lessonService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _lessonService.DeleteAsync(id);
            TempData["Success"] = "Lesson deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    private async Task PopulateCoursesAsync(int? selectedId = null)
    {
        var courses = await _courseService.GetLookupAsync();
        ViewBag.CourseId = courses.ToSelectList(selectedId);
    }
}
