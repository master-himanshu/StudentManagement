using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.Data.Repositories;
using StudentManagement.Interfaces;
using StudentManagement.Middleware;
using StudentManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------------------
// SERVICE REGISTRATION (Dependency Injection container setup)
// ---------------------------------------------------------------------------------------

// MVC (controllers + Razor views) and Web API (controllers only) share the same registration.
builder.Services.AddControllersWithViews();

// Entity Framework Core + SQL Server. Connection string comes from appsettings.json / appsettings.Development.json.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repository Pattern: the concrete EF Core repository is registered against its interface.
// Scoped lifetime matches the DbContext's lifetime (one instance per HTTP request).
builder.Services.AddScoped<IStudentRepository, StudentRepository>();

// Service layer: business logic sits between controllers and repositories.
builder.Services.AddScoped<IStudentService, StudentService>();

// Swagger/OpenAPI - lets students explore and try the REST API from a browser at /swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Student Management API",
        Version = "v1",
        Description = "A simple educational REST API for managing student records."
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
// ---------------------------------------------------------------------------------------
// HTTP REQUEST PIPELINE (middleware order matters!)
// ---------------------------------------------------------------------------------------

// Our own global exception handler goes first so it can catch errors from everything after it.
app.UseGlobalExceptionHandling();

if (!app.Environment.IsDevelopment())
{
    // Built-in friendly error page for MVC views in production.
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Swagger UI is only exposed in Development for security/simplicity.
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Student Management API v1"));
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// MVC route: /{Controller}/{Action}/{id?} — serves the Razor views.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Attribute-routed API controllers (routes defined via [Route] on StudentsApiController).
app.MapControllers();

app.Run();

// Partial class declaration so the Integration Test project can reference Program
// via WebApplicationFactory<Program> (top-level statements otherwise generate an internal Program class).
public partial class Program { }
