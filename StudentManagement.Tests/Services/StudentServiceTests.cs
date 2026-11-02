using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using StudentManagement.Interfaces;
using StudentManagement.Models;
using StudentManagement.Services;
using StudentManagement.ViewModels;
using Xunit;

namespace StudentManagement.Tests.Services
{
    /// <summary>
    /// Unit tests for <see cref="StudentService"/>. The repository is mocked with Moq so these tests
    /// run entirely in memory, with no database dependency, and focus purely on business logic
    /// (uniqueness validation, mapping, orchestration).
    /// </summary>
    public class StudentServiceTests
    {
        private readonly Mock<IStudentRepository> _repositoryMock;
        private readonly StudentService _sut; // "system under test"

        public StudentServiceTests()
        {
            _repositoryMock = new Mock<IStudentRepository>();
            var loggerMock = new Mock<ILogger<StudentService>>();
            _sut = new StudentService(_repositoryMock.Object, loggerMock.Object);
        }

        private static Student CreateSampleStudent(Guid? id = null) => new()
        {
            StudentId = id ?? Guid.NewGuid(),
            StudentNumber = "2026-00001",
            FirstName = "Juan",
            LastName = "Dela Cruz",
            Email = "juan@example.edu",
            Course = "BS Computer Science",
            YearLevel = 1,
            DateCreated = DateTime.UtcNow,
            IsActive = true
        };

        private static StudentCreateUpdateDto CreateSampleDto() => new()
        {
            StudentNumber = "2026-00099",
            FirstName = "New",
            LastName = "Student",
            Email = "new.student@example.edu",
            Course = "BS Information Technology",
            YearLevel = 2
        };

        [Fact]
        public async Task GetByIdAsync_WhenStudentExists_ReturnsMappedDto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var student = CreateSampleStudent(id);
            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(student);

            // Act
            var result = await _sut.GetByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.StudentId.Should().Be(id);
            result.Email.Should().Be(student.Email);
        }

        [Fact]
        public async Task GetByIdAsync_WhenStudentDoesNotExist_ReturnsNull()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Student?)null);

            var result = await _sut.GetByIdAsync(Guid.NewGuid());

            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_WithUniqueStudentNumberAndEmail_CreatesStudent()
        {
            // Arrange
            var dto = CreateSampleDto();
            var createdId = Guid.NewGuid();
            _repositoryMock.Setup(r => r.StudentNumberExistsAsync(dto.StudentNumber, Guid.Empty)).ReturnsAsync(false);
            _repositoryMock.Setup(r => r.EmailExistsAsync(dto.Email, Guid.Empty)).ReturnsAsync(false);
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Student>()))
                .ReturnsAsync((Student s) => { s.StudentId = createdId; return s; });

            // Act
            var (success, created, error) = await _sut.CreateAsync(dto);

            // Assert
            success.Should().BeTrue();
            error.Should().BeNull();
            created.Should().NotBeNull();
            created!.StudentId.Should().Be(createdId);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Student>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateStudentNumber_ReturnsFailureAndDoesNotCallAdd()
        {
            // Arrange
            var dto = CreateSampleDto();
            _repositoryMock.Setup(r => r.StudentNumberExistsAsync(dto.StudentNumber, Guid.Empty)).ReturnsAsync(true);

            // Act
            var (success, created, error) = await _sut.CreateAsync(dto);

            // Assert
            success.Should().BeFalse();
            created.Should().BeNull();
            error.Should().Contain("already in use");
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Student>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateEmail_ReturnsFailure()
        {
            var dto = CreateSampleDto();
            _repositoryMock.Setup(r => r.StudentNumberExistsAsync(dto.StudentNumber, Guid.Empty)).ReturnsAsync(false);
            _repositoryMock.Setup(r => r.EmailExistsAsync(dto.Email, Guid.Empty)).ReturnsAsync(true);

            var (success, created, error) = await _sut.CreateAsync(dto);

            success.Should().BeFalse();
            created.Should().BeNull();
            error.Should().Contain("Email");
        }

        [Fact]
        public async Task UpdateAsync_WhenStudentNotFound_ReturnsFailure()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Student?)null);

            var (success, updated, error) = await _sut.UpdateAsync(Guid.NewGuid(), CreateSampleDto());

            success.Should().BeFalse();
            updated.Should().BeNull();
            error.Should().Be("Student not found.");
        }

        [Fact]
        public async Task UpdateAsync_WithValidData_UpdatesAndReturnsSuccess()
        {
            // Arrange
            var existing = CreateSampleStudent();
            var dto = CreateSampleDto();

            _repositoryMock.Setup(r => r.GetByIdAsync(existing.StudentId)).ReturnsAsync(existing);
            _repositoryMock.Setup(r => r.StudentNumberExistsAsync(dto.StudentNumber, existing.StudentId)).ReturnsAsync(false);
            _repositoryMock.Setup(r => r.EmailExistsAsync(dto.Email, existing.StudentId)).ReturnsAsync(false);
            _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Student>())).Returns(Task.CompletedTask);

            // Act
            var (success, updated, error) = await _sut.UpdateAsync(existing.StudentId, dto);

            // Assert
            success.Should().BeTrue();
            error.Should().BeNull();
            updated!.FirstName.Should().Be(dto.FirstName);
            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Student>(s => s.Email == dto.Email)), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WhenSuccessful_ReturnsTrue()
        {
            var id = Guid.NewGuid();
            _repositoryMock.Setup(r => r.DeleteAsync(id)).ReturnsAsync(true);

            var result = await _sut.DeleteAsync(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_WhenStudentNotFound_ReturnsFalse()
        {
            _repositoryMock.Setup(r => r.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            var result = await _sut.DeleteAsync(Guid.NewGuid());

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetStudentListAsync_NormalizesInvalidPagingValues()
        {
            _repositoryMock
                .Setup(r => r.GetPagedAsync(null, "LastName", "asc", 1, 10))
                .ReturnsAsync(new PagedResult<Student> { Items = new List<Student>(), TotalCount = 0 });

            // Pass invalid page/pageSize (0 and -5) and confirm they get normalized to 1 and 10.
            var result = await _sut.GetStudentListAsync(null, "LastName", "asc", 0, -5);

            result.CurrentPage.Should().Be(1);
            result.PageSize.Should().Be(10);
        }
    }
}
