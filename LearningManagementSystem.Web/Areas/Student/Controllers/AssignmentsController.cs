using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LearningManagementSystem.Application.DTOs.Assignments;
using LearningManagementSystem.Application.Exceptions;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Domain.Enums;
using LearningManagementSystem.Persistence.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class AssignmentsController : Controller
    {
        private static readonly string[] AllowedExtensions =
        {
            ".pdf", ".doc", ".docx", ".txt", ".zip", ".rar",
            ".ppt", ".pptx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png"
        };

        private const long MaxFileSizeBytes = 25 * 1024 * 1024; // 25 MB — real scanned/image-heavy submissions can be large

        private readonly IAssignmentService _assignmentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public AssignmentsController(
            IAssignmentService assignmentService,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _assignmentService = assignmentService;
            _userManager = userManager;
            _environment = environment;
        }

        public async Task<IActionResult> Index(string? search, string filter = "All", CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var assignments = await _assignmentService.GetStudentAssignmentsAsync(user.Id, cancellationToken);

            var assignmentList = assignments.ToList();

            // Calculate overall stats before status filtering
            var now = DateTime.UtcNow;
            ViewBag.TotalCount = assignmentList.Count;
            ViewBag.PendingCount = assignmentList.Count(a => (a.SubmissionStatus == null || a.SubmissionStatus == SubmissionStatus.Pending) && a.DueDate >= now);
            ViewBag.SubmittedCount = assignmentList.Count(a => a.SubmissionStatus == SubmissionStatus.Submitted);
            ViewBag.GradedCount = assignmentList.Count(a => a.SubmissionStatus == SubmissionStatus.Graded);
            ViewBag.OverdueCount = assignmentList.Count(a => (a.SubmissionStatus == null || a.SubmissionStatus == SubmissionStatus.Pending) && a.DueDate < now);

            // Filter search
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                assignmentList = assignmentList.Where(a =>
                    a.Title.ToLower().Contains(term) ||
                    a.CourseTitle.ToLower().Contains(term) ||
                    a.Description.ToLower().Contains(term)
                ).ToList();
            }

            // Filter status tab
            var filteredAssignments = filter switch
            {
                "Pending" => assignmentList.Where(a => (a.SubmissionStatus == null || a.SubmissionStatus == SubmissionStatus.Pending) && a.DueDate >= now).ToList(),
                "Submitted" => assignmentList.Where(a => a.SubmissionStatus == SubmissionStatus.Submitted).ToList(),
                "Graded" => assignmentList.Where(a => a.SubmissionStatus == SubmissionStatus.Graded).ToList(),
                "Overdue" => assignmentList.Where(a => (a.SubmissionStatus == null || a.SubmissionStatus == SubmissionStatus.Pending) && a.DueDate < now).ToList(),
                _ => assignmentList
            };

            ViewBag.Search = search ?? string.Empty;
            ViewBag.ActiveFilter = filter;

            return View(filteredAssignments);
        }

        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var assignment = await _assignmentService.GetStudentAssignmentDetailsAsync(id, user.Id, cancellationToken);
            if (assignment == null)
            {
                TempData["Error"] = "Assignment not found or you are not enrolled in the associated course.";
                return RedirectToAction(nameof(Index));
            }

            return View(assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // No [RequestSizeLimit] here on purpose: a per-action limit lower than Kestrel's own request-body
        // ceiling makes Kestrel abort the TCP connection mid-upload once the limit is crossed — the browser
        // sees this as a dead/reset connection, not a clean error page, which reads exactly like a "crash"
        // even though the server responded correctly. Kestrel's ceiling is configured generously in Program.cs
        // instead, well above MaxFileSizeBytes below, so the graceful check in this action is what a student
        // actually sees for an oversized file, not a raw connection abort.
        public async Task<IActionResult> Submit(int id, IFormFile? file, string? submissionNotes, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            string? relativePath = null;

            if (file != null && file.Length > 0)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(extension))
                {
                    TempData["Error"] = $"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                if (file.Length > MaxFileSizeBytes)
                {
                    TempData["Error"] = "File size exceeds the maximum allowed size of 25 MB.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "submissions");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream, cancellationToken);
                }

                relativePath = $"/uploads/submissions/{uniqueFileName}";
            }

            try
            {
                await _assignmentService.SubmitAssignmentAsync(id, user.Id, relativePath, cancellationToken);
                TempData["Success"] = "Your assignment solution has been submitted successfully!";
            }
            catch (NotFoundException)
            {
                TempData["Error"] = "Assignment not found.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while submitting your assignment. Please try again.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
