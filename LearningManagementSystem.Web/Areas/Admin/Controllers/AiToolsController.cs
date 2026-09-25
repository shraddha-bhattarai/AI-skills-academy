using LearningManagementSystem.Application.DTOs.AiTools;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AiToolsController : Controller
{
    private readonly IAiToolService _aiToolService;
    private const int PageSize = 10;

    public AiToolsController(IAiToolService aiToolService)
    {
        _aiToolService = aiToolService;
    }

    public async Task<IActionResult> Index(string? search, int page = 1, CancellationToken cancellationToken = default)
    {
        var result = await _aiToolService.GetAllForAdminAsync(search, page, PageSize, cancellationToken);
        ViewBag.Search = search;
        return View(result);
    }

    public IActionResult Create()
    {
        return View(new CreateAiToolDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAiToolDto dto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        await _aiToolService.CreateAsync(dto, cancellationToken);
        TempData["Success"] = "AI tool added successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        var item = await _aiToolService.GetByIdAsync(id, null, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return View(new UpdateAiToolDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Url = item.Url,
            Category = item.Category
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateAiToolDto dto, CancellationToken cancellationToken = default)
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
            await _aiToolService.UpdateAsync(dto, cancellationToken);
            TempData["Success"] = "AI tool updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _aiToolService.DeleteAsync(id, cancellationToken);
            TempData["Success"] = "AI tool deleted successfully.";
        }
        catch (NotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }
}
