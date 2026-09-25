using LearningManagementSystem.Application.DTOs.AiTools;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Persistence.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class AiToolsController : Controller
    {
        private readonly IAiToolService _aiToolService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AiToolsController(IAiToolService aiToolService, UserManager<ApplicationUser> userManager)
        {
            _aiToolService = aiToolService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            var tools = await _aiToolService.GetAllAsync(cancellationToken);
            return View(tools);
        }

        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            var tool = await _aiToolService.GetByIdAsync(id, user?.Id, cancellationToken);
            if (tool is null)
            {
                return NotFound();
            }

            return View(tool);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkFeedback(MarkAiToolFeedbackDto dto, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var (success, message) = await _aiToolService.MarkFeedbackAsync(dto, user.Id, cancellationToken);
            TempData[success ? "Success" : "Error"] = message;

            return RedirectToAction(nameof(Details), new { id = dto.AIToolId });
        }
    }
}
