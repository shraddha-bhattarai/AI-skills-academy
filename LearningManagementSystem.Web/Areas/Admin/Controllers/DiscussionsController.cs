using LearningManagementSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DiscussionsController : Controller
{
    private readonly IDiscussionService _discussionService;
    private const int PageSize = 15;

    public DiscussionsController(IDiscussionService discussionService)
    {
        _discussionService = discussionService;
    }

    public async Task<IActionResult> Index(string? search, int page = 1, CancellationToken cancellationToken = default)
    {
        var result = await _discussionService.GetAllForModerationAsync(search, page, PageSize, cancellationToken);
        ViewBag.Search = search;
        return View(result);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
    {
        var discussion = await _discussionService.GetForModerationAsync(id, cancellationToken);
        if (discussion is null)
        {
            return NotFound();
        }

        return View(discussion);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        await _discussionService.DeleteDiscussionAsync(id, cancellationToken);
        TempData["Success"] = "Discussion removed.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReply(int replyId, int discussionId, CancellationToken cancellationToken = default)
    {
        await _discussionService.DeleteReplyAsync(replyId, cancellationToken);
        TempData["Success"] = "Reply removed.";
        return RedirectToAction(nameof(Details), new { id = discussionId });
    }
}
