using LearningManagementSystem.Application.DTOs.Categories;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;
    private readonly ICourseService _courseService;
    private const int PageSize = 10;

    public CategoriesController(ICategoryService categoryService, ICourseService courseService)
    {
        _categoryService = categoryService;
        _courseService = courseService;
    }

    public async Task<IActionResult> Index(string? search, string? sortBy, bool sortDescending = false, int page = 1)
    {
        var result = await _categoryService.GetAllAsync(search, sortBy, sortDescending, page, PageSize);
        ViewBag.Search = search;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDescending = sortDescending;
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _categoryService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        // Fetch related courses for this category
        var courses = await _courseService.GetAllAsync(null, null, false, 1, 10, categoryId: id);
        ViewBag.RelatedCourses = courses.Items;

        return View(item);
    }

    public IActionResult Create()
    {
        return View(new CreateCategoryDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            await _categoryService.CreateAsync(dto);
            TempData["Success"] = "Category created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _categoryService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        return View(new UpdateCategoryDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateCategoryDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            await _categoryService.UpdateAsync(dto);
            TempData["Success"] = "Category updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var item = await _categoryService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        // Retrieve related courses to preview them on the delete confirmation page
        var courses = await _courseService.GetAllAsync(null, null, false, 1, 5, categoryId: id);
        ViewBag.RelatedCourses = courses.Items;

        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _categoryService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        if (item.CourseCount > 0)
        {
            TempData["Error"] = $"Cannot delete category '{item.Name}' because it currently has {item.CourseCount} active course(s) assigned to it. Please reassign or delete those courses first.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _categoryService.DeleteAsync(id);
            TempData["Success"] = "Category deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
