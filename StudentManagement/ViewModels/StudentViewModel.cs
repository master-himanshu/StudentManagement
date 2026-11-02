using System.ComponentModel.DataAnnotations;

namespace StudentManagement.ViewModels
{
    /// <summary>
    /// ViewModel used exclusively by the Create/Edit Razor forms and the REST API request bodies.
    ///
    /// WHY A SEPARATE VIEWMODEL INSTEAD OF BINDING DIRECTLY TO THE ENTITY?
    /// 1. It prevents "over-posting" attacks (a malicious client setting fields like IsActive or DateCreated).
    /// 2. It lets us tailor validation messages / attributes specifically for the UI.
    /// 3. It decouples the persistence model (Student) from the presentation model, so either can change
    ///    independently - a core SOLID (Single Responsibility) idea applied to layers, not just classes.
    /// </summary>
    public class StudentViewModel
    {
        /// <summary>Guid.Empty when creating a new student, otherwise the id of the student being edited.</summary>
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Student number is required.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Student number must be between 3 and 20 characters.")]
        [Display(Name = "Student Number")]
        public string StudentNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course is required.")]
        [StringLength(100)]
        public string Course { get; set; } = string.Empty;

        [Required]
        [Range(1, 6, ErrorMessage = "Year level must be between 1 and 6.")]
        [Display(Name = "Year Level")]
        public int YearLevel { get; set; }

        // Read-only informational fields shown on the Edit/Details view, not editable by the user.
        public DateTime? DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
}
