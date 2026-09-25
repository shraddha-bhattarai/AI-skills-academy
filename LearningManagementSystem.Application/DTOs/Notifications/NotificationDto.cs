using System.ComponentModel.DataAnnotations;
using LearningManagementSystem.Domain.Enums;

namespace LearningManagementSystem.Application.DTOs.Notifications;

public class NotificationDto
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreateNotificationDto
{
    [Required(ErrorMessage = "User is required.")]
    [Display(Name = "User")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required.")]
    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "Type is required.")]
    [Display(Name = "Type")]
    public NotificationType Type { get; set; }

    [Display(Name = "Is Read")]
    public bool IsRead { get; set; }
}

public class UpdateNotificationDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "User is required.")]
    [Display(Name = "User")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required.")]
    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "Type is required.")]
    [Display(Name = "Type")]
    public NotificationType Type { get; set; }

    [Display(Name = "Is Read")]
    public bool IsRead { get; set; }
}
