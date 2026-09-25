using System.Threading;
using System.Threading.Tasks;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Persistence.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class LessonsController : Controller
    {
        private readonly ILessonService _lessonService;
        private readonly ILessonProgressService _lessonProgressService;
        private readonly ICourseService _courseService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LessonsController(
            ILessonService lessonService,
            ILessonProgressService lessonProgressService,
            ICourseService courseService,
            UserManager<ApplicationUser> userManager)
        {
            _lessonService = lessonService;
            _lessonProgressService = lessonProgressService;
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

            var lessons = await _lessonService.GetStudentLessonsAsync(user.Id, courseId, cancellationToken);
            if (lessons == null)
            {
                TempData["Error"] = "You are not enrolled in that course, or it does not exist.";
                return RedirectToAction("Index", "Courses");
            }

            var course = await _courseService.GetByIdAsync(courseId, cancellationToken);
            ViewBag.CourseId = courseId;
            ViewBag.CourseTitle = course?.Title ?? "Course";

            return View(lessons);
        }

        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var lesson = await _lessonService.GetStudentLessonDetailsAsync(id, user.Id, cancellationToken);
            if (lesson == null)
            {
                TempData["Error"] = "Lesson not found or you are not enrolled in the associated course.";
                return RedirectToAction("Index", "Courses");
            }

            return View(lesson);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkComplete(int id, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var (success, message) = await _lessonProgressService.MarkLessonCompleteAsync(id, user.Id, cancellationToken);
            TempData[success ? "Success" : "Error"] = message;

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
