using LearningManagementSystem.Application.DTOs.Courses;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CoursesController : Controller
{
    private readonly ICourseService _courseService;
    private readonly ICategoryService _categoryService;
    private const int PageSize = 10;

    public CoursesController(ICourseService courseService, ICategoryService categoryService)
    {
        _courseService = courseService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index(
        string? search,
        int? categoryId,
        int? level,
        string? sortBy,
        bool sortDescending = false,
        int page = 1)
    {
        var result = await _courseService.GetAllAsync(search, sortBy, sortDescending, page, PageSize, categoryId, level);
        
        var categories = await _categoryService.GetLookupAsync();
        ViewBag.CategoryList = categories;
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.Level = level;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDescending = sortDescending;

        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _courseService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateCategoriesAsync();
        return View(new CreateCourseDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCourseDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(dto.CategoryId);
            return View(dto);
        }

        try
        {
            await _courseService.CreateAsync(dto);
            TempData["Success"] = "Course created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateCategoriesAsync(dto.CategoryId);
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _courseService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        await PopulateCategoriesAsync(item.CategoryId);
        return View(new UpdateCourseDto
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            LearningOutcomes = item.LearningOutcomes,
            Price = item.Price,
            Level = item.Level,
            Status = item.Status,
            ThumbnailUrl = item.ThumbnailUrl,
            CategoryId = item.CategoryId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateCourseDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(dto.CategoryId);
            return View(dto);
        }

        try
        {
            await _courseService.UpdateAsync(dto);
            TempData["Success"] = "Course updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateCategoriesAsync(dto.CategoryId);
            return View(dto);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var item = await _courseService.GetByIdAsync(id);
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
            await _courseService.DeleteAsync(id);
            TempData["Success"] = "Course deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    private async Task PopulateCategoriesAsync(int? selectedId = null)
    {
        var categories = await _categoryService.GetLookupAsync();
        ViewBag.CategoryId = categories.ToSelectList(selectedId);
    }
}
