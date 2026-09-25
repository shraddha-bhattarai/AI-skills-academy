using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Application.DTOs.AiTools;

public class AiToolDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string? Category { get; set; }

    public int UsefulCount { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class AiToolDetailsDto : AiToolDto
{
    public bool MarkedUsefulByCurrentUser { get; set; }

    public IReadOnlyList<AiToolFeedbackDto> Comments { get; set; } = Array.Empty<AiToolFeedbackDto>();
}

public class AiToolFeedbackDto
{
    public string StudentName { get; set; } = string.Empty;

    public bool IsUseful { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreateAiToolDto
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "URL is required.")]
    [Url(ErrorMessage = "Please enter a valid URL.")]
    public string Url { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
    public string? Category { get; set; }
}

public class UpdateAiToolDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "URL is required.")]
    [Url(ErrorMessage = "Please enter a valid URL.")]
    public string Url { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
    public string? Category { get; set; }
}

public class MarkAiToolFeedbackDto
{
    [Required]
    public int AIToolId { get; set; }

    public bool IsUseful { get; set; }

    [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
    public string? Comment { get; set; }
}
