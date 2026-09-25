using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LearningManagementSystem.Persistence.Identity;
using LearningManagementSystem.Web.Areas.Student.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class ProfileController : Controller
    {
        private static readonly string[] AllowedPictureExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxPictureSizeBytes = 5 * 1024 * 1024; // 5 MB

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var viewModel = new StudentProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                ProfilePicture = user.ProfilePicture,
                CreatedAt = user.CreatedAt
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(StudentProfileViewModel model, IFormFile? profilePictureFile, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                model.Email = user.Email ?? string.Empty;
                model.ProfilePicture = user.ProfilePicture;
                model.CreatedAt = user.CreatedAt;
                return View(model);
            }

            if (profilePictureFile != null && profilePictureFile.Length > 0)
            {
                var extension = Path.GetExtension(profilePictureFile.FileName).ToLowerInvariant();
                if (!AllowedPictureExtensions.Contains(extension))
                {
                    ModelState.AddModelError(string.Empty, $"Image type '{extension}' is not allowed. Allowed types: {string.Join(", ", AllowedPictureExtensions)}.");
                    model.Email = user.Email ?? string.Empty;
                    model.ProfilePicture = user.ProfilePicture;
                    model.CreatedAt = user.CreatedAt;
                    return View(model);
                }

                if (profilePictureFile.Length > MaxPictureSizeBytes)
                {
                    ModelState.AddModelError(string.Empty, "Image size exceeds the maximum allowed size of 5 MB.");
                    model.Email = user.Email ?? string.Empty;
                    model.ProfilePicture = user.ProfilePicture;
                    model.CreatedAt = user.CreatedAt;
                    return View(model);
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profile-pictures");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(profilePictureFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePictureFile.CopyToAsync(stream, cancellationToken);
                }

                user.ProfilePicture = $"/uploads/profile-pictures/{uniqueFileName}";
            }

            user.FirstName = model.FirstName.Trim();
            user.LastName = model.LastName.Trim();

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Your profile has been updated successfully!";
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                model.Email = user.Email ?? string.Empty;
                model.ProfilePicture = user.ProfilePicture;
                model.CreatedAt = user.CreatedAt;
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
