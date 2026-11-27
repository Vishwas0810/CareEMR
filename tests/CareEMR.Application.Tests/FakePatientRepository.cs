using CareEMR.Application.Patients;
using CareEMR.Domain.Patients;

namespace CareEMR.Application.Tests;

// An in-memory stand-in for the real database repository.
// It behaves like PatientRepository: inactive patients are hidden from reads,
// but still count when checking for duplicate health cards.
public class FakePatientRepository : IPatientRepository
{
    public List<Patient> Patients { get; } = new();
    public int UpdateCalls { get; private set; }

    public Task AddAsync(Patient patient, CancellationToken ct = default)
    {
        Patients.Add(patient);
        return Task.CompletedTask;
    }

    public Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(Patients.FirstOrDefault(p => p.Id == id && p.IsActive));

    public Task<List<Patient>> ListAsync(CancellationToken ct = default) =>
        Task.FromResult(Patients.Where(p => p.IsActive)
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList());

    public Task UpdateAsync(Patient patient, CancellationToken ct = default)
    {
        UpdateCalls++;
        return Task.CompletedTask;
    }

    public Task<bool> HealthCardExistsAsync(string healthCardNumber, Guid? excludePatientId = null, CancellationToken ct = default) =>
        Task.FromResult(Patients.Any(p => p.HealthCardNumber == healthCardNumber && p.Id != excludePatientId));
}
