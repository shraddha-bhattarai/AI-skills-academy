using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Persistence.Context;
using LearningManagementSystem.Persistence.Identity;
using LearningManagementSystem.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace LearningManagementSystem.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICourseService _courseService;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            ICourseService courseService,
            ICategoryService categoryService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _logger = logger;
            _courseService = courseService;
            _categoryService = categoryService;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            var featured = await _courseService.GetAllAsync(
                search: null, sortBy: "createdat", sortDescending: true,
                page: 1, pageSize: 6, publishedOnly: true, cancellationToken: cancellationToken);

            var categories = await _categoryService.GetAllAsync(
                search: null, sortBy: "name", sortDescending: false,
                page: 1, pageSize: 6, cancellationToken: cancellationToken);

            var studentUsers = await _userManager.GetUsersInRoleAsync("Student");

            var viewModel = new HomeIndexViewModel
            {
                TotalPublishedCourses = featured.TotalCount,
                TotalStudents = studentUsers.Count(u => u.IsActive),
                TotalCertificatesIssued = await _context.Certificates.CountAsync(cancellationToken),
                FeaturedCourses = featured.Items,
                Categories = categories.Items
            };

            return View(viewModel);
        }

        public async Task<IActionResult> About(CancellationToken cancellationToken = default)
        {
            var studentUsers = await _userManager.GetUsersInRoleAsync("Student");

            var viewModel = new HomeIndexViewModel
            {
                TotalPublishedCourses = (await _courseService.GetAllAsync(null, null, false, 1, 1, publishedOnly: true, cancellationToken: cancellationToken)).TotalCount,
                TotalStudents = studentUsers.Count(u => u.IsActive),
                TotalCertificatesIssued = await _context.Certificates.CountAsync(cancellationToken)
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View(new LearningManagementSystem.Application.DTOs.ContactMessages.CreateContactMessageDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(
            LearningManagementSystem.Application.DTOs.ContactMessages.CreateContactMessageDto dto,
            [FromServices] LearningManagementSystem.Application.Interfaces.Services.IContactMessageService contactMessageService,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            await contactMessageService.CreateAsync(dto, cancellationToken);
            TempData["Success"] = "Thank you. Your message has been saved for the academy administrator.";
            return RedirectToAction(nameof(Contact));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
