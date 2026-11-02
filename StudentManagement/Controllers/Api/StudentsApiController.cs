using Microsoft.AspNetCore.Mvc;
using StudentManagement.Interfaces;
using StudentManagement.ViewModels;

namespace StudentManagement.Controllers.Api
{
    /// <summary>
    /// REST API for Student CRUD operations, consumed by non-browser clients (Postman, mobile apps, SPAs, etc).
    /// Returns pure JSON and proper HTTP status codes rather than Razor views.
    /// Routed at /api/students (see [Route] below), separate from the MVC /Students routes.
    /// </summary>
    [ApiController]
    [Route("api/students")]
    [Produces("application/json")]
    public class StudentsApiController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ILogger<StudentsApiController> _logger;

        public StudentsApiController(IStudentService studentService, ILogger<StudentsApiController> logger)
        {
            _studentService = studentService;
            _logger = logger;
        }

        /// <summary>GET /api/students - returns every active student.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<StudentDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetAll()
        {
            var students = await _studentService.GetAllAsync();
            return Ok(students);
        }

        /// <summary>GET /api/students/{id} - returns a single student, or 404 if it doesn't exist.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentDto>> GetById(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            if (student is null)
            {
                return NotFound(new { message = $"Student with id {id} was not found." });
            }
            return Ok(student);
        }

        /// <summary>POST /api/students - creates a new student. Returns 201 with a Location header.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(StudentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<StudentDto>> Create([FromBody] StudentCreateUpdateDto dto)
        {
            // [ApiController] automatically returns 400 for invalid ModelState before this line runs,
            // but we keep the check here too for clarity/teaching purposes.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, created, error) = await _studentService.CreateAsync(dto);
            if (!success)
            {
                return BadRequest(new { message = error });
            }

            return CreatedAtAction(nameof(GetById), new { id = created!.StudentId }, created);
        }

        /// <summary>PUT /api/students/{id} - fully updates an existing student.</summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StudentDto>> Update(int id, [FromBody] StudentCreateUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, updated, error) = await _studentService.UpdateAsync(id, dto);
            if (!success)
            {
                if (error == "Student not found.")
                {
                    return NotFound(new { message = error });
                }
                return BadRequest(new { message = error });
            }

            return Ok(updated);
        }

        /// <summary>DELETE /api/students/{id} - soft-deletes a student. Returns 204 on success.</summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _studentService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Student with id {id} was not found." });
            }
            return NoContent();
        }
    }
}
