using CareEMR.Domain.Patients;

namespace CareEMR.Application.Patients;

public class PatientService
{
    private readonly IPatientRepository _repository;

    public PatientService(IPatientRepository repository)
    {
        _repository = repository;
    }

    public async Task<PatientResponse> CreateAsync(CreatePatientRequest request, CancellationToken ct = default)
    {
        // The domain entity validates its own data (throws ArgumentException if invalid).
        var patient = new Patient(request.FirstName, request.LastName, request.DateOfBirth);
        patient.SetHealthCard(request.HealthCardNumber, request.HealthCardVersionCode);
        patient.UpdateContact(request.Phone, request.Email);

        // Check using the cleaned-up number (spaces/dashes removed by SetHealthCard).
        if (patient.HealthCardNumber is not null &&
            await _repository.HealthCardExistsAsync(patient.HealthCardNumber, ct: ct))
        {
            throw new InvalidOperationException("A patient with this health card number already exists.");
        }

        await _repository.AddAsync(patient, ct);
        return ToResponse(patient);
    }

    // Returns null if the patient doesn't exist (or was deleted).
    public async Task<PatientResponse?> UpdateAsync(Guid id, UpdatePatientRequest request, CancellationToken ct = default)
    {
        var patient = await _repository.GetByIdAsync(id, ct);
        if (patient is null)
            return null;

        // Same domain validation as create.
        patient.UpdateDetails(request.FirstName, request.LastName, request.DateOfBirth);
        patient.SetHealthCard(request.HealthCardNumber, request.HealthCardVersionCode);
        patient.UpdateContact(request.Phone, request.Email);

        if (patient.HealthCardNumber is not null &&
            await _repository.HealthCardExistsAsync(patient.HealthCardNumber, excludePatientId: id, ct))
        {
            throw new InvalidOperationException("A patient with this health card number already exists.");
        }

        await _repository.UpdateAsync(patient, ct);
        return ToResponse(patient);
    }

    // Soft delete. Returns false if the patient doesn't exist (or was already deleted).
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var patient = await _repository.GetByIdAsync(id, ct);
        if (patient is null)
            return false;

        patient.Deactivate();
        await _repository.UpdateAsync(patient, ct);
        return true;
    }

    public async Task<PatientResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var patient = await _repository.GetByIdAsync(id, ct);
        return patient is null ? null : ToResponse(patient);
    }

    public async Task<List<PatientResponse>> ListAsync(CancellationToken ct = default)
    {
        var patients = await _repository.ListAsync(ct);
        return patients.Select(ToResponse).ToList();
    }

    private static PatientResponse ToResponse(Patient p) => new(
        p.Id, p.FirstName, p.LastName, p.DateOfBirth,
        p.HealthCardNumber, p.HealthCardVersionCode, p.Phone, p.Email);
}
