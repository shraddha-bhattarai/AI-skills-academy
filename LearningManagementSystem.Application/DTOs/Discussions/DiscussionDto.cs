using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Application.DTOs.Discussions;

public class DiscussionDto
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;

    public string StudentId { get; set; } = string.Empty;

    public string StudentName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public int ReplyCount { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class DiscussionDetailsDto : DiscussionDto
{
    public IReadOnlyList<DiscussionReplyDto> Replies { get; set; } = Array.Empty<DiscussionReplyDto>();
}

public class DiscussionReplyDto
{
    public int Id { get; set; }

    public int DiscussionId { get; set; }

    public string StudentId { get; set; } = string.Empty;

    public string StudentName { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public class CreateDiscussionDto
{
    [Required]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required.")]
    [StringLength(4000, ErrorMessage = "Content cannot exceed 4000 characters.")]
    public string Content { get; set; } = string.Empty;
}

public class CreateDiscussionReplyDto
{
    [Required]
    public int DiscussionId { get; set; }

    [Required(ErrorMessage = "Reply cannot be empty.")]
    [StringLength(2000, ErrorMessage = "Reply cannot exceed 2000 characters.")]
    public string Content { get; set; } = string.Empty;
}
