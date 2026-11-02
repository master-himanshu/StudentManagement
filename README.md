# Student Management System — ASP.NET Core MVC + Web API (Educational Project)

A complete, production-style **CRUD web application** built with **ASP.NET Core MVC (.NET 8 LTS)**,
**Entity Framework Core**, and **SQL Server**, designed to teach enterprise .NET development
patterns (Repository Pattern, Service Layer, Dependency Injection, SOLID principles) in a
codebase simple enough for students to read end-to-end.

---

## 1. Project Overview

The app manages **Student** records (StudentId, StudentNumber, FirstName, LastName, Email,
Course, YearLevel, DateCreated, DateUpdated) through:

- A **Razor Views (MVC)** front end — dashboard, searchable/sortable/paginated list, create,
  edit, details, and delete-with-confirmation pages, styled with **Bootstrap 5**.
- A parallel **REST API** (`/api/students`) exposing the same CRUD operations as JSON, documented
  with **Swagger/OpenAPI** at `/swagger`.
- A **xUnit test suite** covering the service layer (unit tests with Moq), the MVC controller
  (unit tests with Moq), and the REST API (in-process integration tests with
  `Microsoft.AspNetCore.Mvc.Testing` + EF Core InMemory provider).

## 2. Features

| Area | Features |
|---|---|
| Create | Data-annotation validation, required fields, duplicate StudentNumber/Email checks, success alert |
| Read | Paginated table, free-text search (number/name/email/course), column sorting, live record count |
| Update | Server-side validation, duplicate checks excluding the current record, success alert |
| Delete | Confirmation page, **soft delete** (`IsActive` flag) so history is preserved |
| Dashboard | Total students, distinct courses, year-level breakdown, 5 most recently added students |
| API | Full REST CRUD with correct status codes (200/201/204/400/404), Swagger UI |
| Cross-cutting | Global exception-handling middleware, structured logging, antiforgery tokens, response caching disabled on error page |

## 3. Technology Stack

- ASP.NET Core MVC — **.NET 8 (LTS)**
- C# 12
- Entity Framework Core 8 (Code-First + Migrations)
- Microsoft SQL Server (LocalDB for local dev, full SQL Server/Azure SQL for production)
- Bootstrap 5 + Bootstrap Icons (via CDN)
- Swashbuckle (Swagger/OpenAPI)
- xUnit, Moq, FluentAssertions, `Microsoft.AspNetCore.Mvc.Testing`, EF Core InMemory

## 4. Architecture

```
                         ┌───────────────────────────┐
                         │         Browser            │
                         │  (Razor Views + Bootstrap) │
                         └─────────────┬─────────────┘
                                       │ HTTP
                         ┌─────────────▼─────────────┐
                         │   StudentsController (MVC) │
                         │   StudentsApiController    │──► Swagger / JSON clients
                         └─────────────┬─────────────┘
                                       │ depends on (interface)
                         ┌─────────────▼─────────────┐
                         │   IStudentService           │
                         │   StudentService (business  │
                         │   rules, DTO <-> Entity)    │
                         └─────────────┬─────────────┘
                                       │ depends on (interface)
                         ┌─────────────▼─────────────┐
                         │   IStudentRepository        │
                         │   StudentRepository (EF Core│
                         │   queries, soft delete)     │
                         └─────────────┬─────────────┘
                                       │
                         ┌─────────────▼─────────────┐
                         │  ApplicationDbContext (EF)  │
                         └─────────────┬─────────────┘
                                       │
                         ┌─────────────▼─────────────┐
                         │       SQL Server            │
                         │     dbo.Students table      │
                         └───────────────────────────┘
```

Each layer only depends on the **interface** of the layer below it (Dependency Inversion —
the "D" in SOLID), which is why the Service layer can be unit-tested with a mocked repository,
and the Controller can be unit-tested with a mocked service, with zero database involved.

### Folder Structure

```
StudentManagementSolution/
│   StudentManagement.sln
│   README.md
│
├───SQL/
│       StudentManagementDb_CreateAndSeed.sql
│
├───StudentManagement/                     <-- Main web application
│   │   Program.cs
│   │   appsettings.json
│   │   appsettings.Development.json
│   │   StudentManagement.csproj
│   │
│   ├───Controllers/
│   │   │   HomeController.cs
│   │   │   StudentsController.cs          <-- MVC (Razor views)
│   │   └───Api/
│   │           StudentsApiController.cs   <-- REST API (JSON)
│   │
│   ├───Data/
│   │   │   ApplicationDbContext.cs
│   │   └───Repositories/
│   │           StudentRepository.cs
│   │
│   ├───Interfaces/
│   │       IStudentRepository.cs
│   │       IStudentService.cs
│   │
│   ├───Services/
│   │       StudentService.cs
│   │
│   ├───Models/
│   │       Student.cs
│   │       ErrorViewModel.cs
│   │
│   ├───ViewModels/
│   │       StudentViewModel.cs
│   │       StudentIndexViewModel.cs
│   │       StudentDto.cs
│   │
│   ├───Middleware/
│   │       GlobalExceptionMiddleware.cs
│   │
│   ├───Migrations/
│   │       20260101000000_InitialCreate.cs
│   │       20260101000000_InitialCreate.Designer.cs
│   │       ApplicationDbContextModelSnapshot.cs
│   │
│   ├───Views/
│   │   ├───Home/Index.cshtml
│   │   ├───Students/ (Index, Create, Edit, Details, Delete).cshtml
│   │   └───Shared/ (_Layout, Error, _ValidationScriptsPartial).cshtml
│   │
│   └───wwwroot/css/site.css
│
└───StudentManagement.Tests/                <-- xUnit test project
    ├───Services/StudentServiceTests.cs
    ├───Controllers/StudentsControllerTests.cs
    ├───Api/StudentsApiIntegrationTests.cs
    └───CustomWebApplicationFactory.cs
```

## 5. Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, Developer, Express, or full edition) **or** Docker running `mssql`
- Visual Studio 2022 (17.8+) / VS Code / Rider (any will work)

## 6. Database Setup

You have **two equivalent options** — pick one.

### Option A — Run the raw SQL script (fastest, no EF tooling required)

1. Open the script at `SQL/StudentManagementDb_CreateAndSeed.sql` in SQL Server Management
   Studio, Azure Data Studio, or `sqlcmd`.
2. Execute it against your SQL Server instance. It will:
   - Create the `StudentManagementDb` database
   - Create the `dbo.Students` table with PK, CHECK, UNIQUE constraints
   - Create supporting indexes
   - Insert 22 sample student records
3. Update `appsettings.json` / `appsettings.Development.json` with your connection string if it
   differs from the default (`Server=localhost` or `(localdb)\mssqllocaldb`).

Using `sqlcmd`:
```bash
sqlcmd -S localhost -i SQL/StudentManagementDb_CreateAndSeed.sql
```

### Option B — Run EF Core Migrations (recommended if you plan to keep evolving the schema in C#)

```bash
cd StudentManagement
dotnet tool install --global dotnet-ef   # once, if not already installed
dotnet ef database update
```
This creates an empty `StudentManagementDb` with the `Students` table (no seed data — use
Option A's script, or insert your own test rows, if you also want sample data).

To create a **new** migration after changing the `Student` model:
```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## 7. Running the Application

```bash
cd StudentManagement
dotnet restore
dotnet build
dotnet run
```

Then browse to:
- **MVC app:** `https://localhost:5001` (Dashboard) or `https://localhost:5001/Students`
- **Swagger / REST API docs:** `https://localhost:5001/swagger`

Or press **F5** in Visual Studio with the `StudentManagement` project set as Startup Project.

## 8. Running Unit & Integration Tests

```bash
cd StudentManagement.Tests
dotnet test
```

This runs **all three test categories** in one pass:
- `Services/StudentServiceTests.cs` — business-logic unit tests (Moq)
- `Controllers/StudentsControllerTests.cs` — MVC controller unit tests (Moq)
- `Api/StudentsApiIntegrationTests.cs` — full-pipeline API integration tests (EF Core InMemory
  provider via `CustomWebApplicationFactory`, so **no live SQL Server is required to run tests**)

For a coverage report (requires the `coverlet.collector` package, already referenced):
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 9. REST API Reference

| Method | Route | Body | Success | Notes |
|---|---|---|---|---|
| GET | `/api/students` | – | 200 OK | Returns all active students |
| GET | `/api/students/{id}` | – | 200 OK / 404 Not Found | |
| POST | `/api/students` | `StudentCreateUpdateDto` | 201 Created (+ `Location` header) / 400 Bad Request | |
| PUT | `/api/students/{id}` | `StudentCreateUpdateDto` | 200 OK / 400 / 404 | |
| DELETE | `/api/students/{id}` | – | 204 No Content / 404 Not Found | Soft delete |

Example request body for POST/PUT:
```json
{
  "studentNumber": "2026-00099",
  "firstName": "New",
  "lastName": "Student",
  "email": "new.student@example.edu",
  "course": "BS Information Technology",
  "yearLevel": 2
}
```

## 10. NuGet Package List

**StudentManagement (main project):**
- Microsoft.EntityFrameworkCore.SqlServer 8.0.10
- Microsoft.EntityFrameworkCore.Design 8.0.10
- Microsoft.EntityFrameworkCore.Tools 8.0.10
- Swashbuckle.AspNetCore 6.6.2

**StudentManagement.Tests:**
- Microsoft.NET.Test.Sdk 17.11.1
- xunit 2.9.2 / xunit.runner.visualstudio 2.8.2
- Moq 4.20.72
- FluentAssertions 6.12.1
- Microsoft.AspNetCore.Mvc.Testing 8.0.10
- Microsoft.EntityFrameworkCore.InMemory 8.0.10
- coverlet.collector 6.0.2

## 11. Error Handling Summary

- **`GlobalExceptionMiddleware`** wraps the whole pipeline; unhandled exceptions are logged and
  converted into a JSON error payload for `/api/*` requests, or a redirect to `/Home/Error` for
  browser requests.
- **Model validation** (`[Required]`, `[StringLength]`, `[EmailAddress]`, `[Range]`) is enforced
  both client-side (jQuery Unobtrusive Validation) and server-side (`ModelState.IsValid`),
  and again at the database layer via `NOT NULL` / `CHECK` constraints.
- **404 handling**: `GetByIdAsync` returning `null` is translated to `NotFound()` in both the
  MVC and API controllers.
- **400 handling**: invalid `ModelState` in the API controller returns `BadRequest(ModelState)`;
  business-rule violations (duplicate email/number) also return `400` with a descriptive message.

## 12. Design Notes for Students

- **Why a Service layer on top of a Repository?** The Repository only knows about data access;
  the Service layer owns business rules (e.g., "student numbers must be unique") and DTO
  mapping. This keeps each class focused on one responsibility (SRP) and easy to unit test in
  isolation.
- **Why DTOs/ViewModels instead of binding directly to the EF entity?** It prevents
  over-posting attacks and decouples your public API/UI contract from your database schema, so
  either can evolve independently.
- **Why soft delete?** In real systems, hard-deleting rows destroys audit trails and can break
  foreign-key history. Flagging `IsActive = false` keeps the data recoverable while still
  hiding it from every "normal" query.

---

Happy coding! 🎓
