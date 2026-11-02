using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StudentManagement.Controllers;
using StudentManagement.Interfaces;
using StudentManagement.ViewModels;
using Xunit;

namespace StudentManagement.Tests.Controllers
{
    /// <summary>
    /// Unit tests for <see cref="StudentsController"/> (the MVC controller).
    /// The service layer is mocked so these tests verify controller behavior only:
    /// correct view selection, redirect targets, and ModelState handling.
    /// </summary>
    public class StudentsControllerTests
    {
        private readonly Mock<IStudentService> _serviceMock;
        private readonly StudentsController _sut;

        public StudentsControllerTests()
        {
            _serviceMock = new Mock<IStudentService>();
            var loggerMock = new Mock<ILogger<StudentsController>>();
            _sut = new StudentsController(_serviceMock.Object, loggerMock.Object);
        }

        private static StudentDto SampleDto(Guid? id = null) => new()
        {
            StudentId = id ?? Guid.NewGuid(),
            StudentNumber = "2026-00001",
            FirstName = "Juan",
            LastName = "Dela Cruz",
            Email = "juan@example.edu",
            Course = "BS Computer Science",
            YearLevel = 1,
            DateCreated = DateTime.UtcNow
        };

        [Fact]
        public async Task Index_ReturnsViewWithStudentIndexViewModel()
        {
            var viewModel = new StudentIndexViewModel { Students = new List<StudentManagement.Models.Student>() };
            _serviceMock.Setup(s => s.GetStudentListAsync(null, "LastName", "asc", 1, 10)).ReturnsAsync(viewModel);

            var result = await _sut.Index(null, "LastName", "asc", 1, 10);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeSameAs(viewModel);
        }

        [Fact]
        public async Task Details_WhenStudentExists_ReturnsViewWithModel()
        {
            var dto = SampleDto();
            _serviceMock.Setup(s => s.GetByIdAsync(dto.StudentId)).ReturnsAsync(dto);

            var result = await _sut.Details(dto.StudentId);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(dto);
        }

        [Fact]
        public async Task Details_WhenStudentDoesNotExist_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((StudentDto?)null);

            var result = await _sut.Details(Guid.NewGuid());

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Create_Post_WithInvalidModelState_ReturnsViewWithSameModel()
        {
            var model = new StudentViewModel();
            _sut.ModelState.AddModelError("FirstName", "Required");

            var result = await _sut.Create(model);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(model);
            _serviceMock.Verify(s => s.CreateAsync(It.IsAny<StudentCreateUpdateDto>()), Times.Never);
        }

        [Fact]
        public async Task Create_Post_WithValidModel_RedirectsToIndex()
        {
            var model = new StudentViewModel
            {
                StudentNumber = "2026-00099",
                FirstName = "New",
                LastName = "Student",
                Email = "new@example.edu",
                Course = "BSIT",
                YearLevel = 1
            };

            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<StudentCreateUpdateDto>()))
                .ReturnsAsync((true, SampleDto(), (string?)null));
            _sut.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new Microsoft.AspNetCore.Http.DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

            var result = await _sut.Create(model);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be(nameof(StudentsController.Index));
        }

        [Fact]
        public async Task Create_Post_WhenServiceReturnsError_ReturnsViewWithModelError()
        {
            var model = new StudentViewModel
            {
                StudentNumber = "2026-00001", // duplicate on purpose
                FirstName = "Juan",
                LastName = "Dela Cruz",
                Email = "juan@example.edu",
                Course = "BSCS",
                YearLevel = 1
            };

            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<StudentCreateUpdateDto>()))
                .ReturnsAsync((false, (StudentDto?)null, "Student number '2026-00001' is already in use."));

            var result = await _sut.Create(model);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            _sut.ModelState.IsValid.Should().BeFalse();
            viewResult.Model.Should().Be(model);
        }

        [Fact]
        public async Task DeleteConfirmed_WhenSuccessful_SetsSuccessMessageAndRedirects()
        {
            _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(true);
            _sut.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new Microsoft.AspNetCore.Http.DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

            var result = await _sut.DeleteConfirmed(Guid.NewGuid());

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be(nameof(StudentsController.Index));
            _sut.TempData["SuccessMessage"].Should().NotBeNull();
        }
    }
}
