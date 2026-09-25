using LearningManagementSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Controllers;

// Public category catalog only. Admin CRUD lives in Areas/Admin/Controllers/CategoriesController.
public class CategoriesController : Controller
{
    private readonly ICategoryService _service;
    private const int PageSize = 10;

    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? search, string? sortBy, bool sortDescending = false, int page = 1)
    {
        var result = await _service.GetAllAsync(search, sortBy, sortDescending, page, PageSize);
        ViewBag.Search = search;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDescending = sortDescending;
        return View(result);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }
}
