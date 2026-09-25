using System;
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
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string filter = "All", CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var notifications = await _notificationService.GetStudentNotificationsAsync(user.Id, filter, cancellationToken);
            var allNotifications = await _notificationService.GetStudentNotificationsAsync(user.Id, "All", cancellationToken);

            ViewBag.ActiveFilter = filter;
            ViewBag.UnreadCount = await _notificationService.GetUnreadCountAsync(user.Id, cancellationToken);
            ViewBag.TotalCount = allNotifications.Count;

            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id, string returnUrl = "", CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Server-side ownership check: verifies notification belongs to the logged-in student
            await _notificationService.MarkAsReadAsync(id, user.Id, cancellationToken);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Server-side ownership check: marks only notifications belonging to the logged-in student
            await _notificationService.MarkAllAsReadAsync(user.Id, cancellationToken);
            TempData["Success"] = "All notifications marked as read.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Server-side ownership check: returns null if notification does not belong to logged-in student
            var notification = await _notificationService.GetStudentNotificationByIdAsync(id, user.Id, cancellationToken);
            if (notification == null)
            {
                return NotFound();
            }

            if (!notification.IsRead)
            {
                await _notificationService.MarkAsReadAsync(id, user.Id, cancellationToken);
                notification.IsRead = true;
            }

            return View(notification);
        }
    }
}
