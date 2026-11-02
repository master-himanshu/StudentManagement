using StudentManagement.ViewModels;

namespace StudentManagement.Interfaces
{
    /// <summary>
    /// Business/application logic contract for Student operations.
    /// Controllers (both MVC and API) depend on this abstraction only - never on the repository or DbContext
    /// directly. This keeps controllers "thin" and follows the Dependency Inversion Principle (the "D" in SOLID).
    /// </summary>
    public interface IStudentService
    {
        Task<StudentIndexViewModel> GetStudentListAsync(
            string? searchTerm, string sortColumn, string sortDirection, int pageNumber, int pageSize);

        Task<IEnumerable<StudentDto>> GetAllAsync();

        Task<StudentDto?> GetByIdAsync(int id);

        /// <summary>
        /// Creates a new student. Returns (true, createdDto, null) on success,
        /// or (false, null, errorMessage) if a business rule (e.g. duplicate email) is violated.
        /// </summary>
        Task<(bool Success, StudentDto? Student, string? ErrorMessage)> CreateAsync(StudentCreateUpdateDto dto);

        Task<(bool Success, StudentDto? Student, string? ErrorMessage)> UpdateAsync(int id, StudentCreateUpdateDto dto);

        Task<bool> DeleteAsync(int id);
    }
}
