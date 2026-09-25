using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Persistence.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class BookmarksController : Controller
    {
        private readonly IBookmarkService _bookmarkService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookmarksController(IBookmarkService bookmarkService, UserManager<ApplicationUser> userManager)
        {
            _bookmarkService = bookmarkService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var bookmarks = await _bookmarkService.GetStudentBookmarksAsync(user.Id, cancellationToken);
            return View(bookmarks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int courseId, string? returnUrl, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var (isBookmarked, message) = await _bookmarkService.ToggleAsync(user.Id, courseId, cancellationToken);
            TempData["Success"] = message;

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
