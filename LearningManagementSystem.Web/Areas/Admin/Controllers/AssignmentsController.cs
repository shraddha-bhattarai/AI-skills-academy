using LearningManagementSystem.Application.DTOs.Assignments;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Enums;
using LearningManagementSystem.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LearningManagementSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AssignmentsController : Controller
{
    private readonly IAssignmentService _assignmentService;
    private readonly ICourseService _courseService;
    private const int PageSize = 10;

    public AssignmentsController(IAssignmentService assignmentService, ICourseService courseService)
    {
        _assignmentService = assignmentService;
        _courseService = courseService;
    }

    public async Task<IActionResult> Index(string? search, string? sortBy, bool sortDescending = false, int page = 1)
    {
        var result = await _assignmentService.GetAllAsync(search, sortBy, sortDescending, page, PageSize);
        ViewBag.Search = search;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDescending = sortDescending;
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _assignmentService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        return View(new CreateAssignmentDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAssignmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(dto.CourseId);
            return View(dto);
        }

        try
        {
            await _assignmentService.CreateAsync(dto);
            TempData["Success"] = "Assignment created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateLookupsAsync(dto.CourseId);
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _assignmentService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        await PopulateLookupsAsync(item.CourseId, item.Status);
        return View(new UpdateAssignmentDto
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            DueDate = item.DueDate,
            Status = item.Status,
            CourseId = item.CourseId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateAssignmentDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(dto.CourseId, dto.Status);
            return View(dto);
        }

        try
        {
            await _assignmentService.UpdateAsync(dto);
            TempData["Success"] = "Assignment updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateLookupsAsync(dto.CourseId, dto.Status);
            return View(dto);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var item = await _assignmentService.GetByIdAsync(id);
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
            await _assignmentService.DeleteAsync(id);
            TempData["Success"] = "Assignment deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    private async Task PopulateLookupsAsync(int? selectedCourseId = null, AssignmentStatus? selectedStatus = null)
    {
        var courses = await _courseService.GetLookupAsync();
        ViewBag.CourseId = courses.ToSelectList(selectedCourseId);
        ViewBag.Status = new SelectList(
            Enum.GetValues<AssignmentStatus>().Select(s => new { Value = (int)s, Text = s.ToString() }),
            "Value", "Text",
            (int?)selectedStatus);
    }
}
