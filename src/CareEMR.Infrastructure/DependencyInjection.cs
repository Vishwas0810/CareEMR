using CareEMR.Application.Patients;
using CareEMR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CareEMR.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CareEmrDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IPatientRepository, PatientRepository>();

        return services;
    }

    // Creates/updates the database schema by applying any pending EF Core migrations.
    // Called in Development (including docker-compose) so a fresh database is ready
    // without running "dotnet ef database update" by hand.
    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CareEmrDbContext>();
        await db.Database.MigrateAsync();
    }
}