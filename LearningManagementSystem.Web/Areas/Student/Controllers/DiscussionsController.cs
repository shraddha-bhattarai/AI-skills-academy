using LearningManagementSystem.Application.DTOs.Discussions;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Persistence.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class DiscussionsController : Controller
    {
        private readonly IDiscussionService _discussionService;
        private readonly ICourseService _courseService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DiscussionsController(
            IDiscussionService discussionService,
            ICourseService courseService,
            UserManager<ApplicationUser> userManager)
        {
            _discussionService = discussionService;
            _courseService = courseService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int courseId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var discussions = await _discussionService.GetCourseDiscussionsAsync(courseId, user.Id, cancellationToken);
            if (discussions is null)
            {
                TempData["Error"] = "You must be enrolled in this course to view its discussions.";
                return RedirectToAction("Index", "Courses");
            }

            var course = await _courseService.GetByIdAsync(courseId, cancellationToken);
            ViewBag.CourseId = courseId;
            ViewBag.CourseTitle = course?.Title ?? "Course";

            return View(discussions);
        }

        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var discussion = await _discussionService.GetDiscussionDetailsAsync(id, user.Id, cancellationToken);
            if (discussion is null)
            {
                TempData["Error"] = "Discussion not found or you are not enrolled in the associated course.";
                return RedirectToAction("Index", "Courses");
            }

            return View(discussion);
        }

        public IActionResult Create(int courseId)
        {
            return View(new CreateDiscussionDto { CourseId = courseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDiscussionDto dto, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var (success, message) = await _discussionService.CreateDiscussionAsync(dto, user.Id, cancellationToken);
            if (!success)
            {
                TempData["Error"] = message;
                return RedirectToAction(nameof(Index), new { courseId = dto.CourseId });
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index), new { courseId = dto.CourseId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(CreateDiscussionReplyDto dto, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var (success, message) = await _discussionService.CreateReplyAsync(dto, user.Id, cancellationToken);
            TempData[success ? "Success" : "Error"] = message;

            return RedirectToAction(nameof(Details), new { id = dto.DiscussionId });
        }
    }
}
