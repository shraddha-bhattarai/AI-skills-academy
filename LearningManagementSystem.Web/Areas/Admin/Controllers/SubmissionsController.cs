using LearningManagementSystem.Application.DTOs.Submissions;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SubmissionsController : Controller
{
    private readonly ISubmissionService _submissionService;
    private const int PageSize = 10;

    public SubmissionsController(ISubmissionService submissionService)
    {
        _submissionService = submissionService;
    }

    public async Task<IActionResult> Index(string? search, SubmissionStatus? status, string? sortBy, bool sortDescending = false, int page = 1, CancellationToken cancellationToken = default)
    {
        var result = await _submissionService.GetAllAsync(search, status, sortBy, sortDescending, page, PageSize, cancellationToken);
        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDescending = sortDescending;
        return View(result);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
    {
        var item = await _submissionService.GetByIdAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Grade(GradeSubmissionDto dto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid grading input. Please check the values and try again.";
            return RedirectToAction(nameof(Details), new { id = dto.Id });
        }

        try
        {
            await _submissionService.GradeAsync(dto, cancellationToken);
            TempData["Success"] = "Submission graded successfully.";
        }
        catch (NotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Details), new { id = dto.Id });
    }
}
