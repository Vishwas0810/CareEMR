using CareEMR.Domain.Patients;

namespace CareEMR.Domain.Tests;

// Fake test data only - never real patient information.
public class PatientTests
{
    private static readonly DateOnly ValidDob = new(1985, 4, 12);

    private static Patient NewPatient() => new("Test", "Patient", ValidDob);

    // ---------- Constructor ----------

    [Fact]
    public void Constructor_WithValidData_SetsAndTrimsNames()
    {
        var patient = new Patient("  Test ", " Patient  ", ValidDob);

        Assert.Equal("Test", patient.FirstName);
        Assert.Equal("Patient", patient.LastName);
        Assert.Equal(ValidDob, patient.DateOfBirth);
        Assert.NotEqual(Guid.Empty, patient.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithBlankFirstName_Throws(string? firstName)
    {
        Assert.Throws<ArgumentException>(() => new Patient(firstName!, "Patient", ValidDob));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithBlankLastName_Throws(string? lastName)
    {
        Assert.Throws<ArgumentException>(() => new Patient("Test", lastName!, ValidDob));
    }

    [Fact]
    public void Constructor_WithFutureDateOfBirth_Throws()
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.Today).AddDays(1);

        Assert.Throws<ArgumentException>(() => new Patient("Test", "Patient", tomorrow));
    }

    [Fact]
    public void Constructor_WithTodayAsDateOfBirth_IsAllowed()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var patient = new Patient("Baby", "Patient", today);

        Assert.Equal(today, patient.DateOfBirth);
    }

    [Fact]
    public void NewPatient_IsActive()
    {
        Assert.True(NewPatient().IsActive);
    }

    // ---------- UpdateDetails ----------

    [Fact]
    public void UpdateDetails_WithValidData_ChangesAndTrimsValues()
    {
        var patient = NewPatient();
        var newDob = new DateOnly(1990, 1, 1);

        patient.UpdateDetails(" Testy ", " Updated ", newDob);

        Assert.Equal("Testy", patient.FirstName);
        Assert.Equal("Updated", patient.LastName);
        Assert.Equal(newDob, patient.DateOfBirth);
    }

    [Theory]
    [InlineData("", "Patient")]
    [InlineData("Test", "  ")]
    public void UpdateDetails_WithBlankName_Throws(string firstName, string lastName)
    {
        var patient = NewPatient();

        Assert.Throws<ArgumentException>(() => patient.UpdateDetails(firstName, lastName, ValidDob));
        Assert.Equal("Test", patient.FirstName); // unchanged
    }

    [Fact]
    public void UpdateDetails_WithFutureDateOfBirth_Throws()
    {
        var patient = NewPatient();
        var tomorrow = DateOnly.FromDateTime(DateTime.Today).AddDays(1);

        Assert.Throws<ArgumentException>(() => patient.UpdateDetails("Test", "Patient", tomorrow));
        Assert.Equal(ValidDob, patient.DateOfBirth); // unchanged
    }

    // ---------- Deactivate ----------

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        var patient = NewPatient();

        patient.Deactivate();

        Assert.False(patient.IsActive);
    }

    // ---------- SetHealthCard ----------

    [Theory]
    [InlineData("1234567890")]
    [InlineData("1234-567-890")]
    [InlineData("1234 567 890")]
    [InlineData(" 1234 - 567 - 890 ")]
    public void SetHealthCard_StripsDashesAndSpaces(string input)
    {
        var patient = NewPatient();

        patient.SetHealthCard(input, null);

        Assert.Equal("1234567890", patient.HealthCardNumber);
    }

    [Theory]
    [InlineData("ab", "AB")]
    [InlineData(" k ", "K")]
    [InlineData("XY", "XY")]
    public void SetHealthCard_UppercasesAndTrimsVersionCode(string input, string expected)
    {
        var patient = NewPatient();

        patient.SetHealthCard("1234567890", input);

        Assert.Equal(expected, patient.HealthCardVersionCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetHealthCard_WithNoVersionCode_StoresNull(string? versionCode)
    {
        var patient = NewPatient();

        patient.SetHealthCard("1234567890", versionCode);

        Assert.Null(patient.HealthCardVersionCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetHealthCard_WithBlankNumber_ClearsHealthCard(string? number)
    {
        var patient = NewPatient();
        patient.SetHealthCard("1234567890", "AB");

        patient.SetHealthCard(number, "AB");

        Assert.Null(patient.HealthCardNumber);
        Assert.Null(patient.HealthCardVersionCode);
    }

    [Theory]
    [InlineData("12345")]          // too short
    [InlineData("12345678901")]    // too long
    [InlineData("123456789A")]     // contains a letter
    [InlineData("1234.567.890")]   // dots are not stripped
    public void SetHealthCard_WithInvalidNumber_Throws(string number)
    {
        var patient = NewPatient();

        Assert.Throws<ArgumentException>(() => patient.SetHealthCard(number, null));
    }

    [Theory]
    [InlineData("ABC")]  // too long
    [InlineData("A1")]   // contains a digit
    [InlineData("1")]    // digit only
    public void SetHealthCard_WithInvalidVersionCode_Throws(string versionCode)
    {
        var patient = NewPatient();

        Assert.Throws<ArgumentException>(() => patient.SetHealthCard("1234567890", versionCode));
    }

    [Fact]
    public void SetHealthCard_WhenInvalid_DoesNotChangeExistingCard()
    {
        var patient = NewPatient();
        patient.SetHealthCard("1234567890", "AB");

        Assert.Throws<ArgumentException>(() => patient.SetHealthCard("12345", "CD"));

        Assert.Equal("1234567890", patient.HealthCardNumber);
        Assert.Equal("AB", patient.HealthCardVersionCode);
    }

    // ---------- UpdateContact ----------

    [Fact]
    public void UpdateContact_TrimsValues()
    {
        var patient = NewPatient();

        patient.UpdateContact("  555-0100 ", " test.patient@example.com ");

        Assert.Equal("555-0100", patient.Phone);
        Assert.Equal("test.patient@example.com", patient.Email);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateContact_WithBlankValues_StoresNull(string? value)
    {
        var patient = NewPatient();
        patient.UpdateContact("555-0100", "test.patient@example.com");

        patient.UpdateContact(value, value);

        Assert.Null(patient.Phone);
        Assert.Null(patient.Email);
    }
}
