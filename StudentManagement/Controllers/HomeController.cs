using Microsoft.AspNetCore.Mvc;
using StudentManagement.Interfaces;
using StudentManagement.Models;

namespace StudentManagement.Controllers
{
    /// <summary>
    /// Serves the landing/dashboard page with a few high-level statistics,
    /// and the generic error page used by the exception handling pipeline.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IStudentService _studentService;

        public HomeController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        public async Task<IActionResult> Index()
        {
            var students = (await _studentService.GetAllAsync()).ToList();

            ViewBag.TotalStudents = students.Count;
            ViewBag.TotalCourses = students.Select(s => s.Course).Distinct().Count();
            ViewBag.YearLevelBreakdown = students
                .GroupBy(s => s.YearLevel)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Count());
            ViewBag.RecentStudents = students.OrderByDescending(s => s.DateCreated).Take(5).ToList();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
        }
    }
}
