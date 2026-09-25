using LearningManagementSystem.Persistence.Identity;
using LearningManagementSystem.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private const int PageSize = 15;

    public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    { 
        page = Math.Max(1, page);
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                (u.FirstName + " " + u.LastName).ToLower().Contains(term) ||
                (u.Email ?? string.Empty).ToLower().Contains(term));
        }

        var totalCount = query.Count();
        var users = query
            .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        var items = new List<AdminUserListItemViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            items.Add(new AdminUserListItemViewModel
            {
                Id = user.Id,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Email = user.Email ?? string.Empty,
                IsActive = user.IsActive,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt
            });
        }

        ViewBag.Search = search;
        ViewBag.Page = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

        return View(items);
    }

    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var isLastActiveAdmin = await IsLastActiveAdminAsync(user);

        var viewModel = new AdminUserDetailsViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            IsActive = user.IsActive,
            Roles = roles.ToList(),
            AvailableRoles = _roleManager.Roles.Select(r => r.Name!).OrderBy(n => n).ToList(),
            CreatedAt = user.CreatedAt,
            IsLastActiveAdmin = isLastActiveAdmin
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(string id, string newRole)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (!await _roleManager.RoleExistsAsync(newRole))
        {
            TempData["Error"] = "That role does not exist.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        if (currentRoles.Contains(newRole) && currentRoles.Count == 1)
        {
            TempData["Error"] = $"{user.Email} already has the {newRole} role.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Safety: never let the last active Admin be demoted away from the Admin role.
        if (currentRoles.Contains("Admin") && newRole != "Admin" && await IsLastActiveAdminAsync(user))
        {
            TempData["Error"] = "You cannot change this user's role — they are the last active Admin account. Promote another user to Admin first.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (currentRoles.Any())
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        await _userManager.AddToRoleAsync(user, newRole);

        TempData["Success"] = $"{user.Email}'s role was changed to {newRole}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        // Safety: never let the last active Admin be deactivated (would lock the whole system out).
        if (user.IsActive && await IsLastActiveAdminAsync(user))
        {
            TempData["Error"] = "You cannot deactivate this user — they are the last active Admin account. Promote another user to Admin first.";
            return RedirectToAction(nameof(Details), new { id });
        }

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        TempData["Success"] = user.IsActive
            ? $"{user.Email} has been reactivated and can log in again."
            : $"{user.Email} has been deactivated and can no longer log in.";

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<bool> IsLastActiveAdminAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains("Admin"))
        {
            return false;
        }

        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        var activeAdminCount = admins.Count(a => a.IsActive);

        return user.IsActive && activeAdminCount <= 1;
    }
}
