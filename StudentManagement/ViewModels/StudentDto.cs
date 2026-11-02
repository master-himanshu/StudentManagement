using System.ComponentModel.DataAnnotations;

namespace StudentManagement.ViewModels
{
    /// <summary>
    /// Data returned by GET endpoints of the REST API. Deliberately flat and simple (JSON-friendly).
    /// </summary>
    public class StudentDto
    {
        public int StudentId { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public int YearLevel { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
    }

    /// <summary>
    /// Payload accepted by POST /api/students and PUT /api/students/{id}.
    /// Separate from StudentDto so clients can never set server-controlled fields
    /// like StudentId, DateCreated, DateUpdated directly.
    /// </summary>
    public class StudentCreateUpdateDto
    {
        [Required, StringLength(20, MinimumLength = 3)]
        public string StudentNumber { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Course { get; set; } = string.Empty;

        [Range(1, 6)]
        public int YearLevel { get; set; }
    }
}
