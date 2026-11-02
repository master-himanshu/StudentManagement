using Microsoft.EntityFrameworkCore;
using StudentManagement.Interfaces;
using StudentManagement.Models;

namespace StudentManagement.Data.Repositories
{
    /// <summary>
    /// EF Core implementation of <see cref="IStudentRepository"/>.
    /// Every query filters on IsActive = true so soft-deleted students disappear from normal use,
    /// without ever losing the row from the database (useful for audits/reporting).
    /// </summary>
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Student>> GetPagedAsync(
            string? searchTerm, string sortColumn, string sortDirection, int pageNumber, int pageSize)
        {
            IQueryable<Student> query = _context.Students.Where(s => s.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(s =>
                    s.StudentNumber.ToLower().Contains(term) ||
                    s.FirstName.ToLower().Contains(term) ||
                    s.LastName.ToLower().Contains(term) ||
                    s.Email.ToLower().Contains(term) ||
                    s.Course.ToLower().Contains(term));
            }

            bool descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            query = sortColumn switch
            {
                "StudentNumber" => descending ? query.OrderByDescending(s => s.StudentNumber) : query.OrderBy(s => s.StudentNumber),
                "FirstName" => descending ? query.OrderByDescending(s => s.FirstName) : query.OrderBy(s => s.FirstName),
                "Email" => descending ? query.OrderByDescending(s => s.Email) : query.OrderBy(s => s.Email),
                "Course" => descending ? query.OrderByDescending(s => s.Course) : query.OrderBy(s => s.Course),
                "YearLevel" => descending ? query.OrderByDescending(s => s.YearLevel) : query.OrderBy(s => s.YearLevel),
                _ => descending ? query.OrderByDescending(s => s.LastName) : query.OrderBy(s => s.LastName),
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<Student> { Items = items, TotalCount = totalCount };
        }

        public async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await _context.Students
                .Where(s => s.IsActive)
                .AsNoTracking()
                .OrderBy(s => s.LastName)
                .ToListAsync();
        }

        public async Task<Student?> GetByIdAsync(int id)
        {
            return await _context.Students
                .FirstOrDefaultAsync(s => s.StudentId == id && s.IsActive);
        }

        public async Task<Student> AddAsync(Student student)
        {
            student.DateCreated = DateTime.UtcNow;
            student.IsActive = true;

            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            return student;
        }

        public async Task UpdateAsync(Student student)
        {
            student.DateUpdated = DateTime.UtcNow;
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == id && s.IsActive);
            if (student is null)
            {
                return false;
            }

            // Soft delete: flag inactive instead of removing the row.
            student.IsActive = false;
            student.DateUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> StudentNumberExistsAsync(string studentNumber, int excludeStudentId = default)
        {
            return await _context.Students.AnyAsync(s =>
                s.StudentNumber.ToLower() == studentNumber.ToLower() &&
                s.IsActive &&
                s.StudentId != excludeStudentId);
        }

        public async Task<bool> EmailExistsAsync(string email, int excludeStudentId = default)
        {
            return await _context.Students.AnyAsync(s =>
                s.Email.ToLower() == email.ToLower() &&
                s.IsActive &&
                s.StudentId != excludeStudentId);
        }
    }
}
