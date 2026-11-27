namespace CareEMR.Application.Patients;

public record CreatePatientRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string? HealthCardNumber,
    string? HealthCardVersionCode,
    string? Phone,
    string? Email);

public record PatientResponse(
    Guid Id,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string? HealthCardNumber,
    string? HealthCardVersionCode,
    string? Phone,
    string? Email);