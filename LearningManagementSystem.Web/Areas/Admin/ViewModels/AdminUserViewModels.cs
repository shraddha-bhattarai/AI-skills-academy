namespace LearningManagementSystem.Web.Areas.Admin.ViewModels;

public class AdminUserListItemViewModel
{
    public string Id { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();

    public DateTime CreatedAt { get; set; }
}

public class AdminUserDetailsViewModel
{
    public string Id { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();

    public IReadOnlyList<string> AvailableRoles { get; set; } = Array.Empty<string>();

    public DateTime CreatedAt { get; set; }

    public bool IsLastActiveAdmin { get; set; }
}
