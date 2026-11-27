using CareEMR.Domain.Patients;

namespace CareEMR.Application.Patients;

public interface IPatientRepository
{
    Task AddAsync(Patient patient, CancellationToken ct = default);
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Patient>> ListAsync(CancellationToken ct = default);
    Task UpdateAsync(Patient patient, CancellationToken ct = default);

    // excludePatientId: ignore this patient (so updating a patient doesn't clash with itself).
    Task<bool> HealthCardExistsAsync(string healthCardNumber, Guid? excludePatientId = null, CancellationToken ct = default);
}