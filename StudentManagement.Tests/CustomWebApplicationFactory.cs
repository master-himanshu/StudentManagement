using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudentManagement.Data;

namespace StudentManagement.Tests
{
    /// <summary>
    /// A specialized WebApplicationFactory that swaps the real SQL Server DbContext registration
    /// for an EF Core InMemory provider. This lets API integration tests exercise the *entire*
    /// HTTP pipeline (routing, model binding, filters, JSON serialization) without requiring
    /// an actual SQL Server instance to be running - ideal for CI pipelines and student laptops.
    /// </summary>
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        /// <summary>
        /// Unique per-factory-instance database name so parallel test classes don't collide.
        /// </summary>
        public string DatabaseName { get; } = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the production DbContextOptions<ApplicationDbContext> registration (SQL Server).
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor is not null)
                {
                    services.Remove(descriptor);
                }

                // Register an InMemory provider instead, scoped to this factory instance.
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(DatabaseName);
                });

                // Build the provider and ensure the (in-memory) database is created/clean.
                using var scope = services.BuildServiceProvider().CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
            });
        }
    }
}
