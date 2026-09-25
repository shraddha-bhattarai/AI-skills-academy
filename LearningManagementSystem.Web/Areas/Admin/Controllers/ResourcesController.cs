using LearningManagementSystem.Application.DTOs.Resources;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ResourcesController : Controller
{
    private readonly IResourceService _resourceService;
    private readonly ILessonService _lessonService;

    public ResourcesController(IResourceService resourceService, ILessonService lessonService)
    {
        _resourceService = resourceService;
        _lessonService = lessonService;
    }

    public async Task<IActionResult> Index(int lessonId)
    {
        var lesson = await _lessonService.GetByIdAsync(lessonId);
        if (lesson is null)
        {
            return NotFound();
        }

        var resources = await _resourceService.GetByLessonIdAsync(lessonId);
        ViewBag.Lesson = lesson;
        return View(resources);
    }

    public async Task<IActionResult> Create(int lessonId)
    {
        var lesson = await _lessonService.GetByIdAsync(lessonId);
        if (lesson is null)
        {
            return NotFound();
        }

        ViewBag.Lesson = lesson;
        return View(new CreateResourceDto { LessonId = lessonId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateResourceDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Lesson = await _lessonService.GetByIdAsync(dto.LessonId);
            return View(dto);
        }

        try
        {
            await _resourceService.CreateAsync(dto);
            TempData["Success"] = "Resource added successfully.";
            return RedirectToAction(nameof(Index), new { lessonId = dto.LessonId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Lesson = await _lessonService.GetByIdAsync(dto.LessonId);
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _resourceService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        ViewBag.Lesson = await _lessonService.GetByIdAsync(item.LessonId);
        return View(new UpdateResourceDto
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            FileUrl = item.FileUrl,
            LessonId = item.LessonId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateResourceDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Lesson = await _lessonService.GetByIdAsync(dto.LessonId);
            return View(dto);
        }

        try
        {
            await _resourceService.UpdateAsync(dto);
            TempData["Success"] = "Resource updated successfully.";
            return RedirectToAction(nameof(Index), new { lessonId = dto.LessonId });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Lesson = await _lessonService.GetByIdAsync(dto.LessonId);
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int lessonId)
    {
        try
        {
            await _resourceService.DeleteAsync(id);
            TempData["Success"] = "Resource deleted successfully.";
        }
        catch (NotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index), new { lessonId });
    }
}
