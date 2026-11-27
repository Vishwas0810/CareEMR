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
    public bool IsActive { get; private set; } = true;

    // Needed by the database layer (EF Core)
    private Patient() { }

    public Patient(string firstName, string lastName, DateOnly dateOfBirth)
    {
        UpdateDetails(firstName, lastName, dateOfBirth);
    }

    public void UpdateDetails(string firstName, string lastName, DateOnly dateOfBirth)
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

    // Soft delete: the record stays in the database but is hidden from the API.
    public void Deactivate() => IsActive = false;

    public void SetHealthCard(string? number, string? versionCode)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            HealthCardNumber = null;
            HealthCardVersionCode = null;
            return;
        }

        var digits = number.Replace(" ", "").Replace("-", "");
        if (digits.Length != 10 || !digits.All(char.IsDigit))
            throw new ArgumentException("Ontario health card number must be 10 digits.");

        var version = versionCode?.Trim().ToUpperInvariant();
        if (!string.IsNullOrEmpty(version) &&
            (version.Length > 2 || !version.All(char.IsLetter)))
            throw new ArgumentException("Version code must be 1-2 letters.");

        HealthCardNumber = digits;
        HealthCardVersionCode = string.IsNullOrEmpty(version) ? null : version;
    }

    public void UpdateContact(string? phone, string? email)
    {
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }
}