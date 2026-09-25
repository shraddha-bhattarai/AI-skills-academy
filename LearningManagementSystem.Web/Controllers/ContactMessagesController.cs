using LearningManagementSystem.Application.DTOs.ContactMessages;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Controllers;

// Stopgap: no Admin-area equivalent yet (planned for a later session). Locked down here in the meantime.
// Note: the public "Contact Us" form posts to HomeController.Contact directly via IContactMessageService,
// not through this controller, so locking this down does not affect the public contact form.
[Authorize(Roles = "Admin")]
public class ContactMessagesController : Controller
{
    private readonly IContactMessageService _service;
    private const int PageSize = 10;

    public ContactMessagesController(IContactMessageService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(string? search, string? sortBy, bool sortDescending = false, int page = 1)
    {
        var result = await _service.GetAllAsync(search, sortBy, sortDescending, page, PageSize);
        ViewBag.Search = search;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDescending = sortDescending;
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }

    public IActionResult Create()
    {
        return View(new CreateContactMessageDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateContactMessageDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        await _service.CreateAsync(dto);
        TempData["Success"] = "Contact message submitted successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        return View(new UpdateContactMessageDto
        {
            Id = item.Id,
            Name = item.Name,
            Email = item.Email,
            Subject = item.Subject,
            Message = item.Message,
            IsResolved = item.IsResolved
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateContactMessageDto dto)
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
            await _service.UpdateAsync(dto);
            TempData["Success"] = "Contact message updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var item = await _service.GetByIdAsync(id);
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
            await _service.DeleteAsync(id);
            TempData["Success"] = "Contact message deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
