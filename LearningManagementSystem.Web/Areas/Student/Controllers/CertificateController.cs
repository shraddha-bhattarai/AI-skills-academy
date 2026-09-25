using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LearningManagementSystem.Application.Interfaces.Services;
using LearningManagementSystem.Persistence.Identity;
using LearningManagementSystem.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class CertificateController : Controller
    {
        private readonly ICertificateService _certificateService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CertificateController(
            ICertificateService certificateService,
            UserManager<ApplicationUser> userManager)
        {
            _certificateService = certificateService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var certificates = await _certificateService.GetStudentCertificatesAsync(user.Id, cancellationToken);

            ViewBag.TotalCount = certificates.Count;
            ViewBag.RecentCount = certificates.Count(c => c.IssuedDate >= DateTime.UtcNow.AddDays(-30));

            return View(certificates);
        }

        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Server-side authorization check: verifies certificate belongs to the logged-in student
            var certificate = await _certificateService.GetStudentCertificateByIdAsync(id, user.Id, cancellationToken);
            if (certificate == null)
            {
                return NotFound();
            }

            return View(certificate);
        }

        public async Task<IActionResult> Download(int id, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Server-side authorization check: verifies certificate belongs to the logged-in student
            var certificate = await _certificateService.GetStudentCertificateByIdAsync(id, user.Id, cancellationToken);
            if (certificate == null)
            {
                return NotFound();
            }

            var pdfBytes = CertificatePdfService.Generate(certificate);
            return File(pdfBytes, "application/pdf", $"Certificate_{certificate.CertificateNumber}.pdf");
        }
    }
}
