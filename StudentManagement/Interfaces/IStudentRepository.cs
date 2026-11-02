using StudentManagement.Models;

namespace StudentManagement.Interfaces
{
    /// <summary>
    /// Result wrapper for a paged query, keeping the "total count before paging" alongside the page of data.
    /// </summary>
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// Abstraction over data access for <see cref="Student"/> entities.
    /// The Repository Pattern isolates EF Core / SQL concerns from the rest of the application,
    /// which makes the Service layer (and its unit tests) independent of the actual database technology.
    /// </summary>
    public interface IStudentRepository
    {
        Task<PagedResult<Student>> GetPagedAsync(
            string? searchTerm,
            string sortColumn,
            string sortDirection,
            int pageNumber,
            int pageSize);

        Task<IEnumerable<Student>> GetAllAsync();

        Task<Student?> GetByIdAsync(int id);

        Task<Student> AddAsync(Student student);

        Task UpdateAsync(Student student);

        /// <summary>Soft-deletes (IsActive = false) the student with the given id. Returns false if not found.</summary>
        Task<bool> DeleteAsync(int id);

        Task<bool> StudentNumberExistsAsync(string studentNumber, int excludeStudentId = 0);

        Task<bool> EmailExistsAsync(string email, int excludeStudentId = 0);
    }
}
