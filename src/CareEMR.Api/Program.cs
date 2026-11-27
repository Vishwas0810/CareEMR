using CareEMR.Application.Patients;
using CareEMR.Infrastructure;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("CareEmr")
    ?? throw new InvalidOperationException("Connection string 'CareEmr' not found.");

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddScoped<PatientService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                 // OpenAPI JSON at /openapi/v1.json
    app.MapScalarApiReference(options => // Interactive docs at /scalar
    {
        options.Title = "CareEMR Patient Registry API";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
