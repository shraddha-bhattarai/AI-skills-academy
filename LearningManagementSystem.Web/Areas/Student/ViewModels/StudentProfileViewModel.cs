using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Web.Areas.Student.ViewModels
{
    public class StudentProfileViewModel
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? ProfilePicture { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
