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

    public async Task UpdateAsync(Patient patient, CancellationToken ct = default)
    {
        // The patient was loaded with AsNoTracking, so attach it and mark it as changed.
        _db.Patients.Update(patient);
        await _db.SaveChangesAsync(ct);
    }

    // Includes soft-deleted patients, so a deleted record's health card can't be reused.
    public Task<bool> HealthCardExistsAsync(string healthCardNumber, Guid? excludePatientId = null, CancellationToken ct = default) =>
        _db.Patients.IgnoreQueryFilters()
            .AnyAsync(p => p.HealthCardNumber == healthCardNumber &&
                           (excludePatientId == null || p.Id != excludePatientId), ct);
}
