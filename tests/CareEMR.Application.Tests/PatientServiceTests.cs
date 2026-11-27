using CareEMR.Application.Patients;

namespace CareEMR.Application.Tests;

// Fake test data only - never real patient information.
public class PatientServiceTests
{
    private readonly FakePatientRepository _repo = new();
    private readonly PatientService _service;

    public PatientServiceTests()
    {
        _service = new PatientService(_repo);
    }

    private static CreatePatientRequest CreateRequest(string? healthCard = "1234-567-890") =>
        new("Test", "Patient", new DateOnly(1985, 4, 12), healthCard, "ab", "555-0100", "test.patient@example.com");

    private static UpdatePatientRequest UpdateRequest(string? healthCard = "1234567890") =>
        new("Testy", "Updated", new DateOnly(1985, 4, 13), healthCard, "cd", "555-0199", "updated@example.com");

    // ---------- Create ----------

    [Fact]
    public async Task CreateAsync_SavesPatientAndReturnsCleanedData()
    {
        var result = await _service.CreateAsync(CreateRequest());

        Assert.Single(_repo.Patients);
        Assert.Equal(_repo.Patients[0].Id, result.Id);
        Assert.Equal("1234567890", result.HealthCardNumber);
        Assert.Equal("AB", result.HealthCardVersionCode);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateHealthCard_ThrowsAndDoesNotSave()
    {
        await _service.CreateAsync(CreateRequest("1234567890"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(CreateRequest("1234 567 890")));
        Assert.Single(_repo.Patients);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidData_ThrowsArgumentException()
    {
        var request = CreateRequest() with { FirstName = " " };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(request));
        Assert.Empty(_repo.Patients);
    }

    [Fact]
    public async Task CreateAsync_WithoutHealthCard_AllowsMultiplePatients()
    {
        await _service.CreateAsync(CreateRequest(healthCard: null));
        await _service.CreateAsync(CreateRequest(healthCard: null));

        Assert.Equal(2, _repo.Patients.Count);
    }

    // ---------- Read ----------

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        Assert.Null(await _service.GetByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ListAsync_ReturnsActivePatientsOnly()
    {
        var keep = await _service.CreateAsync(CreateRequest("1111111111"));
        var remove = await _service.CreateAsync(CreateRequest("2222222222"));
        await _service.DeleteAsync(remove.Id);

        var list = await _service.ListAsync();

        Assert.Equal(keep.Id, Assert.Single(list).Id);
    }

    // ---------- Update ----------

    [Fact]
    public async Task UpdateAsync_ChangesAllFieldsAndSaves()
    {
        var created = await _service.CreateAsync(CreateRequest());

        var updated = await _service.UpdateAsync(created.Id, UpdateRequest());

        Assert.NotNull(updated);
        Assert.Equal("Testy", updated.FirstName);
        Assert.Equal("Updated", updated.LastName);
        Assert.Equal(new DateOnly(1985, 4, 13), updated.DateOfBirth);
        Assert.Equal("CD", updated.HealthCardVersionCode);
        Assert.Equal("555-0199", updated.Phone);
        Assert.Equal(1, _repo.UpdateCalls);
    }

    [Fact]
    public async Task UpdateAsync_KeepingOwnHealthCard_IsNotADuplicate()
    {
        var created = await _service.CreateAsync(CreateRequest("1234567890"));

        var updated = await _service.UpdateAsync(created.Id, UpdateRequest("1234567890"));

        Assert.NotNull(updated);
    }

    [Fact]
    public async Task UpdateAsync_ToAnotherPatientsHealthCard_Throws()
    {
        await _service.CreateAsync(CreateRequest("1111111111"));
        var second = await _service.CreateAsync(CreateRequest("2222222222"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateAsync(second.Id, UpdateRequest("1111111111")));
        Assert.Equal(0, _repo.UpdateCalls);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidData_ThrowsAndDoesNotSave()
    {
        var created = await _service.CreateAsync(CreateRequest());
        var request = UpdateRequest() with { DateOfBirth = DateOnly.FromDateTime(DateTime.Today).AddDays(1) };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(created.Id, request));
        Assert.Equal(0, _repo.UpdateCalls);
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_ReturnsNull()
    {
        Assert.Null(await _service.UpdateAsync(Guid.NewGuid(), UpdateRequest()));
    }

    // ---------- Delete ----------

    [Fact]
    public async Task DeleteAsync_SoftDeletesPatient()
    {
        var created = await _service.CreateAsync(CreateRequest());

        var deleted = await _service.DeleteAsync(created.Id);

        Assert.True(deleted);
        Assert.False(_repo.Patients[0].IsActive);               // still stored...
        Assert.Null(await _service.GetByIdAsync(created.Id));   // ...but hidden
    }

    [Fact]
    public async Task DeleteAsync_UnknownOrAlreadyDeleted_ReturnsFalse()
    {
        var created = await _service.CreateAsync(CreateRequest());
        await _service.DeleteAsync(created.Id);

        Assert.False(await _service.DeleteAsync(created.Id));
        Assert.False(await _service.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateAsync_WithDeletedPatientsHealthCard_Throws()
    {
        var created = await _service.CreateAsync(CreateRequest("1234567890"));
        await _service.DeleteAsync(created.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(CreateRequest("1234567890")));
    }
}
