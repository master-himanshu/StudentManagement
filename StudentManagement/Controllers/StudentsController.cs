using Microsoft.AspNetCore.Mvc;
using StudentManagement.Interfaces;
using StudentManagement.ViewModels;

namespace StudentManagement.Controllers
{
    /// <summary>
    /// MVC controller that serves the human-facing Razor views for managing students
    /// (List / Details / Create / Edit / Delete). All actual work is delegated to
    /// <see cref="IStudentService"/> - the controller only handles HTTP concerns
    /// (model binding, ModelState validation, choosing which view/redirect to return).
    /// </summary>
    public class StudentsController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(IStudentService studentService, ILogger<StudentsController> logger)
        {
            _studentService = studentService;
            _logger = logger;
        }

        // GET: /Students?searchTerm=&sortColumn=&sortDirection=&page=
        public async Task<IActionResult> Index(string? searchTerm, string sortColumn = "LastName",
            string sortDirection = "asc", int page = 1, int pageSize = 10)
        {
            var viewModel = await _studentService.GetStudentListAsync(searchTerm, sortColumn, sortDirection, page, pageSize);
            return View(viewModel);
        }

        // GET: /Students/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student is null)
            {
                return NotFound();
            }
            return View(student);
        }

        // GET: /Students/Create
        public IActionResult Create()
        {
            return View(new StudentViewModel());
        }

        // POST: /Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF protection - required for every state-changing POST form.
        public async Task<IActionResult> Create(StudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dto = ToDto(model);
            var (success, created, error) = await _studentService.CreateAsync(dto);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Unable to create student.");
                return View(model);
            }

            TempData["SuccessMessage"] = $"Student '{created!.FirstName} {created.LastName}' was created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Students/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student is null)
            {
                return NotFound();
            }

            var model = new StudentViewModel
            {
                StudentId = student.StudentId,
                StudentNumber = student.StudentNumber,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                Course = student.Course,
                YearLevel = student.YearLevel,
                DateCreated = student.DateCreated,
                DateUpdated = student.DateUpdated
            };

            return View(model);
        }

        // POST: /Students/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentViewModel model)
        {
            if (id != model.StudentId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dto = ToDto(model);
            var (success, updated, error) = await _studentService.UpdateAsync(id, dto);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Unable to update student.");
                return View(model);
            }

            TempData["SuccessMessage"] = $"Student '{updated!.FirstName} {updated.LastName}' was updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Students/Delete/5  -> shows confirmation page
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student is null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: /Students/Delete/5 -> actually performs the (soft) delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = await _studentService.DeleteAsync(id);
            if (!deleted)
            {
                TempData["ErrorMessage"] = "Student could not be found or was already deleted.";
            }
            else
            {
                TempData["SuccessMessage"] = "Student was deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private static StudentCreateUpdateDto ToDto(StudentViewModel model) => new()
        {
            StudentNumber = model.StudentNumber,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            Course = model.Course,
            YearLevel = model.YearLevel
        };
    }
}
