using CareEMR.Domain.Patients;
using Microsoft.EntityFrameworkCore;

namespace CareEMR.Infrastructure.Persistence;

public class CareEmrDbContext : DbContext
{
    public CareEmrDbContext(DbContextOptions<CareEmrDbContext> options)
        : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>(patient =>
        {
            patient.ToTable("patients");
            patient.HasKey(p => p.Id);

            patient.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
            patient.Property(p => p.LastName).HasMaxLength(100).IsRequired();
            patient.Property(p => p.HealthCardNumber).HasMaxLength(10);
            patient.Property(p => p.HealthCardVersionCode).HasMaxLength(2);
            patient.Property(p => p.Phone).HasMaxLength(20);
            patient.Property(p => p.Email).HasMaxLength(254);

            patient.HasIndex(p => p.HealthCardNumber);

            // Soft delete: every query automatically skips inactive patients.
            // Use IgnoreQueryFilters() when you really need to see them.
            patient.HasQueryFilter(p => p.IsActive);
        });
    }
}