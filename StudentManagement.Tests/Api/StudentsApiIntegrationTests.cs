using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StudentManagement.ViewModels;
using Xunit;

namespace StudentManagement.Tests.Api
{
    /// <summary>
    /// Integration tests for the /api/students REST endpoints.
    /// Uses <see cref="CustomWebApplicationFactory"/> (InMemory EF Core provider) and a real
    /// in-process HttpClient, so these tests exercise the full ASP.NET Core pipeline end-to-end:
    /// routing -> model binding -> validation -> controller -> service -> repository -> JSON response.
    ///
    /// Implements IClassFixture so the factory (and its InMemory database) is shared across the
    /// tests in this class, while each test creates its own student data to stay independent.
    /// </summary>
    public class StudentsApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public StudentsApiIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        private static StudentCreateUpdateDto SampleCreateDto(string suffix) => new()
        {
            StudentNumber = $"TEST-{suffix}",
            FirstName = "Test",
            LastName = "Student",
            Email = $"test.student.{suffix}@example.edu",
            Course = "BS Computer Science",
            YearLevel = 1
        };

        [Fact]
        public async Task GET_All_ReturnsOkAndArray()
        {
            // Arrange - seed one student via the API itself.
            await _client.PostAsJsonAsync("/api/students", SampleCreateDto(Guid.NewGuid().ToString("N")[..8]));

            // Act
            var response = await _client.GetAsync("/api/students");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var students = await response.Content.ReadFromJsonAsync<List<StudentDto>>();
            students.Should().NotBeNull();
            students!.Should().NotBeEmpty();
        }

        [Fact]
        public async Task POST_WithValidData_ReturnsCreatedWithLocationHeader()
        {
            var dto = SampleCreateDto(Guid.NewGuid().ToString("N")[..8]);

            var response = await _client.PostAsJsonAsync("/api/students", dto);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();

            var created = await response.Content.ReadFromJsonAsync<StudentDto>();
            created.Should().NotBeNull();
            created!.StudentNumber.Should().Be(dto.StudentNumber);
            created.StudentId.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task POST_WithMissingRequiredFields_ReturnsBadRequest()
        {
            var invalidDto = new StudentCreateUpdateDto(); // everything empty/default -> fails validation

            var response = await _client.PostAsJsonAsync("/api/students", invalidDto);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task POST_WithDuplicateStudentNumber_ReturnsBadRequest()
        {
            var dto = SampleCreateDto(Guid.NewGuid().ToString("N")[..8]);
            await _client.PostAsJsonAsync("/api/students", dto);

            // Second attempt reuses the same StudentNumber but a different email.
            var duplicate = SampleCreateDto(Guid.NewGuid().ToString("N")[..8]);
            duplicate.StudentNumber = dto.StudentNumber;

            var response = await _client.PostAsJsonAsync("/api/students", duplicate);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GET_ById_WhenStudentExists_ReturnsOk()
        {
            var dto = SampleCreateDto(Guid.NewGuid().ToString("N")[..8]);
            var createResponse = await _client.PostAsJsonAsync("/api/students", dto);
            var created = await createResponse.Content.ReadFromJsonAsync<StudentDto>();

            var response = await _client.GetAsync($"/api/students/{created!.StudentId}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var fetched = await response.Content.ReadFromJsonAsync<StudentDto>();
            fetched!.StudentId.Should().Be(created.StudentId);
        }

        [Fact]
        public async Task GET_ById_WhenStudentDoesNotExist_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/students/999999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PUT_WithValidData_ReturnsOkAndUpdatesStudent()
        {
            var dto = SampleCreateDto(Guid.NewGuid().ToString("N")[..8]);
            var createResponse = await _client.PostAsJsonAsync("/api/students", dto);
            var created = await createResponse.Content.ReadFromJsonAsync<StudentDto>();

            var updateDto = SampleCreateDto(Guid.NewGuid().ToString("N")[..8]);
            updateDto.FirstName = "UpdatedFirstName";

            var response = await _client.PutAsJsonAsync($"/api/students/{created!.StudentId}", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var updated = await response.Content.ReadFromJsonAsync<StudentDto>();
            updated!.FirstName.Should().Be("UpdatedFirstName");
        }

        [Fact]
        public async Task PUT_WhenStudentDoesNotExist_ReturnsNotFound()
        {
            var updateDto = SampleCreateDto(Guid.NewGuid().ToString("N")[..8]);

            var response = await _client.PutAsJsonAsync("/api/students/999999", updateDto);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DELETE_WhenStudentExists_ReturnsNoContent()
        {
            var dto = SampleCreateDto(Guid.NewGuid().ToString("N")[..8]);
            var createResponse = await _client.PostAsJsonAsync("/api/students", dto);
            var created = await createResponse.Content.ReadFromJsonAsync<StudentDto>();

            var response = await _client.DeleteAsync($"/api/students/{created!.StudentId}");

            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Confirm it is no longer retrievable (soft-deleted).
            var getResponse = await _client.GetAsync($"/api/students/{created.StudentId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DELETE_WhenStudentDoesNotExist_ReturnsNotFound()
        {
            var response = await _client.DeleteAsync("/api/students/999999");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
