using Microsoft.EntityFrameworkCore;
using StudentManagement.Models;

namespace StudentManagement.Data
{
    /// <summary>
    /// The EF Core database context. Represents a session with the database and
    /// exposes DbSet properties that map to tables.
    /// Registered as Scoped in DI (the default lifetime for AddDbContext) - one instance per HTTP request.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("Students");

                entity.HasKey(s => s.StudentId);

                entity.Property(s => s.StudentNumber)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(s => s.FirstName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(s => s.LastName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(s => s.Email)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.Property(s => s.Course)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(s => s.YearLevel)
                      .IsRequired();

                entity.Property(s => s.DateCreated)
                      .IsRequired()
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(s => s.IsActive)
                      .IsRequired()
                      .HasDefaultValue(true);

                // Unique constraints enforced at the database level, mirroring the SQL script.
                entity.HasIndex(s => s.StudentNumber)
                      .IsUnique()
                      .HasDatabaseName("UQ_Students_StudentNumber");

                entity.HasIndex(s => s.Email)
                      .IsUnique()
                      .HasDatabaseName("UQ_Students_Email");

                // Non-unique index to speed up name-based searches/sorting, a common list-page pattern.
                entity.HasIndex(s => new { s.LastName, s.FirstName })
                      .HasDatabaseName("IX_Students_LastName_FirstName");

                entity.HasIndex(s => s.Course)
                      .HasDatabaseName("IX_Students_Course");
            });
        }
    }
}
