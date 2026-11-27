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
}