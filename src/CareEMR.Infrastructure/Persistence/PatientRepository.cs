using CareEMR.Application.Patients;
using CareEMR.Domain.Patients;
using Microsoft.EntityFrameworkCore;

namespace CareEMR.Infrastructure.Persistence;

public class PatientRepository : IPatientRepository
{
    private readonly CareEmrDbContext _db;

    public PatientRepository(CareEmrDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Patient patient, CancellationToken ct = default)
    {
        _db.Patients.Add(patient);
        await _db.SaveChangesAsync(ct);
    }

    public Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<List<Patient>> ListAsync(CancellationToken ct = default) =>
        _db.Patients.AsNoTracking()
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .ToListAsync(ct);

    public Task<bool> HealthCardExistsAsync(string healthCardNumber, CancellationToken ct = default) =>
        _db.Patients.AnyAsync(p => p.HealthCardNumber == healthCardNumber, ct);
}
