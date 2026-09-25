using LearningManagementSystem.Application.DTOs.Notifications;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Enums;
using LearningManagementSystem.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LearningManagementSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class NotificationsController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly IUserRepository _userRepository;
    private const int PageSize = 10;

    public NotificationsController(INotificationService notificationService, IUserRepository userRepository)
    {
        _notificationService = notificationService;
        _userRepository = userRepository;
    }

    public async Task<IActionResult> Index(string? search, string? sortBy, bool sortDescending = false, int page = 1)
    {
        var result = await _notificationService.GetAllAsync(search, sortBy, sortDescending, page, PageSize);
        ViewBag.Search = search;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDescending = sortDescending;
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _notificationService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        return View(new CreateNotificationDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateNotificationDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(dto.UserId, dto.Type);
            return View(dto);
        }

        try
        {
            await _notificationService.CreateAsync(dto);
            TempData["Success"] = "Notification created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateLookupsAsync(dto.UserId, dto.Type);
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _notificationService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        await PopulateLookupsAsync(item.UserId, item.Type);
        return View(new UpdateNotificationDto
        {
            Id = item.Id,
            UserId = item.UserId,
            Title = item.Title,
            Message = item.Message,
            Type = item.Type,
            IsRead = item.IsRead
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateNotificationDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(dto.UserId, dto.Type);
            return View(dto);
        }

        try
        {
            await _notificationService.UpdateAsync(dto);
            TempData["Success"] = "Notification updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateLookupsAsync(dto.UserId, dto.Type);
            return View(dto);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var item = await _notificationService.GetByIdAsync(id);
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
            await _notificationService.DeleteAsync(id);
            TempData["Success"] = "Notification deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    private async Task PopulateLookupsAsync(string? selectedUserId = null, NotificationType? selectedType = null)
    {
        var users = await _userRepository.GetAllUsersAsync();
        ViewBag.UserId = users.ToSelectList(selectedUserId);
        ViewBag.Type = new SelectList(
            Enum.GetValues<NotificationType>().Select(t => new { Value = (int)t, Text = t.ToString() }),
            "Value", "Text",
            (int?)selectedType);
    }
}
