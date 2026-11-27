using CareEMR.Application.Patients;
using Microsoft.AspNetCore.Mvc;

namespace CareEMR.Api.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
    private readonly PatientService _service;

    public PatientsController(PatientService service)
    {
        _service = service;
    }

    // POST /api/patients
    [HttpPost]
    public async Task<ActionResult<PatientResponse>> Create(CreatePatientRequest request, CancellationToken ct)
    {
        try
        {
            var created = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            // Invalid data (e.g. missing name, bad health card format)
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (InvalidOperationException ex)
        {
            // Duplicate health card number
            return Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
        }
    }

    // GET /api/patients
    [HttpGet]
    public async Task<ActionResult<List<PatientResponse>>> List(CancellationToken ct)
    {
        return await _service.ListAsync(ct);
    }

    // GET /api/patients/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PatientResponse>> GetById(Guid id, CancellationToken ct)
    {
        var patient = await _service.GetByIdAsync(id, ct);
        return patient is null ? NotFound() : patient;
    }
}
