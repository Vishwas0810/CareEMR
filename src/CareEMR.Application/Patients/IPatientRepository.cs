using CareEMR.Domain.Patients;

namespace CareEMR.Application.Patients;

public interface IPatientRepository
{
    Task AddAsync(Patient patient, CancellationToken ct = default);
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Patient>> ListAsync(CancellationToken ct = default);
    Task<bool> HealthCardExistsAsync(string healthCardNumber, CancellationToken ct = default);
}