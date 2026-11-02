using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagement.Models
{
    /// <summary>
    /// Represents a Student record in the system.
    /// This is the EF Core "entity" class that maps directly to the dbo.Students table.
    ///
    /// NOTE FOR STUDENTS:
    /// We keep Data Annotations here as a *second* layer of validation (the database/model layer).
    /// The primary validation the user sees on forms happens on <see cref="ViewModels.StudentViewModel"/>.
    /// Keeping both is a common enterprise pattern: the ViewModel protects the UI/UX,
    /// the Entity protects data integrity no matter which code path writes to the database.
    /// </summary>
    public class Student
    {
        /// <summary>
        /// Primary Key. EF Core recognizes "Id" or "{ClassName}Id" as a convention-based key,
        /// so no extra [Key] attribute is required, but we add it for clarity/teaching purposes.
        /// </summary>
        [Key]
        public int StudentId { get; set; }

        /// <summary>
        /// A unique, human-friendly identifier for the student (e.g. "2026-00001").
        /// </summary>
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string StudentNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Course { get; set; } = string.Empty;

        /// <summary>
        /// Year level of the student, restricted to a realistic range (1 to 6, covers most degree programs).
        /// </summary>
        [Range(1, 6)]
        public int YearLevel { get; set; }

        /// <summary>
        /// Timestamp automatically set when the record is created. Never edited by the user.
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Timestamp automatically updated every time the record changes. Null until the first update.
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime? DateUpdated { get; set; }

        /// <summary>
        /// Soft-delete flag. Rather than physically removing rows (which destroys history/audit trail),
        /// we flag them inactive. All "list" and "get" queries filter on IsActive = true by default.
        /// This is an OPTIONAL enterprise pattern shown here for teaching purposes.
        /// </summary>
        public bool IsActive { get; set; } = true;

        // NotMapped convenience property used only for display purposes (never persisted).
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}
