using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Application.DTOs.Announcements;

public class AnnouncementDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime PublishDate { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreateAnnouncementDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required.")]
    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "Publish date is required.")]
    [Display(Name = "Publish Date")]
    [DataType(DataType.DateTime)]
    public DateTime PublishDate { get; set; } = DateTime.UtcNow;
}

public class UpdateAnnouncementDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required.")]
    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "Publish date is required.")]
    [Display(Name = "Publish Date")]
    [DataType(DataType.DateTime)]
    public DateTime PublishDate { get; set; }
}
