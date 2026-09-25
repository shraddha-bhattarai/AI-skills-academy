using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Application.DTOs.Certificates;

public class CertificateDto
{
    public int Id { get; set; }

    public string StudentId { get; set; } = string.Empty;

    public string StudentName { get; set; } = string.Empty;

    public string CertificateNumber { get; set; } = string.Empty;

    public DateTime IssuedDate { get; set; }

    public int CourseId { get; set; }

    public string CourseTitle { get; set; } = string.Empty;
}

public class CreateCertificateDto
{
    [Required(ErrorMessage = "Student is required.")]
    [Display(Name = "Student")]
    public string StudentId { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Certificate number cannot exceed 100 characters.")]
    [Display(Name = "Certificate Number")]
    public string? CertificateNumber { get; set; }

    [Required(ErrorMessage = "Issued date is required.")]
    [Display(Name = "Issued Date")]
    [DataType(DataType.Date)]
    public DateTime IssuedDate { get; set; } = DateTime.UtcNow.Date;

    [Required(ErrorMessage = "Course is required.")]
    [Display(Name = "Course")]
    public int CourseId { get; set; }
}

public class UpdateCertificateDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Student is required.")]
    [Display(Name = "Student")]
    public string StudentId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Certificate number is required.")]
    [StringLength(100, ErrorMessage = "Certificate number cannot exceed 100 characters.")]
    [Display(Name = "Certificate Number")]
    public string CertificateNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Issued date is required.")]
    [Display(Name = "Issued Date")]
    [DataType(DataType.Date)]
    public DateTime IssuedDate { get; set; }

    [Required(ErrorMessage = "Course is required.")]
    [Display(Name = "Course")]
    public int CourseId { get; set; }
}
