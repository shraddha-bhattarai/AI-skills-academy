using LearningManagementSystem.Application.DTOs.Certificates;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Repositories;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Web.Extensions;
using LearningManagementSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CertificatesController : Controller
{
    private readonly ICertificateService _certificateService;
    private readonly ICourseService _courseService;
    private readonly IUserRepository _userRepository;
    private const int PageSize = 10;

    public CertificatesController(
        ICertificateService certificateService,
        ICourseService courseService,
        IUserRepository userRepository)
    {
        _certificateService = certificateService;
        _courseService = courseService;
        _userRepository = userRepository;
    }

    public async Task<IActionResult> Index(string? search, string? sortBy, bool sortDescending = false, int page = 1)
    {
        var result = await _certificateService.GetAllAsync(search, sortBy, sortDescending, page, PageSize);
        ViewBag.Search = search;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDescending = sortDescending;
        return View(result);
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await _certificateService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        return View(new CreateCertificateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCertificateDto dto)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(dto.StudentId, dto.CourseId);
            return View(dto);
        }

        try
        {
            await _certificateService.CreateAsync(dto);
            TempData["Success"] = "Certificate created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateLookupsAsync(dto.StudentId, dto.CourseId);
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await _certificateService.GetByIdAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        await PopulateLookupsAsync(item.StudentId, item.CourseId);
        return View(new UpdateCertificateDto
        {
            Id = item.Id,
            StudentId = item.StudentId,
            CertificateNumber = item.CertificateNumber,
            IssuedDate = item.IssuedDate,
            CourseId = item.CourseId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateCertificateDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(dto.StudentId, dto.CourseId);
            return View(dto);
        }

        try
        {
            await _certificateService.UpdateAsync(dto);
            TempData["Success"] = "Certificate updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateLookupsAsync(dto.StudentId, dto.CourseId);
            return View(dto);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var item = await _certificateService.GetByIdAsync(id);
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
            await _certificateService.DeleteAsync(id);
            TempData["Success"] = "Certificate deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken = default)
    {
        var certificate = await _certificateService.GetByIdAsync(id, cancellationToken);
        if (certificate is null)
        {
            return NotFound();
        }

        var pdfBytes = CertificatePdfService.Generate(certificate);
        return File(pdfBytes, "application/pdf", $"Certificate_{certificate.CertificateNumber}.pdf");
    }

    private async Task PopulateLookupsAsync(string? selectedStudentId = null, int? selectedCourseId = null)
    {
        var users = await _userRepository.GetAllUsersAsync();
        var courses = await _courseService.GetLookupAsync();
        ViewBag.StudentId = users.ToSelectList(selectedStudentId);
        ViewBag.CourseId = courses.ToSelectList(selectedCourseId);
    }
}
