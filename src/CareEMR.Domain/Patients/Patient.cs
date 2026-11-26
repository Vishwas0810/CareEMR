namespace CareEMR.Domain.Patients;

public class Patient
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly DateOfBirth { get; private set; }
    public string? HealthCardNumber { get; private set; }
    public string? HealthCardVersionCode { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    // Needed by the database layer (EF Core) later
    private Patient() { }

    public Patient(string firstName, string lastName, DateOnly dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.");
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("Date of birth cannot be in the future.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        DateOfBirth = dateOfBirth;
    }
}